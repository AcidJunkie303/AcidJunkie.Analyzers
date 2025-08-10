using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterialisedCollectionAsEnumerable;

internal sealed class TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzerImplementation>
{
    public TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzerImplementation(SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeInvocation()
    {
        var invocation = (InvocationExpressionSyntax)Context.Node;

        if (Context.SemanticModel.GetSymbolInfo(invocation, Context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            Logger.WriteLine(() => "Unable to get IMethodSymbol from invocation");
            return;
        }

        if (!methodSymbol.Name.EqualsOrdinal(nameof(Task.FromResult)))
        {
            Logger.WriteLine(() => $"Method name is not {nameof(Task.FromResult)}");
            return;
        }

        if (!IsTaskType(methodSymbol.ContainingType))
        {
            Logger.WriteLine(() => "Containing type is not Task or ValueTask");
            return;
        }

        if (!IsFromResultMethod(methodSymbol, out var taskType))
        {
            Logger.WriteLine(() => $"Method name is not {nameof(Task.FromResult)}");
            return;
        }

        if (!taskType.IsEnumerable())
        {
            Logger.WriteLine(() => "Task generic parameter type is not or does not implement IEnumerable or IEnumerable<T>");
            return;
        }

        var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (argument is null)
        {
            Logger.WriteLine(() => "Invocation doesn't seem to have any arguments");
            return;
        }

        var firstNonCastExpression = argument.Expression.GetFirstNonCastExpression();

        var actualType = Context.SemanticModel.GetTypeInfo(firstNonCastExpression, Context.CancellationToken).Type;
        if (actualType is null)
        {
            Logger.WriteLine(() => "Unable to determine the expression return type");
            return;
        }

        if (!actualType.DoesImplementWellKnownCollectionInterface())
        {
            Logger.WriteLine(() => $"{actualType.GetFullName()} doesn't seem to be or implement a well-known collection interface");
            return;
        }

        Logger.ReportDiagnostic2(DiagnosticRules.Default.Rule, invocation.Expression.GetLocation());
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, invocation.Expression.GetLocation()));
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
        internal static ImmutableArray<DiagnosticDescriptor> AllRules { get; } = [Default.Rule];

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
