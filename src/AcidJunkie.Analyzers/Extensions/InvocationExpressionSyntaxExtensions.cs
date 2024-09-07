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
}
