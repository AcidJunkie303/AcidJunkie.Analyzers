using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ReturnMaterializedCollectionAsEnumerable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnMaterializedCollectionAsEnumerableAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation>(a => a.AnalyzeReturn, SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeActionAndAnalyze<ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation>(a => a.AnalyzeArrowExpression, SyntaxKind.ArrowExpressionClause);
    }
}
