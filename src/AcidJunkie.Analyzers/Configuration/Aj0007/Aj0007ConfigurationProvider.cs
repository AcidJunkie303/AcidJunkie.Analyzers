using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal sealed class Aj0007ConfigurationProvider : ConfigurationProvider<Aj0007Configuration>
{
    public static Aj0007ConfigurationProvider Instance { get; } = new();

    private Aj0007ConfigurationProvider()
    {
    }

    protected override Aj0007Configuration GetConfigurationCore(AnalyzerConfigOptions options)
    {
        if (!options.IsDiagnosticEnabled("AJ0007"))
        {
            return Aj0007Configuration.Disabled;
        }

        var (parameterOrder, parameterOrderFlat) = GetParameterOrdering(options);
        if (parameterOrder.Count == 0)
        {
            return Aj0007Configuration.Default;
        }

        var firstDuplicate = parameterOrder
                            .GroupBy(a => a, StringComparer.OrdinalIgnoreCase)
                            .Where(a => a.Count() > 1)
                            .Select(a => a.Key)
                            .FirstOrDefault();

        if (firstDuplicate is not null)
        {
            var error = new ConfigurationError(Aj0007Configuration.KeyNames.ParameterOrderingFlat, ".editorconfig", $"Duplicate value: {firstDuplicate}");
            return new Aj0007Configuration(error);
        }

        return new Aj0007Configuration(true, parameterOrderFlat, ParameterOrderParser.Parse(parameterOrder));
    }

    private static (IReadOnlyList<string> ParameterOrder, string ParameterOrderFlat) GetParameterOrdering(AnalyzerConfigOptions options)
    {
        var value = options.GetOptionsValueOrDefault(Aj0007Configuration.KeyNames.ParameterOrderingFlat);
        return value.IsNullOrWhiteSpace()
            ? ([], string.Empty)
            : (ParameterOrderParser.SplitConfigurationParameterOrder(value), value);
    }
}
