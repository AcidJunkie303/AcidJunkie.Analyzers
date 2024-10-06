using System.Text;
using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

internal static class MethodSymbolExtensions
{
    public static string GetSimplifiedName(this IMethodSymbol methodSymbol)
    {
        var buffer = new StringBuilder();
        buffer.Append(methodSymbol.ContainingType.GetSimplifiedName());
        buffer.Append('.');
        buffer.Append(methodSymbol.Name);

        return buffer.ToString();
    }

    public static string GetFullName(this IMethodSymbol methodSymbol)
    {
        var ns = methodSymbol.ContainingType.ContainingNamespace.ToString();
        return ns.IsNullOrWhiteSpace()
            ? $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}"
            : $"{ns}.{methodSymbol.ContainingType.Name}.{methodSymbol.Name}";
    }
}
