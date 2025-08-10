using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingUsingStatement;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingUsingStatementAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [..CommonRules.AllCommonRules, ..MissingUsingStatementAnalyzerImplementation.DiagnosticRules.AllRules];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<MissingUsingStatementAnalyzerImplementation>(a => a.AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }
}
