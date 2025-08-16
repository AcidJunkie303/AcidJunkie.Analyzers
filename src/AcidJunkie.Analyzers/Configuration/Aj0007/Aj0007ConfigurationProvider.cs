using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal sealed class Aj0007ConfigurationProvider : IConfigurationProvider<Aj0007Configuration>
{
    public static Aj0007ConfigurationProvider Instance { get; } = new();

    private Aj0007ConfigurationProvider()
    {
    }

    public Aj0007Configuration GetConfiguration(SyntaxNodeAnalysisContext context)
    {
        if (!IsEnabled(context))
        {
            return Aj0007Configuration.Disabled;
        }

        var (parameterOrder, parameterOrderFlat) = GetParameterOrdering(context);
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

    private static (IReadOnlyList<string> ParameterOrder, string ParameterOrderFlat) GetParameterOrdering(SyntaxNodeAnalysisContext context)
    {
        var value = context.GetOptionsValueOrDefault(Aj0007Configuration.KeyNames.ParameterOrderingFlat);
        return value.IsNullOrWhiteSpace()
            ? ([], string.Empty)
            : (ParameterOrderParser.SplitConfigurationParameterOrder(value), value);
    }

    private static bool IsEnabled(SyntaxNodeAnalysisContext context)
        => context.GetOptionsBooleanValue(Aj0007Configuration.KeyNames.IsEnabled);
}
