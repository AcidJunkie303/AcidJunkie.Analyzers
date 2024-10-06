using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.WarningSuppression;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GeneralWarningSuppressionAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndCheck<GeneralWarningSuppressionAnalyzer>(AnalyzeReturn, SyntaxKind.PragmaWarningDirectiveTrivia);
    }

    private static void AnalyzeReturn(SyntaxNodeAnalysisContext context, ILogger<GeneralWarningSuppressionAnalyzer> logger)
    {
        var directive = (PragmaWarningDirectiveTriviaSyntax)context.Node;
        if (directive.ErrorCodes.Count == 0)
        {
            logger.LogReportDiagnostic(DiagnosticRules.Default.Rule, directive.GetLocation());
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, directive.GetLocation()));
        }
    }

    internal static class DiagnosticRules
    {
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
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
        }
    }
}
