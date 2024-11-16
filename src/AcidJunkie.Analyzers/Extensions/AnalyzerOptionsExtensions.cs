using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

public static class AnalyzerOptionsExtensions
{

    private static readonly Dictionary<string, bool> BooleanValuesByValue = new(StringComparer.OrdinalIgnoreCase)
    {
        {"false", false},
        {"0", false},
        {"disable", false},
        {"disabled", false},
        {"no", false},
        {"true", true},
        {"1", true},
        {"enable", true},
        {"enabled", true},
        {"yes", true}
    };

    public static string? GetGlobalOptionsValueOrDefault(this AnalyzerOptions options, string key)
    {
        _ = options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(key, out var value);
        return value;
    }

    public static bool GetGlobalOptionsBooleanValue(this AnalyzerOptions options, string key, bool defaultValue = true)
    {
#pragma warning disable IDE0046 // Use conditional expression for return -> If it is simplified, we'd have nested conditional expressions
        if (!options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(key, out var value))
#pragma warning restore IDE0046
        {
            return defaultValue;
        }

        return BooleanValuesByValue.TryGetValue(value, out var result)
            ? result
            : defaultValue;
    }
}
