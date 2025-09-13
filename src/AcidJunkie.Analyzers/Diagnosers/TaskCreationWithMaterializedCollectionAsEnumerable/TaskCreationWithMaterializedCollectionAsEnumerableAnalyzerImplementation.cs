using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterializedCollectionAsEnumerable;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerImplementation>
{
    private readonly IAnalyzerConfiguration _configuration;

    public TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeInvocation()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var invocation = (InvocationExpressionSyntax)Context.Node;

        if (Context.SemanticModel.GetSymbolInfo(invocation, Context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to get IMethodSymbol from invocation");
            return;
        }

        if (!methodSymbol.Name.EqualsOrdinal(nameof(Task.FromResult)))
        {
            Logger.WriteLine(LogLevel.Full, $"Method name is not {nameof(Task.FromResult)}");
            return;
        }

        if (!IsTaskType(methodSymbol.ContainingType))
        {
            Logger.WriteLine(LogLevel.Full, "Containing type is not Task or ValueTask");
            return;
        }

        if (!IsFromResultMethod(methodSymbol, out var taskType))
        {
            Logger.WriteLine(LogLevel.Full, $"Method name is not {nameof(Task.FromResult)}");
            return;
        }

        if (!taskType.IsEnumerable())
        {
            Logger.WriteLine(LogLevel.Full, "Task generic parameter type is not or does not implement IEnumerable or IEnumerable<T>");
            return;
        }

        var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (argument is null)
        {
            Logger.WriteLine(LogLevel.Full, "Invocation doesn't seem to have any arguments");
            return;
        }

        var firstNonCastExpression = argument.Expression.GetFirstNonCastExpression();

        var actualType = Context.SemanticModel.GetTypeInfo(firstNonCastExpression, Context.CancellationToken).Type;
        if (actualType is null)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to determine the expression return type");
            return;
        }

        if (!actualType.DoesImplementWellKnownCollectionInterface())
        {
            Logger.WriteLine(LogLevel.Full, $"{actualType.GetFullName()} doesn't seem to be or implement a well-known collection interface");
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, invocation.Expression.GetLocation());
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
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Performance";
            public const string DiagnosticId = "AJ0004";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Do not create tasks of enumerable type containing a materialized collection";
            public static readonly LocalizableString MessageFormat = "Do not create tasks of type IEnumerable or IEnumerable<T> containing a materialized collection";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
