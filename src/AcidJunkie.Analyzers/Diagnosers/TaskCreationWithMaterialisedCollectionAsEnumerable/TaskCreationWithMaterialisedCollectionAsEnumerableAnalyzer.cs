using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterialisedCollectionAsEnumerable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndCheck<TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzer>(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (!methodSymbol.Name.EqualsOrdinal(nameof(Task.FromResult)))
        {
            return;
        }

        if (!IsTaskType(methodSymbol.ContainingType))
        {
            return;
        }

        if (!IsFromResultMethod(methodSymbol, out var taskType))
        {
            return;
        }

        if (!taskType.IsEnumerable())
        {
            return;
        }

        var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (argument is null)
        {
            return;
        }

        var firstNonCastExpression = argument.Expression.GetFirstNonCastExpression();
        var actualType = context.SemanticModel.GetTypeInfo(firstNonCastExpression, context.CancellationToken).Type;
        if (actualType is null)
        {
            return;
        }

        if (!actualType.DoesImplementWellKnownCollectionInterface())
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, invocation.Expression.GetLocation()));
    }

    private static bool IsFromResultMethod(IMethodSymbol methodSymbol, [NotNullWhen(true)] out ITypeSymbol? taskType)
    {
        taskType = null;
        if (methodSymbol.Arity != 1)
        {
            return false;
        }

        if (!methodSymbol.Name.EqualsOrdinal(nameof(Task.FromResult)))
        {
            return false;
        }

        taskType = methodSymbol.TypeArguments[0];
        return true;
    }

    private static bool IsTaskType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        if (namedTypeSymbol.Arity != 0) // either it's Task or Task`1
        {
            return false;
        }

        return typeSymbol.Name.EqualsOrdinal("Task") || typeSymbol.Name.EqualsOrdinal("ValueTask");
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            private const string Category = "Performance";
            public const string DiagnosticId = "AJ0004";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0004.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Do not create tasks of enumerable type containing a materialised collection";
            public static readonly LocalizableString MessageFormat = "Do not create tasks of type IEnumerable or IEnumerable<T> containing a materialised collection";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
        }
    }
}
