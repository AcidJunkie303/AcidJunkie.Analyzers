using System.Collections.Immutable;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.WarningSuppression;

internal sealed class GeneralWarningSuppressionAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<GeneralWarningSuppressionAnalyzerImplementation>
{
    public GeneralWarningSuppressionAnalyzerImplementation(SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzePragma()
    {
        var directive = (PragmaWarningDirectiveTriviaSyntax)Context.Node;
        if (directive.ErrorCodes.Count == 0)
        {
            Logger.ReportDiagnostic2(DiagnosticRules.Default.Rule, directive.GetLocation());
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, directive.GetLocation()));
        }
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> AllRules { get; } = [Default.Rule];

        internal static class Default
        {
            private const string Category = "Code Smell";
            public const string DiagnosticId = "AJ0005";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0005.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Do not use general warning suppression";
            public static readonly LocalizableString MessageFormat = Title;
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);
        }
    }
}
