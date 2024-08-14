using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

#nullable enable
namespace AttributeDI.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeDIAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ADI-001";
        private const string _attName = "ServiceRegistrationAttribute";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString StaticAbstractRuleTitle = new LocalizableResourceString(nameof(Resources.StaticAbstractRuleTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString StaticAbstractRuleMessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString StaticAbstractRuleDescription = new LocalizableResourceString(nameof(Resources.StaticAbstractRuleDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor StaticAbstractRule = new(DiagnosticId, StaticAbstractRuleTitle, StaticAbstractRuleMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: StaticAbstractRuleDescription);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(StaticAbstractRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if ((namedTypeSymbol.TypeKind != TypeKind.Class && namedTypeSymbol.TypeKind != TypeKind.Struct)
                ||
                !TryGetSRAttribute(namedTypeSymbol, out AttributeData? attribute))
            {
                return;
            }

            if (IsAbstractOrStatic(namedTypeSymbol, in context))
            {
                return;
            }

            var constructors = namedTypeSymbol.Constructors;
            if (constructors.Length <= 0 || !constructors.Any(x => x.DeclaredAccessibility == Accessibility.Public))
            {

            }

            //// Find just those named type symbols with names containing lowercase letters.
            //if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            //{
            //    // For all such symbols, produce a diagnostic.
            //    var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

            //    context.ReportDiagnostic(diagnostic);
            //}
        }

        private static bool EqualsSRAttribute(AttributeData attribute)
        {
            return _attName.Equals(attribute.AttributeClass?.Name, StringComparison.Ordinal);
        }
        private static bool HasProperConstructor(INamedTypeSymbol symbol)
        {
            bool isPublic = symbol.DeclaredAccessibility == Accessibility.Public;
            var ctors = symbol.Constructors.Where(x => x.DeclaredAccessibility == Accessibility.Public
                                                       &&
                                                       !x.IsStatic);
            return true;
        }
        private static bool HasServiceRegistrationAttribute(INamedTypeSymbol symbol)
        {
            return symbol.GetAttributes().Any(EqualsSRAttribute);
        }
        private static bool IsAbstractOrStatic(INamedTypeSymbol symbol, in SymbolAnalysisContext context)
        {
            if (symbol.IsAbstract || symbol.IsStatic)
            {
                Diagnostic.Create(StaticAbstractRule, symbol.Locations[0], symbol.Name);
                return true;
            }

            return false;
        }
        private static bool IsStructAndServiceTypeIsNotInterface(AttributeData data, INamedTypeSymbol symbol, in SymbolAnalysisContext context)
        {
            if (symbol.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            var constructorArgs = data.ConstructorArguments;
            var namedArgs = data.NamedArguments;
            if (constructorArgs.Length == 0 && namedArgs.Length == 0)
            {

            }
        }
        private static bool TryGetSRAttribute(INamedTypeSymbol symbol, [NotNullWhen(true)] out AttributeData? attribute)
        {
            ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
            attribute = attributes.FirstOrDefault(EqualsSRAttribute);

            return attribute is not null;
        }
    }
}
