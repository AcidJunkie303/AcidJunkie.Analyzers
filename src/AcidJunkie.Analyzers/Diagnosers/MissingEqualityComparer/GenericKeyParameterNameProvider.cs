namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

internal static class GenericKeyParameterNameProvider
{
    private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> GenericKeyByMethodNameByContainingTypeByContainingTypeNameSpace =
        new(StringComparer.Ordinal)
        {
            {
                "System.Linq",
                new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
                {
                    {
                        "Enumerable",
                        new Dictionary<string, string> (StringComparer.Ordinal)
                        {
                            {"Contains", "TSource" },
                            {"Distinct", "TSource" },
                            {"DistinctBy", "TKey" },
                            {"Except", "TSource" },
                            {"ExceptBy", "TKey" },
                            {"GroupBy", "TKey" },
                            {"GroupJoin", "TKey" },
                            {"Intersect", "TSource" },
                            {"IntersectBy", "TKey" },
                            {"Join", "TKey" },
                            {"SequenceEqual", "TSource" },
                            {"ToDictionary", "TKey" },
                            {"ToHashSet", "TSource" },
                            {"ToLookup", "TKey" },
                            {"Union", "TSource" },
                            {"UnionBy", "TKey" }
                        }
                    }
                }
            },
            {
                "System.Collections.Immutable",
                new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
                {
                    {
                        "ImmutableDictionary",
                        new Dictionary<string, string> (StringComparer.Ordinal)
                        {
                            {"Create", "TKey" },
                            {"CreateRange", "TKey" },
                            {"CreateBuilder", "TKey" },
                            {"ToImmutableDictionary", "TKey" }
                        }
                    },
                    {
                        "ImmutableHashSet",
                        new Dictionary<string, string> (StringComparer.Ordinal)
                        {
                            {"Create", "T" },
                            {"CreateRange", "T" },
                            {"CreateBuilder", "T" },
                            {"ToImmutableHashSet", "TSource" }
                        }
                    }
                }
            },
            {
                "System.Collections.Frozen",
                new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
                {
                    {
                        "FrozenDictionary",
                        new Dictionary<string, string> (StringComparer.Ordinal)
                        {
                            {"ToFrozenDictionary", "TKey" }
                        }
                    },
                    {
                        "FrozenSet",
                        new Dictionary<string, string> (StringComparer.Ordinal)
                        {
                            {"ToFrozenSet", "T" }
                        }
                    }
                }
            }
        };

    private static readonly Dictionary<string, Dictionary<string, Dictionary<int, string>>> GenericKeyByGenericTypeCountByTypeNameByNameSpace =
        new(StringComparer.Ordinal)
        {
            {
                "System.Collections.Generic",
                new (StringComparer.Ordinal)
                {
                    {
                        "Dictionary",
                        new Dictionary<int, string>
                        {
                            {
                                2, "TKey"
                            }
                        }

                    },
                    {
                        "HashSet",
                        new Dictionary<int, string>
                        {
                            {
                                1, "T"
                            }
                        }

                    }
                }
            }
        };

    public static string? GetKeyParameterNameForInvocation(string containingTypeNamespaceName, string containingTypeName, string methodName)
    {
        if (!GenericKeyByMethodNameByContainingTypeByContainingTypeNameSpace.TryGetValue(containingTypeNamespaceName, out var genericKeyByMethodNameByContainingType))
        {
            return null;
        }

        if (!genericKeyByMethodNameByContainingType.TryGetValue(containingTypeName, out var genericKeyByMethodName))
        {
            return null;
        }

        return genericKeyByMethodName.TryGetValue(methodName, out var genericKeyName) ? genericKeyName : null;
    }

    public static string? GetKeyParameterNameForCreation(string containingTypeNamespaceName, string containingTypeName, int genericParameterCount)
    {
        if (!GenericKeyByGenericTypeCountByTypeNameByNameSpace.TryGetValue(containingTypeNamespaceName, out var genericKeyByMethodNameByContainingType))
        {
            return null;
        }

        if (!genericKeyByMethodNameByContainingType.TryGetValue(containingTypeName, out var genericKeyByMethodName))
        {
            return null;
        }

        return genericKeyByMethodName.TryGetValue(genericParameterCount, out var genericKeyName) ? genericKeyName : null;
    }
}
