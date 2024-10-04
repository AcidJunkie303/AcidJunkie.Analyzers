using System.Diagnostics.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

internal static class StringExtensions
{
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
        => string.IsNullOrWhiteSpace(value);

    public static bool EqualsOrdinal(this string? value1, string? value2)
        => string.Equals(value1, value2, StringComparison.Ordinal);

    public static string? NullIfWhiteSpace(this string? value)
    {
        return value.IsNullOrWhiteSpace()
            ? null
            : value;
    }
}
