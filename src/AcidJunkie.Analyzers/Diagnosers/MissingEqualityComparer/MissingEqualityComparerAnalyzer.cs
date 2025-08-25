using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingEqualityComparerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => MissingEqualityComparerAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();

        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(
            implementation => implementation.AnalyzeInvocation,
            syntaxKinds: SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(
            implementation => implementation.AnalyzeObjectCreation,
            syntaxKinds: SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(
            implementation => implementation.AnalyzeImplicitObjectCreation,
            syntaxKinds: SyntaxKind.ImplicitObjectCreationExpression);

#if CSHARP_12_OR_GREATER
        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(
            implementation => implementation.AnalyzeCollectionExpression,
            syntaxKinds: SyntaxKind.CollectionExpression);
#endif
    }
}
