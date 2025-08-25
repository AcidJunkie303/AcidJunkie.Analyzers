using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0008;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;

// TODO: remove
#pragma warning disable

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation
    : SyntaxNodeAnalyzerImplementationBase<NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation>
{
    private readonly Aj0008Configuration _configuration;

    public NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = Aj0008ConfigurationProvider.Instance.GetConfiguration(context);
    }

    public void AnalyzeClassDeclaration()
    {
        if (!_configuration.IsEnabled)
        {
        }
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Standardization";
            public const string DiagnosticId = "AJ0008";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0008.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Non-nullable reference member not initialized/assigned";
            public static readonly LocalizableString MessageFormat = "The non-nullable reference type member `{0}` is either not checked for null or no value was assigned in all execution paths of {1}";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
