using AcidJunkie.Analyzers.Configuration.Aj0007;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0002;

internal sealed class Aj0002ConfigurationProvider : ConfigurationProvider<Aj0002Configuration>
{
    public static Aj0002ConfigurationProvider Instance { get; } = new();

    private Aj0002ConfigurationProvider()
    {
    }

    protected override Aj0002Configuration GetConfigurationCore(AnalyzerConfigOptions options)
    {
        if (!options.IsDiagnosticEnabled("AJ0002"))
        {
            return Aj0002Configuration.Disabled;
        }

        try
        {
            var mode = GetMode(options);
            return mode switch
            {
                Mode.Strict  => Aj0002Configuration.Strict,
                Mode.Relaxed => Aj0002Configuration.Relaxed,
                _            => throw new InvalidOperationException($"Unknown mode: {mode}")
            };
        }
        catch (Exception ex)
        {
            var error = ConfigurationError.CreateWithDefaultEditorConfigFile(Aj0002Configuration.KeyNames.ParameterOrderingFlat, ex.Message);
            return new Aj0002Configuration(error);
        }
    }

    private static Mode GetMode(AnalyzerConfigOptions options)
    {
        var value = options.GetOptionsValueOrDefault(Aj0002Configuration.KeyNames.ParameterOrderingFlat);
        if (value.IsNullOrWhiteSpace())
        {
            return Mode.Strict;
        }

        return Enum.TryParse<Mode>(value, ignoreCase: true, out var mode)
            ? mode
            : throw new InvalidOperationException($"Unknown mode: {value}");
    }
}
