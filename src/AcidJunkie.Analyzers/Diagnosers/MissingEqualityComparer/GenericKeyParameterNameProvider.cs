using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

internal static class GenericKeyParameterNameProvider
{
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

    // @formatter:off
    private static readonly ImmutableDictionary<string, ImmutableDictionary<string, ImmutableDictionary<string, string>>> GenericKeyByMethodNameByContainingTypeByContainingTypeNameSpace =
        ImmutableDictionary
           .CreateBuilder<string, ImmutableDictionary<string, ImmutableDictionary<string, string>>>()
           .AddFluent(
                "System.Linq",
                ImmutableDictionary
                   .CreateBuilder<string, ImmutableDictionary<string, string>>()
                   .AddFluent(
                        "Enumerable",
                        ImmutableDictionary
                           .CreateBuilder<string, string>()
                           .AddFluent("AggregateBy", TypeNames.Key)
                           .AddFluent("Contains", TypeNames.Source)
                           .AddFluent("CountBy", TypeNames.Source)
                           .AddFluent("Distinct", TypeNames.Source)
                           .AddFluent("DistinctBy", TypeNames.Key)
                           .AddFluent("Except", TypeNames.Source)
                           .AddFluent("ExceptBy", TypeNames.Key)
                           .AddFluent("GroupBy", TypeNames.Key)
                           .AddFluent("GroupJoin", TypeNames.Key)
                           .AddFluent("Intersect", TypeNames.Source)
                           .AddFluent("IntersectBy", TypeNames.Key)
                           .AddFluent("Join", TypeNames.Key)
                           .AddFluent("SequenceEqual", TypeNames.Source)
                           .AddFluent("ToDictionary", TypeNames.Key)
                           .AddFluent("ToHashSet", TypeNames.Source)
                           .AddFluent("ToLookup", TypeNames.Key)
                           .AddFluent("Union", TypeNames.Source)
                           .AddFluent("UnionBy", TypeNames.Key)
                           .ToImmutable()
                    )
                   .ToImmutable()
            )
           .AddFluent(
                "System.Collections.Immutable",
                ImmutableDictionary
                   .CreateBuilder<string, ImmutableDictionary<string, string>>()
                   .AddFluent(
                        "ImmutableDictionary",
                        ImmutableDictionary.
                            CreateBuilder<string, string>()
                           .AddFluent("Create", TypeNames.Key)
                           .AddFluent("CreateRange", TypeNames.Key)
                           .AddFluent("CreateBuilder", TypeNames.Key)
                           .AddFluent("ToImmutableDictionary", TypeNames.Key)
                           .ToImmutable()
                    )
                   .AddFluent(
                        "ImmutableHashSet",
                        ImmutableDictionary
                           .CreateBuilder<string, string>()
                           .AddFluent("Create", TypeNames.T)
                           .AddFluent("CreateRange", TypeNames.T)
                           .AddFluent("CreateBuilder", TypeNames.T)
                           .AddFluent("ToImmutableHashSet", TypeNames.Source)
                           .ToImmutable()
                    )
                   .ToImmutable()
            )
           .AddFluent(
                "System.Collections.Frozen",
                ImmutableDictionary
                    .CreateBuilder<string, ImmutableDictionary<string, string>>()
                   .AddFluent(
                        "FrozenDictionary",
                        ImmutableDictionary
                           .CreateBuilder<string, string>()
                           .AddFluent("ToFrozenDictionary", TypeNames.Key)
                           .ToImmutable()
                    )
                   .AddFluent(
                        "FrozenSet",
                        ImmutableDictionary
                           .CreateBuilder<string, string>()
                           .AddFluent("ToFrozenSet", TypeNames.T)
                           .ToImmutable()
                    )
                   .ToImmutable()
            )
           .AddFluent(
                "Microsoft.EntityFrameworkCore",
                ImmutableDictionary
                   .CreateBuilder<string, ImmutableDictionary<string, string>>()
                   .AddFluent(
                        "EntityFrameworkQueryableExtensions",
                        ImmutableDictionary
                           .CreateBuilder<string, string>()
                           .AddFluent("ToDictionaryAsync", TypeNames.Key)
                           .AddFluent("ToHashSetAsync", TypeNames.Key)
                           .ToImmutable()
                    )
                   .ToImmutable()
            )
           .ToImmutable();

    private static readonly ImmutableDictionary<string, ImmutableDictionary<string, ImmutableDictionary<int, string>>>
        GenericKeyByGenericTypeCountByTypeNameByNameSpace =
            ImmutableDictionary
               .CreateBuilder<string, ImmutableDictionary<string, ImmutableDictionary<int, string>>>()
               .AddFluent(
                    "System.Collections.Generic",
                    ImmutableDictionary
                       .CreateBuilder<string, ImmutableDictionary<int, string>>()
                       .AddFluent(
                            "Dictionary",
                            ImmutableDictionary
                               .CreateBuilder<int, string>()
                               .AddFluent(2, TypeNames.Key)
                               .ToImmutable()
                        )
                       .AddFluent(
                            "HashSet",
                            ImmutableDictionary
                               .CreateBuilder<int, string>()
                               .AddFluent(1, TypeNames.T)
                               .ToImmutable()
                        )
                       .AddFluent(
                            "OrderedDictionary",
                            ImmutableDictionary
                               .CreateBuilder<int, string>()
                               .AddFluent(2, TypeNames.Key)
                               .ToImmutable()
                        )
                       .AddFluent(
                            "SortedDictionary",
                            ImmutableDictionary
                               .CreateBuilder<int, string>()
                               .AddFluent(2, TypeNames.Key)
                               .ToImmutable()
                        )
                       .ToImmutable()
                )
               .ToImmutable();
    // @formatter:on
}
