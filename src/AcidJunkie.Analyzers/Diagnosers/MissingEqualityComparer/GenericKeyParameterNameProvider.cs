using System.Collections.Frozen;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

internal static class GenericKeyParameterNameProvider
{
    private static readonly FrozenDictionary<string, FrozenDictionary<string, FrozenDictionary<string, string>>> GenericKeyByMethodNameByContainingTypeByContainingTypeNameSpace =
        new[]
        {
            (
                "System.Linq",
                new[]
                {
                    (
                        "Enumerable",
                        new[]
                        {
                            ("Contains", "TSource"),
                            ("Distinct", "TSource"),
                            ("DistinctBy", "TKey"),
                            ("Except", "TSource"),
                            ("ExceptBy", "TKey"),
                            ("GroupBy", "TKey"),
                            ("GroupJoin", "TKey"),
                            ("Intersect", "TSource"),
                            ("IntersectBy", "TKey"),
                            ("Join", "TKey"),
                            ("SequenceEqual", "TSource"),
                            ("ToDictionary", "TKey"),
                            ("ToHashSet", "TSource"),
                            ("ToLookup", "TKey"),
                            ("Union", "TSource"),
                            ("UnionBy", "TKey")
                        }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            ),
            (
                "System.Collections.Immutable",
                new[]
                {
                    (
                        "ImmutableDictionary",
                        new[]
                        {
                            ("Create", "TKey"),
                            ("CreateRange", "TKey"),
                            ("CreateBuilder", "TKey"),
                            ("ToImmutableDictionary", "TKey")
                        }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    ),
                    (
                        "ImmutableHashSet",
                        new[]
                        {
                            ("Create", "T"),
                            ("CreateRange", "T"),
                            ("CreateBuilder", "T"),
                            ("ToImmutableHashSet", "TSource")
                        }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            ),
            (
                "System.Collections.Frozen",
                new[]
                {
                    (
                        "FrozenDictionary",
                        new[]
                        {
                            ("ToFrozenDictionary", "TKey")
                        }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    ),
                    (
                        "FrozenSet",
                        new[]
                        {
                            ("ToFrozenSet", "T")
                        }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            )
        }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, FrozenDictionary<string, FrozenDictionary<int, string>>>
        GenericKeyByGenericTypeCountByTypeNameByNameSpace =
            new[]
            {
                (
                    "System.Collections.Generic",
                    new[]
                    {
                        (
                            "Dictionary",
                            new[]
                            {
                                (
                                    2, "TKey"
                                )
                            }.ToFrozenDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "HashSet",
                            new[]
                            {
                                (
                                    1, "T"
                                )
                            }.ToFrozenDictionary(a => a.Item1, a => a.Item2)
                        )
                    }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                )
            }.ToFrozenDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal);

    public static string? GetKeyParameterNameForInvocation(string containingTypeNamespaceName,
        string containingTypeName, string methodName)
    {
        if (!GenericKeyByMethodNameByContainingTypeByContainingTypeNameSpace.TryGetValue(containingTypeNamespaceName,
                out var genericKeyByMethodNameByContainingType))
            return null;

        if (!genericKeyByMethodNameByContainingType.TryGetValue(containingTypeName, out var genericKeyByMethodName)) return null;

        return genericKeyByMethodName.TryGetValue(methodName, out var genericKeyName) ? genericKeyName : null;
    }

    public static string? GetKeyParameterNameForCreation(string containingTypeNamespaceName, string containingTypeName,
        int genericParameterCount)
    {
        if (!GenericKeyByGenericTypeCountByTypeNameByNameSpace.TryGetValue(containingTypeNamespaceName,
                out var genericKeyByMethodNameByContainingType))
            return null;

        if (!genericKeyByMethodNameByContainingType.TryGetValue(containingTypeName, out var genericKeyByMethodName)) return null;

        return genericKeyByMethodName.TryGetValue(genericParameterCount, out var genericKeyName)
            ? genericKeyName
            : null;
    }
}
