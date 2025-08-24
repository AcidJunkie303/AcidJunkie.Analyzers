using System.Text;
using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

internal static class MethodSymbolExtensions
{
    public static string GetSimplifiedName(this IMethodSymbol methodSymbol)
    {
        return new StringBuilder()
              .Append(methodSymbol.ContainingType.GetSimplifiedName())
              .Append('.')
              .Append(methodSymbol.Name)
              .ToString();
    }

    public static string GetFullName(this IMethodSymbol methodSymbol)
    {
        var ns = methodSymbol.ContainingType.ContainingNamespace.ToString();
        return ns.IsNullOrWhiteSpace()
            ? $"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}"
            : $"{ns}.{methodSymbol.ContainingType.Name}.{methodSymbol.Name}";
    }
}
