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

        // Missing Target Type
        private static readonly LocalizableString MissingTargetTypeTitle = new LocalizableResourceString(nameof(Resources.MissingTargetTypeTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetTypeMessageFormat = new LocalizableResourceString(nameof(Resources.MissingTargetTypeMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetTypeDescription = new LocalizableResourceString(nameof(Resources.MissingTargetTypeDescription), Resources.ResourceManager, typeof(Resources));

        // Missing Target
        private static readonly LocalizableString MissingTargetTitle = new LocalizableResourceString(nameof(Resources.MissingTargetTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetMessageFormat = new LocalizableResourceString(nameof(Resources.MissingTargetMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetDescription = new LocalizableResourceString(nameof(Resources.MissingTargetDescription), Resources.ResourceManager, typeof(Resources));
        
        // Differing Types
        private static readonly LocalizableString DifferingTypesTitle = new LocalizableResourceString(nameof(Resources.DifferingTypesTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString DifferingTypesMessageFormat = new LocalizableResourceString(nameof(Resources.DifferingTypesMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString DifferingTypesDescription = new LocalizableResourceString(nameof(Resources.DifferingTypesDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "Naming";

        private static DiagnosticDescriptor MissingTargetTypeRule = new DiagnosticDescriptor(
            DiagnosticId,
            MissingTargetTypeTitle,
            MissingTargetTypeMessageFormat, 
            Category,
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: MissingTargetTypeDescription);

        private static DiagnosticDescriptor MissingTargetRule = new DiagnosticDescriptor(
            DiagnosticId,
            MissingTargetTitle,
            MissingTargetMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: MissingTargetDescription);

        private static DiagnosticDescriptor DifferingTypesRule = new DiagnosticDescriptor(
            DiagnosticId,
            DifferingTypesTitle,
            DifferingTypesMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DifferingTypesDescription);

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule, MissingTargetTypeRule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
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
                    var diagnostic = Diagnostic.Create(MissingTargetTypeRule, propertySymbol.Locations[0], typeName);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                var target = (IPropertySymbol)targetClass.GetMembers().FirstOrDefault(m => m.Name == propertyName);
                if (target == null)
                {
                    var diagnostic = Diagnostic.Create(MissingTargetRule, propertySymbol.Locations[0], targetName);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                if (target.Type != propertySymbol.Type )
                {
                    var diagnostic = Diagnostic.Create(DifferingTypesRule, propertySymbol.Locations[0], propertySymbol.ToString(), targetName);
                    context.ReportDiagnostic(diagnostic);
                    return;
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
