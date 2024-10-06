using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
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
        context.RegisterSyntaxNodeAction(AnalyzeReturn, SyntaxKind.PragmaWarningDirectiveTrivia);
    }

    private static void AnalyzeReturn(SyntaxNodeAnalysisContext context)
    {
        var directive = (PragmaWarningDirectiveTriviaSyntax)context.Node;
        if (directive.ErrorCodes.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, directive.GetLocation()));
        }
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            public const string Category = "Design";
            public const string DiagnosticId = "AJ0005";

            public static readonly LocalizableString Title = "Do not use general warning suppression";
            public static readonly LocalizableString MessageFormat = Title;
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        }
    }
}
