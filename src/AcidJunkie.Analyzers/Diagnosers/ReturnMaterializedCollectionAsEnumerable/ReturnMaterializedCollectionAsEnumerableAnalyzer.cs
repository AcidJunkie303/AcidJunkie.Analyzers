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
        context.RegisterSyntaxNodeActionAndAnalyze<ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation>(
            implementation => implementation.AnalyzeReturn,
            syntaxKinds: SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeActionAndAnalyze<ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation>(
            implementation => implementation.AnalyzeArrowExpression,
            syntaxKinds: SyntaxKind.ArrowExpressionClause);
    }
}
