using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

internal static class TypeSymbolExtensions
{
    private static readonly Dictionary<string, Dictionary<string, int>> CollectionInterfaceArityByTypeByNamespace = new(StringComparer.Ordinal)
    {
        {
            "System.Collections", new(StringComparer.Ordinal)
            {
                { "ICollection", 0 },
                { "IDictionary", 0 },
                { "IList", 0 }
            }
        },
        {
            "System.Collections.Generic", new(StringComparer.Ordinal)
            {
                { "ICollection", 1 },
                { "IDictionary", 1 },
                { "IList", 1 },
                { "ISet", 1 },
                { "IReadOnlyCollection", 1 }
            }
        }
    };

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

    public static string GetFullName(this ITypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace?.ToString();
        return ns.IsNullOrWhiteSpace()
            ? symbol.Name
            : $"{ns}.{symbol.Name}";
    }

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

    public static string GetSimplifiedName(this INamedTypeSymbol symbol)
    {
        var ns = symbol.GetFullNamespace();

        if (ns.IsNullOrWhiteSpace())
        {
            return symbol.Arity == 0
                ? symbol.Name
                : $"{symbol.Name}`{symbol.Arity}";
        }

        return symbol.Arity == 0
            ? $"{ns}.{symbol.Name}"
            : $"{ns}.{symbol.Name}`{symbol.Arity}";
    }

    public static bool DoesImplementWellKnownCollectionInterface(this ITypeSymbol typeSymbol)
        => IsWellKnownCollectionInterface(typeSymbol) || typeSymbol.AllInterfaces.Any(IsWellKnownCollectionInterface);

    public static bool IsWellKnownCollectionInterface(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        var ns = namedTypeSymbol.ContainingNamespace?.ToString();
        if (ns.IsNullOrWhiteSpace())
        {
            return false;
        }

        if (!CollectionInterfaceArityByTypeByNamespace.TryGetValue(ns, out var arityByType))
        {
            return false;
        }

        if (!arityByType.TryGetValue(typeSymbol.Name, out var arity))
        {
            return false;
        }

        return namedTypeSymbol.Arity == arity;
    }

    public static bool IsEqualTo(this ITypeSymbol typeSymbol, string @namespace, string typeName, int arity)
    {
        if (arity != 0)
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return false;
            }

            if (namedTypeSymbol.Arity != arity)
            {
                return false;
            }
        }

        return typeSymbol.Name.EqualsOrdinal(typeName) && typeSymbol.GetFullNamespace().EqualsOrdinal(@namespace);
    }

    public static bool IsEnumerable(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        return namedTypeSymbol.Arity switch
        {
            1 when !typeSymbol.IsContainedInNamespace("System.Collections.Generic") => false,
            0 when !typeSymbol.IsContainedInNamespace("System.Collections") => false,
            _ => typeSymbol.Name.EqualsOrdinal("IEnumerable")
        };
    }
}
