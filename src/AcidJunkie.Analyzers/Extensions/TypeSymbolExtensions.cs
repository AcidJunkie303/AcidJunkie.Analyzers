using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

internal static class TypeSymbolExtensions
{
    public static bool ImplementsInterface(this ITypeSymbol typeSymbol, string typeName, params string[] typeArguments)
    {
        foreach (var namedTypeSymbol in typeSymbol.AllInterfaces)
        {
            if (!namedTypeSymbol.Name.Equals(typeName, StringComparison.Ordinal))
            {
                continue;
            }

            if (!namedTypeSymbol.IsGenericType && typeArguments.Length > 0)
            {
                continue;
            }

            if (namedTypeSymbol.TypeArguments.Length != typeArguments.Length)
            {
                continue;
            }

            var typeMatchCount = namedTypeSymbol.TypeArguments
                .Select(a => a.ToString())
                .Intersect(typeArguments, StringComparer.Ordinal)
                .Count();

            if (typeMatchCount == typeArguments.Length)
            {
                return true;
            }
        }

        return false;
    }
}
