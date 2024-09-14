using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

public static class InvocationExpressionSyntaxExtensions
{
    public static bool IsCalledOnTypeName(this InvocationExpressionSyntax node, string typeName, SyntaxNodeAnalysisContext context)
    {
        var methodSymbol = node.GetOwningSymbol(context);
        if (methodSymbol is null)
        {
            return false;
        }

        var owningMethodType = methodSymbol.ToString();

        return owningMethodType.EqualsOrdinal(typeName);
    }

    public static SyntaxNode? GetFirstArgumentChild(this InvocationExpressionSyntax node)
        => node.ArgumentList.Arguments.Count == 0
            ? null
            : node.ArgumentList.Arguments[0].ChildNodes().FirstOrDefault();

    public static bool DoesMethodNameMatchAny(this InvocationExpressionSyntax node, params string[] methodNames)
    {
        if (node.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return false;
        }

        var actualMethodName = memberAccessExpression.Name.Identifier.Value?.ToString();

        return actualMethodName is not null && methodNames.Contains(actualMethodName, StringComparer.Ordinal);
    }

    public static (string? OwningTypeNameSpace, string? OwningTypeName, string? MethodName, MemberAccessExpressionSyntax? MemberAccess) GetInvokedMethod(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
        var methodName = methodSymbol?.Name;
        var containingType = methodSymbol?.ContainingType;

        var memberAccess = invocationExpression.Expression as MemberAccessExpressionSyntax;

        return (containingType?.ContainingNamespace?.ToString(), containingType?.Name, methodName, memberAccess);

    }

    public static ITypeSymbol? GetTypeForTypeParameter(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, string typeParameterName)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var index = GetTypeParameterIndex();
        return index < 0 ? null : methodSymbol.TypeArguments[index];

        int GetTypeParameterIndex()
        {
            for (var i = 0; i < methodSymbol.TypeParameters.Length; i++)
            {
                var typeParameter = methodSymbol.TypeParameters[i];
                if (typeParameter.Name.EqualsOrdinal(typeParameterName))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
