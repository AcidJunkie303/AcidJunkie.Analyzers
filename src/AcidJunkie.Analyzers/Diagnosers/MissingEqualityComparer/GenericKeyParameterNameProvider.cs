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
                            ("AggregateBy", TypeNames.Key),
                            ("Contains", TypeNames.Source),
                            ("CountBy", TypeNames.Source),
                            ("Distinct", TypeNames.Source),
                            ("DistinctBy", TypeNames.Key),
                            ("Except", TypeNames.Source),
                            ("ExceptBy", TypeNames.Key),
                            ("GroupBy", TypeNames.Key),
                            ("GroupJoin", TypeNames.Key),
                            ("Intersect", TypeNames.Source),
                            ("IntersectBy", TypeNames.Key),
                            ("Join", TypeNames.Key),
                            ("SequenceEqual", TypeNames.Source),
                            ("ToDictionary", TypeNames.Key),
                            ("ToHashSet", TypeNames.Source),
                            ("ToLookup", TypeNames.Key),
                            ("Union", TypeNames.Source),
                            ("UnionBy", TypeNames.Key)
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
                            ("Create", TypeNames.Key),
                            ("CreateRange", TypeNames.Key),
                            ("CreateBuilder", TypeNames.Key),
                            ("ToImmutableDictionary", TypeNames.Key)
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    ),
                    (
                        "ImmutableHashSet",
                        new[]
                        {
                            ("Create", TypeNames.T),
                            ("CreateRange", TypeNames.T),
                            ("CreateBuilder", TypeNames.T),
                            ("ToImmutableHashSet", TypeNames.Source)
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            ),
            (
                "System.Collections.Frozen",
                new[]
                {
                    (
                        "FrozenDictionary",
                        new[]
                        {
                            ("ToFrozenDictionary", TypeNames.Key)
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    ),
                    (
                        "FrozenSet",
                        new[]
                        {
                            ("ToFrozenSet", TypeNames.T)
                        }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
                    )
                }.ToImmutableDictionary(a => a.Item1, a => a.Item2, StringComparer.Ordinal)
            ),
            (
                "Microsoft.EntityFrameworkCore",
                new[]
                {
                    (
                        "EntityFrameworkQueryableExtensions",
                        new[]
                        {
                            ("ToDictionaryAsync", TypeNames.Key),
                            ("ToHashSetAsync", TypeNames.Key)
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
                                    2, TypeNames.Key
                                )
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "HashSet",
                            new[]
                            {
                                (
                                    1, TypeNames.T
                                )
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "OrderedDictionary",
                            new[]
                            {
                                (
                                    2, TypeNames.Key
                                )
                            }.ToImmutableDictionary(a => a.Item1, a => a.Item2)
                        ),
                        (
                            "SortedDictionary",
                            new[]
                            {
                                (
                                    2, TypeNames.Key
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

    private static class TypeNames
    {
        public const string Key = "TKey";
        public const string Source = "TSource";
        public const string T = "T";
    }
}
