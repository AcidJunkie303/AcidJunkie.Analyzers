using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class ArgumentSyntaxExtensions
{

    public static string? GetArgumentTypeName(this ArgumentSyntax node, SyntaxNodeAnalysisContext context)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(node.Expression);
        return typeInfo.Type?.ToString();
    }

    public static ITypeSymbol? GetArgumentType(this ArgumentSyntax node, SyntaxNodeAnalysisContext context)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(node.Expression);

        return typeInfo.Type;
    }

    public static string? GetArgumentName(this ArgumentSyntax node, SyntaxNodeAnalysisContext context)
    {
        var info = context.SemanticModel.GetSymbolInfo(node.Expression);
        return info.Symbol?.ToString();
    }
}
