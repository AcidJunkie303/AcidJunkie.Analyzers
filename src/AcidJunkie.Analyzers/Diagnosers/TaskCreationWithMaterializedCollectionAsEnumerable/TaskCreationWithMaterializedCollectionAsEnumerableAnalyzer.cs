using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterializedCollectionAsEnumerable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TaskCreationWithMaterializedCollectionAsEnumerableAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerImplementation>(a => a.AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }
}
