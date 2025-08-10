using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ReturnMaterialisedCollectionAsEnumerable;

internal sealed class ReturnMaterialisedCollectionAsEnumerableAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<ReturnMaterialisedCollectionAsEnumerableAnalyzerImplementation>
{
    public ReturnMaterialisedCollectionAsEnumerableAnalyzerImplementation(SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeReturn()
    {
        var returnStatement = (ReturnStatementSyntax)Context.Node;

        if (returnStatement.Expression is null)
        {
            Logger.WriteLine(() => "return statement has no expression");
            return;
        }

        if (!DoesMethodReturnEnumerable(returnStatement))
        {
            Logger.WriteLine(() => "Method return type is not IEnumerable or IEnumerable<T>");
            return;
        }

        var realReturnExpression = returnStatement.Expression.GetFirstNonCastExpression();
        var returnType = Context.SemanticModel.GetTypeInfo(realReturnExpression, Context.CancellationToken).Type;
        if (returnType is null)
        {
            Logger.WriteLine(() => $"Unable to determine the method return type from expression {realReturnExpression}");
            return;
        }

        if (!returnType.DoesImplementWellKnownCollectionInterface())
        {
            Logger.WriteLine(() => $"Return type {returnType.GetFullName()} does is or does not implement any well known collection interfaces");
            return;
        }

        Logger.ReportDiagnostic2(DiagnosticRules.Default.Rule, returnStatement.ReturnKeyword.GetLocation());
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, returnStatement.ReturnKeyword.GetLocation()));
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

    private bool DoesMethodReturnEnumerable(ReturnStatementSyntax returnStatement)
    {
        var firstMatchingParent = returnStatement.GetParents().FirstOrDefault(static a => a is MethodDeclarationSyntax or LocalFunctionStatementSyntax or LambdaExpressionSyntax or SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax);
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
            MethodDeclarationSyntax methodDeclaration           => methodDeclaration.ReturnType,
            LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.ReturnType,
            _                                                   => null
        };

        if (returnTypeSyntax is null)
        {
            return false;
        }

        var returnType = Context.SemanticModel.GetTypeInfo(returnTypeSyntax, Context.CancellationToken).Type;
        if (returnType is null)
        {
            return false;
        }

        return IsEnumerable(returnType);
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> AllRules { get; } = [Default.Rule];

        internal static class Default
        {
            private const string Category = "Performance";
            public const string DiagnosticId = "AJ0003";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0003.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Do not return materialised collection as enumerable";
            public static readonly LocalizableString MessageFormat = "Do not return materialised collection as enumerable";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
