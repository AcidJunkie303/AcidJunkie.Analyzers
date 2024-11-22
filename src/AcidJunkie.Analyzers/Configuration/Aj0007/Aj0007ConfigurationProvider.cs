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

        var (parameterOrder, parameterOrderFlat) = GetParameterOrdering(options);
        if (parameterOrder.Count == 0)
        {
            return (Aj0007Configuration.Default, true);
        }

        var firstDuplicate = parameterOrder
            .GroupBy(a => a, StringComparer.OrdinalIgnoreCase)
            .Where(a => a.Count() > 1)
            .Select(a => a.Key)
            .FirstOrDefault();

        if (firstDuplicate is not null)
        {
            var error = new ConfigurationError(Aj0007Configuration.KeyNames.ParameterOrderingFlat, ".globalconfig", $"Duplicate value: {firstDuplicate}");
            return (new Aj0007Configuration(error), false);
        }

        return (new Aj0007Configuration(true, parameterOrderFlat, ParameterOrderParser.Parse(parameterOrder)), true);
    }

    private static (IReadOnlyList<string> ParameterOrder, string ParameterOrderFlat) GetParameterOrdering(AnalyzerOptions options)
    {
        var value = options.GetGlobalOptionsValueOrDefault(Aj0007Configuration.KeyNames.ParameterOrderingFlat);
        return value.IsNullOrWhiteSpace()
            ? ([], string.Empty)
            : (ParameterOrderParser.SplitConfigurationParameterOrder(value), value);
    }

    private static bool IsEnabled(AnalyzerOptions options) => options.GetGlobalOptionsBooleanValue(Aj0007Configuration.KeyNames.IsEnabled);
}
