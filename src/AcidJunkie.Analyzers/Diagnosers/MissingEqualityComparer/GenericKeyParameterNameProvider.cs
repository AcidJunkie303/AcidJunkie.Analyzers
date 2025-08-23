using System.Collections.Immutable;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

internal static class GenericKeyParameterNameProvider
{
    private static readonly ImmutableDictionary<string, ImmutableDictionary<string, ImmutableDictionary<string, string>>> GenericKeyByMethodNameByContainingTypeByContainingTypeNameSpace =
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
                            ("AggregateBy", "TKey"),
                            ("Contains", "TSource"),
                            ("CountBy", "TSource"),
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
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
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
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    ),
                    (
                        "ImmutableHashSet",
                        new[]
                        {
                            ("Create", "T"),
                            ("CreateRange", "T"),
                            ("CreateBuilder", "T"),
                            ("ToImmutableHashSet", "TSource")
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            ),
            (
                "System.Collections.Frozen",
                new[]
                {
                    (
                        "ImmutableDictionary",
                        new[]
                        {
                            ("ToImmutableDictionary", "TKey")
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    ),
                    (
                        "FrozenSet",
                        new[]
                        {
                            ("ToFrozenSet", "T")
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            )
        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal);

    private static readonly ImmutableDictionary<string, ImmutableDictionary<string, ImmutableDictionary<int, string>>>
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
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "HashSet",
                            new[]
                            {
                                (
                                    1, "T"
                                )
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "OrderedDictionary",
                            new[]
                            {
                                (
                                    2, "TKey"
                                )
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "SortedDictionary",
                            new[]
                            {
                                (
                                    2, "TKey"
                                )
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        )
                    }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                )
            }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal);

    public static string? GetKeyParameterNameForInvocation(string containingTypeNamespaceName,
                                                           string containingTypeName, string methodName)
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

    public static string? GetKeyParameterNameForCreation(string containingTypeNamespaceName, string containingTypeName,
                                                         int genericParameterCount)
    {
        if (!GenericKeyByGenericTypeCountByTypeNameByNameSpace.TryGetValue(containingTypeNamespaceName, out var genericKeyByMethodNameByContainingType))
        {
            return null;
        }

        if (!genericKeyByMethodNameByContainingType.TryGetValue(containingTypeName, out var genericKeyByMethodName))
        {
            return null;
        }

        return genericKeyByMethodName.TryGetValue(genericParameterCount, out var genericKeyName)
            ? genericKeyName
            : null;
    }
}
