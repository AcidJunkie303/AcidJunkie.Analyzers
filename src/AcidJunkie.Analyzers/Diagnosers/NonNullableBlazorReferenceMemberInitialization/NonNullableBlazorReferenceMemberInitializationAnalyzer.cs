using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonNullableBlazorReferenceMemberInitializationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation>(
            implementation => implementation.AnalyzeClassDeclaration,
            SyntaxKind.ClassDeclaration);
    }
}
