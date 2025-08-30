using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Configuration.Aj0002;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

public static class AnalyzerConfigOptionsExtensions
{
    private static readonly ImmutableDictionary<string, bool> BooleanValuesByValue = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
    {
        {
            "false", false
        },
        {
            "0", false
        },
        {
            "disable", false
        },
        {
            "disabled", false
        },
        {
            "no", false
        },
        {
            "true", true
        },
        {
            "1", true
        },
        {
            "enable", true
        },
        {
            "enabled", true
        },
        {
            "yes", true
        }
    }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

    public static bool IsEnabled(AnalyzerConfigOptions options)
        => options.GetOptionsBooleanValue(Aj0002Configuration.KeyNames.IsEnabled, defaultValue: true);

    public static string? GetOptionsValueOrDefault(this AnalyzerConfigOptions options, string key)
    {
        options.TryGetValue(key, out var value);
        return value;
    }

    public static bool GetOptionsBooleanValue(this AnalyzerConfigOptions options, string key, bool defaultValue)
    {
        var value = options.GetOptionsValueOrDefault(key);
        if (value.IsNullOrWhiteSpace())
        {
            return defaultValue;
        }

        return BooleanValuesByValue.TryGetValue(value, out var result)
            ? result
            : defaultValue;
    }

    public static bool IsDiagnosticEnabled(this AnalyzerConfigOptions options, string diagnosticId)
    {
        var keyName = GenericConfigurationKeyNames.CreateIsEnabledKeyName(diagnosticId);
        return options.GetOptionsBooleanValue(keyName, defaultValue: true);
    }
}
