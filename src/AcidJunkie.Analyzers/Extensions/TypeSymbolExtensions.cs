using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

internal static class TypeSymbolExtensions
{
    private static readonly Dictionary<string, Dictionary<string, int>> CollectionInterfaceArityByTypeByNamespace = new(StringComparer.Ordinal)
    {
        {
            "System.Collections", new Dictionary<string, int>(StringComparer.Ordinal)
            {
                {
                    "ICollection", 0
                },
                {
                    "IDictionary", 0
                },
                {
                    "IList", 0
                }
            }
        },
        {
            "System.Collections.Generic", new Dictionary<string, int>(StringComparer.Ordinal)
            {
                {
                    "ICollection", 1
                },
                {
                    "IDictionary", 1
                },
                {
                    "IList", 1
                },
                {
                    "ISet", 1
                },
                {
                    "IReadOnlyCollection", 1
                }
            }
        }
    };

    public static bool ImplementsGenericEquatable(this ITypeSymbol symbol)
        => symbol.AllInterfaces
                 .Any(static a => a.TypeParameters.Length == 1 && a.ContainingNamespace.Name.EqualsOrdinal("System") && a.Name.EqualsOrdinal("IEquatable"));

    public static bool IsEqualsOverridden(this ITypeSymbol symbol)
        => symbol
          .GetMembers(nameof(Equals))
          .OfType<IMethodSymbol>()
          .Any(static a => a.Parameters.Length == 1 && a is { Arity: 0, ReturnType.SpecialType: SpecialType.System_Boolean } && a.Parameters[0].Type.SpecialType == SpecialType.System_Object);

    public static bool IsSpecificEqualsOverridden(this ITypeSymbol symbol)
        => symbol
          .GetMembers(nameof(Equals))
          .OfType<IMethodSymbol>()
          .Any(a => a.Parameters.Length == 1
                    && a is { Arity: 0, ReturnType.SpecialType: SpecialType.System_Boolean }
                    && SymbolEqualityComparer.Default.Equals(a.Parameters[0].Type, symbol));

    public static bool IsGetHashCodeOverridden(this ITypeSymbol symbol)
        => symbol
          .GetMembers(nameof(GetHashCode))
          .OfType<IMethodSymbol>()
          .Any(static a => a.Parameters.Length == 0 && a is { Arity: 0, ReturnType.SpecialType: SpecialType.System_Int32 });

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

    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high")]
    [SuppressMessage("Critical Code Smell", "S134:Control flow statements \"if\", \"switch\", \"for\", \"foreach\", \"while\", \"do\"  and \"try\" should not be nested too deeply")]
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
                     .Where(a => a.ContainingNamespace.ToString().EqualsOrdinal(interfaceNamespace) && a.Name.EqualsOrdinal(interfaceName))
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
                      });
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
            0 when !typeSymbol.IsContainedInNamespace("System.Collections")         => false,
            _                                                                       => typeSymbol.Name.EqualsOrdinal("IEnumerable")
        };
    }

    public static bool IsEnumerable(this ITypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeSymbol? ofType)
    {
        ofType = null;

        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        if (namedTypeSymbol.Arity != 1)
        {
            return false;
        }

        if (!typeSymbol.Name.EqualsOrdinal("IEnumerable") || !typeSymbol.IsContainedInNamespace("System.Collections.Generic"))
        {
            return false;
        }

        ofType = namedTypeSymbol.TypeArguments[0];
        return true;
    }

    public static bool IsQueryable(this ITypeSymbol typeSymbol)
        => typeSymbol is INamedTypeSymbol namedTypeSymbol
           && namedTypeSymbol.Name.EqualsOrdinal("IQueryable")
           && namedTypeSymbol.ContainingNamespace.ToString().EqualsOrdinal("System.Linq");

    public static bool IsQueryable(this ITypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeSymbol? ofType)
    {
        ofType = null;

        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        if (namedTypeSymbol.Arity != 1)
        {
            return false;
        }

        if (!typeSymbol.Name.EqualsOrdinal("IQueryable") || !typeSymbol.IsContainedInNamespace("System.Linq"))
        {
            return false;
        }

        ofType = namedTypeSymbol.TypeArguments[0];
        return true;
    }
}
