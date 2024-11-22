using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
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

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ILogger<TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzer> logger)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            logger.WriteLine(() => "Unable to get IMethodSymbol from invocation");
            return;
        }

        if (!methodSymbol.Name.EqualsOrdinal(nameof(Task.FromResult)))
        {
            logger.WriteLine(() => $"Method name is not {nameof(Task.FromResult)}");
            return;
        }

        if (!IsTaskType(methodSymbol.ContainingType))
        {
            logger.WriteLine(() => "Containing type is not Task or ValueTask");
            return;
        }

        if (!IsFromResultMethod(methodSymbol, out var taskType))
        {
            logger.WriteLine(() => $"Method name is not {nameof(Task.FromResult)}");
            return;
        }

        if (!taskType.IsEnumerable())
        {
            logger.WriteLine(() => "Task generic parameter type is not or does not implement IEnumerable or IEnumerable<T>");
            return;
        }

        var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (argument is null)
        {
            logger.WriteLine(() => "Invocation doesn't seem to have any arguments");
            return;
        }

        var firstNonCastExpression = argument.Expression.GetFirstNonCastExpression();

        var actualType = context.SemanticModel.GetTypeInfo(firstNonCastExpression, context.CancellationToken).Type;
        if (actualType is null)
        {
            logger.WriteLine(() => "Unable to determine the expression return type");
            return;
        }

        if (!actualType.DoesImplementWellKnownCollectionInterface())
        {
            logger.WriteLine(() => $"{actualType.GetFullName()} doesn't seem to be or implement a well-known collection interface");
            return;
        }

        logger.ReportDiagnostic(DiagnosticRules.Default.Rule, invocation.Expression.GetLocation());
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

    private static class DiagnosticRules
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
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
