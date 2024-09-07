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

    public static (string? OwningTypeNameSpace, string? OwningTypeName, string? MethodName) GetInvokedMethod(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
        var methodName = methodSymbol?.Name;
        var containingType = methodSymbol?.ContainingType;

        return (containingType?.ContainingNamespace?.ToString(), containingType?.Name, methodName);

        //   HandleExpression(invocationExpression.Expression);
        //
        //   return result;
        //
        //   void HandleExpression(SyntaxNode node)
        //   {
        //       if (node is IdentifierNameSyntax identifierName )
        //       {
        //           var methodName = identifierName.Identifier.Text;
        //           var methodSymbolInfo = semanticModel.GetSymbolInfo(identifierName);
        //           var methodSymbol = methodSymbolInfo.Symbol as IMethodSymbol;
        //
        //           var containingTypeNamespace = methodSymbol?.ContainingNamespace.Name;
        //           var containingTypeName = methodSymbol?.Name;
        //
        //           if (containingTypeNamespace is not null && containingTypeName is not null)
        //           {
        //               result.Add((containingTypeNamespace, containingTypeName, methodName));
        //           }
        //       }
        //
        //       foreach (var child in node.ChildNodes())
        //       {
        //           HandleExpression(child);
        //       }

    }

    //  var node = invocationExpression.Expression;
    //
    //  while (node is MemberAccessExpressionSyntax memberAccessExpression)
    //  {
    //      // Traverse the member access chain
    //      node = memberAccessExpression.Expression;
    //  }
    //
    //  if (node is IdentifierNameSyntax identifierName)
    //  {
    //      var methodName = identifierName.Identifier.Text;
    //      var methodSymbolInfo = semanticModel.GetSymbolInfo(identifierName);
    //      var methodSymbol = methodSymbolInfo.Symbol as IMethodSymbol;
    //
    //      return (methodSymbol?.ContainingNamespace?.Name, methodSymbol?.Name, methodName);
    //
    //      /*
    //      var symbolInfo = semanticModel.GetSymbolInfo(identifierName.Expression).Symbol as ITypeSymbol;
    //
    //      */
    //  }
    //
    //  return (null, null, null);
    //  /*
    //  if (invocationExpression.Expression is IdentifierNameSyntax identifierName)
    //  {
    //      IdentifierNameSyntax
    //      // Handle simple method calls without member access
    //      var methodName = identifierName.Identifier.Text;
    //      Console.WriteLine($"Method '{methodName}' is invoked.");
    //  }
    //  */

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
