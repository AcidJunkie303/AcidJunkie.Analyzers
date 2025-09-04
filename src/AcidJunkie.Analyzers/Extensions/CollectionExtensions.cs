namespace AcidJunkie.Analyzers.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
        => source
          .Where(a => a is not null)
          .Select(a => a!);

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? [];

    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        => source.Where(a => !predicate(a));
}
