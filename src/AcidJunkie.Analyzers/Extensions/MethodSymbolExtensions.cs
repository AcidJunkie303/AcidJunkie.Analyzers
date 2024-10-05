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
}
