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

        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(a => a.AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(a => a.AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(a => a.AnalyzeImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);

#if CSHARP_12_OR_GREATER
        context.RegisterSyntaxNodeActionAndAnalyze<MissingEqualityComparerAnalyzerImplementation>(a => a.AnalyzeCollectionExpression, SyntaxKind.CollectionExpression);
#endif
    }
}
