using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace TanglAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TanglCodeFixProvider)), Shared]
    public class TanglCodeFixProvider : CodeFixProvider
    {
        private const string title = "Change type to match target";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TanglCodeAnalyzer.DifferingTypesId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            //var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

            if (diagnostic.Id == TanglCodeAnalyzer.DifferingAttributeId)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => UpdateAttribute(diagnostic, context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
            else
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => UpdateType(diagnostic, context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private async Task<Document> UpdateType(Diagnostic diagnostic, Document document, PropertyDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            //var identifierToken = typeDecl.Identifier;
            //var newName = identifierToken.Text.ToUpperInvariant();
            var targetName = diagnostic.Properties["TargetName"];
            var fullTargetType = diagnostic.Properties["TargetType"];
            var genericStart = fullTargetType.IndexOf('<');
            var targetType = genericStart > 1 ?
                fullTargetType.Substring(0, genericStart).Split('.').Last() 
                    + fullTargetType.Substring(genericStart) :
                fullTargetType.Split('.').Last();
            
            //var typeToken = typeDecl.Type.Keyword;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var targetSymbol = semanticModel.Compilation.GetTypeByMetadataName(targetName);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);
            //var targetTypeAsString = typeSymbol.ToDisplayString();
            var predefinedType = typeDecl.Type;
            var firstToken = predefinedType.GetFirstToken();
            var leadingTrivia = firstToken.LeadingTrivia;
            var trailingTrivia = firstToken.TrailingTrivia;
            var typeToken = SyntaxFactory.PredefinedType(SyntaxFactory.Token(leadingTrivia, SyntaxKind.IntKeyword, trailingTrivia));
            //var typeToken = SyntaxFactory.PredefinedType(SyntaxFactory.Token(leadingTrivia, targetSymbol.Syn, trailingTrivia));
            //typeDecl.WithType(SyntaxFactory.Identifier.UpdateType()
            var identifierTypeToken = SyntaxFactory.Identifier(leadingTrivia, targetType, trailingTrivia);
            var identifierNode = SyntaxFactory.IdentifierName(identifierTypeToken);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(predefinedType, identifierNode);

            return document.WithSyntaxRoot(newRoot);

            //var identifierToken = typeDecl.Token;
            //var updatedText = identifierToken.Text.Replace("myword", "anotherword");
            //var valueText = identifierToken.ValueText.Replace("myword", "anotherword");
            //var newToken = SyntaxFactory.Literal(identifierTok(en.LeadingTrivia, updatedText, valueText, identifierToken.TrailingTrivia);

            //var sourceText = await typeDecl.SyntaxTree.GetTextAsync(cancellationToken);
            //// update document by changing the source text
            //return document.WithText(sourceText.WithChanges(new TextChange(identifierToken.FullSpan, newToken.ToFullString())));
        }

        private async Task<Document> UpdateAttribute(Diagnostic diagnostic, Document document, PropertyDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var attributeToReplace = diagnostic.Properties["AttributeToReplace"];
            var attributeReplacement = diagnostic.Properties["AttributeReplacement"];

            var currentText = typeDecl.ToFullString();
            var newText = currentText.Replace(attributeToReplace, attributeReplacement);

            //// update document by changing the source text
            var sourceText = await typeDecl.SyntaxTree.GetTextAsync(cancellationToken);
            return document.WithText(sourceText.WithChanges(new TextChange(typeDecl.FullSpan, newText)));
        }
    }

    //    private IPropertySymbol GetTarget(SemanticModel semanticModel, IPropertySymbol propertySymbol)
    //    {
    //        var tangls = propertySymbol.GetAttributes().Where(a => a.AttributeClass.Name == "TanglAttribute");
    //        // Pull all arguments from the attribute constructor
    //        foreach (var tangl in tangls.Where(t => t.ConstructorArguments.Any()))
    //        {
    //            // The first argument has to be the name of the property this is entangled with
    //            var targetName = tangl.ConstructorArguments.First().Value.ToString();
    //            if (string.IsNullOrWhiteSpace(targetName))
    //            {
    //                continue;
    //            }
    //            var pos = targetName.LastIndexOf('.');
    //            var typeName = targetName.Substring(0, pos);
    //            var propertyName = targetName.Substring(pos + 1, targetName.Length - pos - 1);
    //            var targetClass = semanticModel.Compilation.GetTypeByMetadataName(typeName);
    //            if (targetClass != null)
    //            {
    //                var target = (IPropertySymbol)targetClass.GetMembers().FirstOrDefault(m => m.Name == propertyName);
    //                return target;
    //            }
    //        }
    //        return null;
    //    }
    //}
}
