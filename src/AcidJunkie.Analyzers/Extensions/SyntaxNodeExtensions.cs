using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class SyntaxNodeExtensions
{
    public static string GetFullTypeName(this SyntaxNode node, SyntaxNodeAnalysisContext context, TypeSyntax type)
    {
        var typeNameSpace = node.GetNameSpace(context);
        return string.Concat(typeNameSpace, '.', type);
    }

    public static string? GetNameSpace(this SyntaxNode node, SyntaxNodeAnalysisContext context)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(node);
        return ((INamedTypeSymbol?)typeInfo.Type)?.ContainingNamespace.ToString();
    }

    public static ITypeSymbol? GetOwningSymbol(this InvocationExpressionSyntax node, SyntaxNodeAnalysisContext context)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(node);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

        return methodSymbol?.ReceiverType;
    }

    public static SyntaxNode? GetFirstChild(this SyntaxNode node) => node.ChildNodes().FirstOrDefault();

    public static ClassDeclarationSyntax? FindTopClass(this SyntaxNode node)
    {
        ClassDeclarationSyntax? topmostClass = null;

        var n = node;

        while (n is not null)
        {

            if (n is ClassDeclarationSyntax cd)
            {
                topmostClass = cd;
            }

            n = n.Parent;
        }

        return topmostClass;
    }
}
