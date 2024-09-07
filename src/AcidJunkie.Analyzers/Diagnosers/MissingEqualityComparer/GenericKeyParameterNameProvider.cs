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
            }
        };

    public static string? GetKeyParameterName(string containingTypeNamespaceName, string containingTypeName, string methodName)
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
}
