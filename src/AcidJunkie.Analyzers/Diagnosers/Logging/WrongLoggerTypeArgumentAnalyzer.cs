using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WrongLoggerTypeArgumentAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [..CommonRules.AllCommonRules, ..WrongLoggerTypeArgumentAnalyzerImplementation.DiagnosticRules.AllRules];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<WrongLoggerTypeArgumentAnalyzerImplementation>(a => a.AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeActionAndAnalyze<WrongLoggerTypeArgumentAnalyzerImplementation>(a => a.AnalyzeField, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeActionAndAnalyze<WrongLoggerTypeArgumentAnalyzerImplementation>(a => a.AnalyzeParameterList, SyntaxKind.ParameterList);
    }
}
