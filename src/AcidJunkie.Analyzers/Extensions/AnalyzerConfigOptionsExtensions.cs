using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

public static class AnalyzerConfigOptionsExtensions
{
    private static readonly ImmutableDictionary<string, bool> BooleanValuesByValue
        = ImmutableDictionary.CreateBuilder<string, bool>(StringComparer.OrdinalIgnoreCase)
                             .AddFluent("false", false)
                             .AddFluent("0", false)
                             .AddFluent("disable", false)
                             .AddFluent("disabled", false)
                             .AddFluent("no", false)
                             .AddFluent("true", true)
                             .AddFluent("1", true)
                             .AddFluent("enable", true)
                             .AddFluent("enabled", true)
                             .AddFluent("yes", true)
                             .ToImmutable();

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
