using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.LambdaVariableHidesOuterLambdaVariable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LambdaVariableHidesOuterLambdaVariableAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => LambdaVariableHidesOuterLambdaVariableAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();

        context.RegisterSyntaxNodeActionAndAnalyze<LambdaVariableHidesOuterLambdaVariableAnalyzerImplementation>(a => a.AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }
}
