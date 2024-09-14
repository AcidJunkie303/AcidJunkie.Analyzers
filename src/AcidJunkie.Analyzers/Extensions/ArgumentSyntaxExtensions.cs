using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

internal static class ArgumentSyntaxExtensions
{

    public static string? GetArgumentTypeName(this ArgumentSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken = default)
    {
        var typeInfo = semanticModel.GetTypeInfo(node.Expression, cancellationToken);
        return typeInfo.Type?.ToString();
    }

    public static ITypeSymbol? GetArgumentType(this ArgumentSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken = default)
    {
        var typeInfo = semanticModel.GetTypeInfo(node.Expression, cancellationToken);

        return typeInfo.Type;
    }

    public static string? GetArgumentName(this ArgumentSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken = default)
    {
        var info = semanticModel.GetSymbolInfo(node.Expression, cancellationToken);
        return info.Symbol?.ToString();
    }
}
