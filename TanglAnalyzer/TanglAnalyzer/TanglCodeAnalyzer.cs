using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TanglAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TanglCodeAnalyzer : DiagnosticAnalyzer
    {
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        // Missing Target Type
        public const string MissingTargetTypeId = "MissingTanglTargetType";
        private static readonly LocalizableString MissingTargetTypeTitle = new LocalizableResourceString(nameof(Resources.MissingTargetTypeTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetTypeMessageFormat = new LocalizableResourceString(nameof(Resources.MissingTargetTypeMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetTypeDescription = new LocalizableResourceString(nameof(Resources.MissingTargetTypeDescription), Resources.ResourceManager, typeof(Resources));

        // Missing Target
        public const string MissingTargetId = "MissingTanglTarget";
        private static readonly LocalizableString MissingTargetTitle = new LocalizableResourceString(nameof(Resources.MissingTargetTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetMessageFormat = new LocalizableResourceString(nameof(Resources.MissingTargetMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingTargetDescription = new LocalizableResourceString(nameof(Resources.MissingTargetDescription), Resources.ResourceManager, typeof(Resources));

        // Differing Types
        public const string DifferingTypesId = "DifferingTanglTypes";
        private static readonly LocalizableString DifferingTypesTitle = new LocalizableResourceString(nameof(Resources.DifferingTypesTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString DifferingTypesMessageFormat = new LocalizableResourceString(nameof(Resources.DifferingTypesMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString DifferingTypesDescription = new LocalizableResourceString(nameof(Resources.DifferingTypesDescription), Resources.ResourceManager, typeof(Resources));

        // Missing Attributes
        public const string MissingAttributeId = "MissingAttribute";
        private static readonly LocalizableString MissingAttributeTitle = new LocalizableResourceString(nameof(Resources.MissingAttributeTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingAttributeMessageFormat = new LocalizableResourceString(nameof(Resources.MissingAttributeMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingAttributeDescription = new LocalizableResourceString(nameof(Resources.MissingAttributeDescription), Resources.ResourceManager, typeof(Resources));


        private const string Category = "Naming";

        private static DiagnosticDescriptor MissingTargetTypeRule = new DiagnosticDescriptor(
            MissingTargetTypeId,
            MissingTargetTypeTitle,
            MissingTargetTypeMessageFormat, 
            Category,
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: MissingTargetTypeDescription);

        private static DiagnosticDescriptor MissingTargetRule = new DiagnosticDescriptor(
            MissingTargetId,
            MissingTargetTitle,
            MissingTargetMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: MissingTargetDescription);

        private static DiagnosticDescriptor DifferingTypesRule = new DiagnosticDescriptor(
            DifferingTypesId,
            DifferingTypesTitle,
            DifferingTypesMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DifferingTypesDescription);

        private static DiagnosticDescriptor MissingAttributesRule = new DiagnosticDescriptor(
            MissingAttributeId,
            MissingAttributeTitle,
            MissingAttributeMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: MissingAttributeDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get 
            { return ImmutableArray.Create(
                MissingTargetTypeRule, 
                MissingTargetRule, 
                DifferingTypesRule,
                MissingAttributesRule); 
            } 
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
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
                var arg1 = tangl.ConstructorArguments.First();
                var arg2 = tangl.ConstructorArguments.Skip(1).FirstOrDefault();
                // The first argument has to be the name of the property this is entangled with

                var stronglyTyped = arg1.Value.GetType().Name.ToLower() != "string";
                //if (stronglyTyped)
                //{
                //    // Using constructor TanglAttribute(Type type, string propertyName, bool includeAttributes = true, string except = null)
                //}
                //else
                //{
                //    // Using constructor TanglAttribute(string target, bool includeAttributes = true, string except = null)
                //}
                var targetName = stronglyTyped ? $"{arg1.Value}.{arg2.Value}" : $"{arg1.Value}";
                if (string.IsNullOrWhiteSpace(targetName)) {
                    continue;
                }

                var includeAttributes = true;
                for (var ii = 1; ii < tangl.ConstructorArguments.Length; ii++)
                {
                    var arg = tangl.ConstructorArguments[ii];
                    if (arg.Value?.GetType().Name?.ToLower() == "boolean")
                    {
                        includeAttributes = (bool)arg.Value;
                    }
                }

                var except = "";
                for (var ii = 1 + (stronglyTyped ? 1 : 0); ii < tangl.ConstructorArguments.Length; ii++)
                {
                    var arg = tangl.ConstructorArguments[ii];
                    if (arg.Value?.GetType().Name?.ToLower() == "string")
                    {
                        except = (string)arg.Value;
                    }
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
                var targetType = (INamedTypeSymbol)target.Type;
                if (!targetType.IsGenericType && !SymbolEqualityComparer.Default.Equals(target.Type, propertySymbol.Type) || 
                    (targetType.IsGenericType && target.Type.ToDisplayString() != propertySymbol.Type.ToDisplayString()))
                {
                    var propertyBag = ImmutableDictionary<string, string>.Empty
                                .Add("TargetName", targetName)
                                .Add("TargetType", target.Type.ToString());
                    var diagnostic = Diagnostic.Create(DifferingTypesRule, propertySymbol.Locations[0], propertyBag, propertySymbol.ToString(), targetName);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                // Determine if tangl is missing attributes specified in the target
                var exceptions = except.Split(',').Where(s => !string.IsNullOrWhiteSpace(s));
                if (includeAttributes || exceptions.Any()) {
                    var propertyAttributes = propertySymbol.GetAttributes().Where(a => a.AttributeClass.Name != "TanglAttribute");
                    var targetAttributes = target.GetAttributes().Where(a => a.AttributeClass.Name != "TanglAttribute");
                    var missingAttributes = includeAttributes
                        ? targetAttributes.Where(t => propertyAttributes.All(a => a.AttributeClass.Name != t.AttributeClass.Name) && !exceptions.Contains(t.AttributeClass.Name))
                        : targetAttributes.Where(t => propertyAttributes.All(a => a.AttributeClass.Name != t.AttributeClass.Name) && exceptions.Contains(t.AttributeClass.Name));
                    if (missingAttributes.Any())
                    {
                        var attr = missingAttributes.First();
                        var propertyBag = ImmutableDictionary<string, string>.Empty
                                    .Add("AttributeName", attr.ToString());
                        var diagnostic = Diagnostic.Create(MissingAttributesRule, propertySymbol.Locations[0], propertyBag, attr.ToString());
                        context.ReportDiagnostic(diagnostic);
                        return;
                    }
                }


            }
        }

    }
}
