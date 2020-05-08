using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tangl
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TanglAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Tangl";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;
            // Find TanglAttribute associated with property
            var tangls = propertySymbol.GetAttributes().Where(a => a.AttributeClass.Name == "TanglAttribute");
            // Pull all arguments from the attribute constructor
            foreach (var tangl in tangls.Where(t => t.ConstructorArguments.Any()))
            {
                // The first argument has to be the name of the property this is entangled with
                var targetName = tangl.ConstructorArguments.First().Value.ToString();
                if (string.IsNullOrWhiteSpace(targetName)) {
                    continue;
                }
                var pos = targetName.LastIndexOf('.');
                var typeName = targetName.Substring(0, pos);
                var propertyName = targetName.Substring(pos + 1, targetName.Length - pos - 1);
                var targetClass = context.Compilation.GetTypeByMetadataName(typeName);
                //var targetSymbols = context.Compilation.GetSymbolsWithName(propertyName);
                if (targetClass == null)
                {
                    // raise missing target type 
                }
                var target = (IPropertySymbol)targetClass.GetMembers().FirstOrDefault(m => m.Name == propertyName);
                if (target == null)
                {
                    // raise missing target property 
                }
                if (target.Type != propertySymbol.Type )
                {
                    // raise differing type warning
                }
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
