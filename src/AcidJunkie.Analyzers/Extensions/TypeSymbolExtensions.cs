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

    public static bool ImplementsGenericEquatable(this ITypeSymbol symbol)
        => symbol.AllInterfaces
            .Where(a => a.TypeParameters.Length == 1)
            .Where(a => a.ContainingNamespace.Name.EqualsOrdinal("System"))
            .Any(a => a.Name.EqualsOrdinal("IEquatable"));

    public static bool IsGetHashCodeOverridden(this ITypeSymbol symbol)
        => symbol
            .GetMembers(nameof(GetHashCode))
            .OfType<IMethodSymbol>()
            .Any(a => a.Parameters.Length == 0);

    public static string GetFullNamespace(this ITypeSymbol symbol)
        => symbol.ContainingNamespace?.ToString() ?? string.Empty;

    public static bool IsContainedInNamespace(this ITypeSymbol symbol, string ns)
        => symbol.GetFullNamespace().EqualsOrdinal(ns);

    public static bool ImplementsOrIsInterface(this ITypeSymbol symbol, string interfaceNamespace, string interfaceName, params ITypeSymbol[] typeArguments)
    {
        if (symbol.IsContainedInNamespace(interfaceNamespace) && symbol.Name.EqualsOrdinal(interfaceName))
        {
            if (symbol is INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.TypeArguments.Length != typeArguments.Length)
                {
                    return false;
                }

                for (var i = 0; i < typeArguments.Length; i++)
                {
                    var typeArgument = typeArguments[i];
                    var typeArgument2 = typeSymbol.TypeArguments[i];

                    if (!typeArgument.Equals(typeArgument2, SymbolEqualityComparer.Default))
                    {
                        return false;
                    }
                }

                return true;
            }

            return typeArguments.Length == 0;
        }

        return symbol.AllInterfaces
            .Where(a => a.TypeParameters.Length == typeArguments.Length)
            .Where(a => a.ContainingNamespace.ToString().EqualsOrdinal(interfaceNamespace))
            .Where(a => a.Name.EqualsOrdinal(interfaceName))
            .Any(a =>
            {
                if (a.TypeArguments.Length != typeArguments.Length)
                {
                    return false;
                }

                for (var i = 0; i < typeArguments.Length; i++)
                {
                    var typeArgument = typeArguments[i];
                    var typeArgument2 = a.TypeArguments[i];

                    if (!typeArgument.Equals(typeArgument2, SymbolEqualityComparer.Default))
                    {
                        return false;
                    }
                }

                return true;
            })
            ;
    }
}
