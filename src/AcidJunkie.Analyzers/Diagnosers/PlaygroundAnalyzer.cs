using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers;

#if DEBUG
#pragma warning disable RS1026 // Enable concurrent execution -> for easier debugging, we disable concurrent executions
#endif

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlaygroundAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var node = (InvocationExpressionSyntax)context.Node;

        var methodSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
        if (methodSymbol is null)
        {
            return;
        }

        var id = methodSymbol.GetDocumentationLikeId();

        Console.Write(id);

    }

}
