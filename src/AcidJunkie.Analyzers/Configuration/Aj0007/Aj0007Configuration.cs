using System.Collections.Immutable;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal sealed class Aj0007Configuration : IAnalyzerConfiguration
{
    public static Aj0007Configuration Default { get; } = new(true, Defaults.ParameterOrderFlat, Defaults.ParameterDescriptions);
    public static Aj0007Configuration Disabled { get; } = new(false, Defaults.ParameterOrderFlat, ImmutableArray<ParameterDescription>.Empty);

    public bool IsEnabled { get; }
    public ConfigurationError? ConfigurationError { get; }
    public string ParameterOrderFlat { get; }
    public ImmutableArray<ParameterDescription> ParameterDescriptions { get; }

    public Aj0007Configuration(bool isEnabled, string parameterOrderFlat, in ImmutableArray<ParameterDescription> parameterDescriptions)
    {
        IsEnabled = isEnabled;
        ParameterOrderFlat = parameterOrderFlat;
        ParameterDescriptions = parameterDescriptions;
    }

    public Aj0007Configuration(ConfigurationError configurationError)
    {
        ConfigurationError = configurationError;
        ParameterOrderFlat = string.Empty;
        IsEnabled = false;
        ParameterDescriptions = ImmutableArray<ParameterDescription>.Empty;
    }

    public static class KeyNames
    {
        public const string IsEnabled = "AJ0007.is_enabled";
        public const string ParameterOrderingFlat = "AJ0007.parameter_ordering";
    }

    internal static class Placeholders
    {
        public const string Other = "{other}";
    }

    internal static class Defaults
    {
        public const string ParameterOrderFlat = $"{Placeholders.Other}|Microsoft.Extensions.Logging*|System.Threading.CancellationToken";
        public static ImmutableArray<ParameterDescription> ParameterDescriptions { get; } = ParameterOrderParser.Parse(ParameterOrderFlat);
    }
}
