using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal sealed class Aj0007ConfigurationProvider : IConfigurationProvider<Aj0007Configuration>
{
    public (Aj0007Configuration, bool) GetConfiguration(AnalyzerOptions options)
    {
        if (!IsEnabled(options))
        {
            return (Aj0007Configuration.Disabled, false);
        }

        var parameterOrdering = GetParameterOrdering(options);
        if (parameterOrdering.Length == 0)
        {
            return (Aj0007Configuration.Default, true);
        }

        var firstDuplicate = parameterOrdering
            .GroupBy(a => a, StringComparer.OrdinalIgnoreCase)
            .Where(a => a.Count() > 1)
            .Select(a => a.Key)
            .FirstOrDefault();


        if (firstDuplicate is not null)
        {
            throw new IOException($"Duplicate parameter names found: {firstDuplicate}");
        }

        new Aj0007Configuration()
    }

    private static ImmutableArray<string> GetParameterOrdering(AnalyzerOptions options)
    {
        var value = options.GetGlobalOptionsValueOrDefault(Aj0007Configuration.KeyNames.ParameterOrderingFlat);
        return value.IsNullOrWhiteSpace()
            ? []
            : ParseParameterOrdering(value);

        static ImmutableArray<string> ParseParameterOrdering(string value)
        {
            return value.Split(['|'], StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => a.Length > 0)
                .ToImmutableArray();
        }
    }


    private static bool IsEnabled(AnalyzerOptions options) => options.GetGlobalOptionsBooleanValue(Aj0007Configuration.KeyNames.IsEnabled);

    private static class Defaults
    {
        public static Aj0007Configuration First { get; } = new(true, ParameterPlacement.First);
        public static Aj0007Configuration Last { get; } = new(true, ParameterPlacement.Last);
        public static Aj0007Configuration Disabled { get; } = new(false, ParameterPlacement.First);
    }
}
