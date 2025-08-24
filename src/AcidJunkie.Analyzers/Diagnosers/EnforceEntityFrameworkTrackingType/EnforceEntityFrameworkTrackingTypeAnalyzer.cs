using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.EnforceEntityFrameworkTrackingType;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnforceEntityFrameworkTrackingTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => EnforceEntityFrameworkTrackingTypeAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();

        context.RegisterSyntaxNodeActionAndAnalyze<EnforceEntityFrameworkTrackingTypeAnalyzerImplementation>(a => a.AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
    }
}
