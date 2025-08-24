namespace AcidJunkie.Analyzers.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
        => source
          .Where(a => a is not null)
          .Select(a => a!);
}
