using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ReturnMaterialisedCollectionAsEnumerable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnMaterialisedCollectionAsEnumerableAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [..CommonRules.AllCommonRules, ..ReturnMaterialisedCollectionAsEnumerableAnalyzerImplementation.DiagnosticRules.AllRules];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<ReturnMaterialisedCollectionAsEnumerableAnalyzerImplementation>(a => a.AnalyzeReturn, SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeActionAndAnalyze<ReturnMaterialisedCollectionAsEnumerableAnalyzerImplementation>(a => a.AnalyzeArrowExpression, SyntaxKind.ArrowExpressionClause);
    }
}
