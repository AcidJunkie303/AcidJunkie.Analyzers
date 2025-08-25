using System.Diagnostics.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
        => source
          .Where(a => a is not null)
          .Select(a => a!);

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? source)
        => source is null || source.Count == 0;
}
