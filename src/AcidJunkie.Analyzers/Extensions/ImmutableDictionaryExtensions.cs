using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

public static class ImmutableDictionaryExtensions
{
    [SuppressMessage("Major Code Smell", "S4017:Method signatures should not contain nested generic types")]
    public static ImmutableDictionary<TKey, TValue>.Builder AddRangeFluent<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue>.Builder builder,
        IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        foreach (var kv in items)
        {
            builder.Add(kv.Key, kv.Value);
        }

        return builder;
    }

    [SuppressMessage("Major Code Smell", "S4017:Method signatures should not contain nested generic types")]
    public static ImmutableDictionary<TKey, TValue>.Builder AddFluent<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue>.Builder builder,
        TKey key,
        TValue value)
    {
        builder.Add(key, value);

        return builder;
    }
}
