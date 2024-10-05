using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ReturnMaterialisedCollectionAsEnumerable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnMaterialisedCollectionAsEnumerableAnalyzer : DiagnosticAnalyzer
{
    private static readonly Dictionary<string, Dictionary<string, int>> ArityByTypeByNamespace = new(StringComparer.Ordinal)
    {
        {
            "System.Collections", new(StringComparer.Ordinal)
            {
                { "ICollection", 0 },
                { "IDictionary", 0 },
                { "IList", 0 }
            }
        },
        {
            "System.Collections.Generic", new(StringComparer.Ordinal)
            {
                { "ICollection", 1 },
                { "IDictionary", 1 },
                { "IList", 1 },
                { "ISet", 1 },
                { "IReadOnlyCollection", 1 }
            }
        }
    };

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeAction(AnalyzeReturn, SyntaxKind.ReturnStatement);
    }

    private static void AnalyzeReturn(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;

        if (returnStatement.Expression is null)
        {
            return;
        }

        if (!DoesMethodReturnEnumerable(context, returnStatement))
        {
            return;
        }

        var realReturnExpression = returnStatement.Expression.GetFirstNonCastExpression();
        var returnType = context.SemanticModel.GetTypeInfo(realReturnExpression).Type;
        if (returnType is null)
        {
            return;
        }

        if (!DoesTypeImplementAnyWellKnownCollectionInterface(returnType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, returnStatement.ReturnKeyword.GetLocation()));
    }

    private static bool DoesMethodReturnEnumerable(SyntaxNodeAnalysisContext context, ReturnStatementSyntax returnStatement)
    {
        var firstMatchingParent = returnStatement.GetParents().FirstOrDefault(a => a is MethodDeclarationSyntax or LocalFunctionStatementSyntax or LambdaExpressionSyntax or SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax);
        if (firstMatchingParent is null)
        {
            return false;
        }

        if (firstMatchingParent is LambdaExpressionSyntax or SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax)
        {
            return false;
        }

        var returnTypeSyntax = firstMatchingParent switch
        {
            MethodDeclarationSyntax methodDeclaration => methodDeclaration.ReturnType,
            LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.ReturnType,
            _ => null
        };

        if (returnTypeSyntax is null)
        {
            return false;
        }

        var returnType = context.SemanticModel.GetTypeInfo(returnTypeSyntax).Type;
        if (returnType is null)
        {
            return false;
        }

        return IsEnumerable(returnType);
    }

    private static bool IsEnumerable(ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.ToString() ?? string.Empty;

        var namedTypeSymbol = typeSymbol as INamedTypeSymbol;

        if (typeSymbol.Name.EqualsOrdinal("IEnumerable"))
        {
            if (ns.EqualsOrdinal("System.Collections"))
            {
                return true;
            }

            return namedTypeSymbol is not null && namedTypeSymbol.Arity == 1 && ns.EqualsOrdinal("System.Collections.Generic");
        }

        return false;
    }

    private static bool DoesTypeImplementAnyWellKnownCollectionInterface(ITypeSymbol typeSymbol)
        => IsWellKnownCollectionInterface(typeSymbol) || typeSymbol.AllInterfaces.Any(IsWellKnownCollectionInterface);

    private static bool IsWellKnownCollectionInterface(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        var ns = namedTypeSymbol.ContainingNamespace?.ToString();
        if (ns.IsNullOrWhiteSpace())
        {
            return false;
        }

        if (!ArityByTypeByNamespace.TryGetValue(ns, out var arityByType))
        {
            return false;
        }

        if (!arityByType.TryGetValue(typeSymbol.Name, out var arity))
        {
            return false;
        }

        return namedTypeSymbol.Arity == arity;
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            public const string Category = "Design";
            public const string DiagnosticId = "AJ0003";

            public static readonly LocalizableString Title = "Do not return materialised collection as enumerable";
            public static readonly LocalizableString MessageFormat = "Do not return materialised collection as enumerable";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        }
    }
}
