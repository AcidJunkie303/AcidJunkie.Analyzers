using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ExtensionClassName;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class ExtensionClassNameAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<ExtensionClassNameAnalyzer>
{
    private readonly IAnalyzerConfiguration _configuration;

    public ExtensionClassNameAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeClassDeclaration()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var classDeclaration = (ClassDeclarationSyntax)Context.Node;
        var className = classDeclaration.Identifier.Text;
        var endsWithExtensions = className.EndsWith("Extensions", StringComparison.Ordinal);
        if (endsWithExtensions)
        {
            return;
        }

        if (!ContainsOldStyleExtensionMethods(classDeclaration))
        {
            return;
        }

        var suggestedClassName = $"{className}Extensions";

        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, classDeclaration.Identifier.GetLocation(), className, suggestedClassName));
    }

    private static bool ContainsOldStyleExtensionMethods(ClassDeclarationSyntax classDeclaration) // "old" means "pre .net 10.0"
        => classDeclaration
          .Members.OfType<MethodDeclarationSyntax>()
          .Any(a => a.IsStatic() && a.ParameterList.Parameters.Count > 0 && a.ParameterList.Parameters[0].HasThisKeyword());

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Intention";
            public const string DiagnosticId = "AJ0006";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0006.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Classes containing extension methods should have an `Extensions` suffix";
            public static readonly LocalizableString MessageFormat = "Rename the class `{0}` to `{1}` to indicate it contains extension methods";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
