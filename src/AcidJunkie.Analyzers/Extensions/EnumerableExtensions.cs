namespace AcidJunkie.Analyzers.Extensions;

internal static class EnumerableExtensions
{
    public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        var index = 0;
        foreach (var item in items)
        {
            if (predicate(item))
            {
                return index;
            }

            index++;
        }

        return -1;
    }
}
