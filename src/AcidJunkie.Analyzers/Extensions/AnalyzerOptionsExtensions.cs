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
        options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(key, out var value);
        return value;
    }

    public static bool GetGlobalOptionsBooleanValue(this AnalyzerOptions options, string key, bool defaultValue = true)
    {
        if (!options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return BooleanValuesByValue.TryGetValue(value, out var result)
            ? result
            : defaultValue;
    }
}
