using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

internal static class ArgumentSyntaxExtensions
{

    public static string? GetArgumentTypeName(this ArgumentSyntax node, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(node.Expression);
        return typeInfo.Type?.ToString();
    }

    public static ITypeSymbol? GetArgumentType(this ArgumentSyntax node, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(node.Expression);

        return typeInfo.Type;
    }

    public static string? GetArgumentName(this ArgumentSyntax node, SemanticModel semanticModel)
    {
        var info = semanticModel.GetSymbolInfo(node.Expression);
        return info.Symbol?.ToString();
    }
}
