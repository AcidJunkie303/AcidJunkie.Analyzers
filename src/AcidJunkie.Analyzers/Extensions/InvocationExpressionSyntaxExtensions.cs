using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

public static class InvocationExpressionSyntaxExtensions
{
    public static (string? OwningTypeNameSpace, string? OwningTypeName, string? MethodName, MemberAccessExpressionSyntax? MemberAccess)
        GetInvokedMethod(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
        => invocationExpression.GetInvokedMethod(semanticModel, CancellationToken.None);

    public static (string? OwningTypeNameSpace, string? OwningTypeName, string? MethodName, MemberAccessExpressionSyntax? MemberAccess)
        GetInvokedMethod(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression, cancellationToken);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
        var methodName = methodSymbol?.Name;
        var containingType = methodSymbol?.ContainingType;

        var memberAccess = invocationExpression.Expression as MemberAccessExpressionSyntax;

        return (containingType?.ContainingNamespace?.ToString(), containingType?.Name, methodName, memberAccess);
    }

    public static ITypeSymbol? GetTypeForTypeParameter(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, string typeParameterName)
        => invocationExpression.GetTypeForTypeParameter(semanticModel, typeParameterName, CancellationToken.None);

    public static ITypeSymbol? GetTypeForTypeParameter(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, string typeParameterName, CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression, cancellationToken);
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
