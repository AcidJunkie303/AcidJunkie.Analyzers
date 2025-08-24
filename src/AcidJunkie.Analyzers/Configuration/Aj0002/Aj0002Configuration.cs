namespace AcidJunkie.Analyzers.Configuration.Aj0002;

internal sealed class Aj0002Configuration : IAnalyzerConfiguration
{
    public static Aj0002Configuration Default { get; } = new(true, Mode.Strict);
    public static Aj0002Configuration Disabled { get; } = new(false, Mode.Strict);
    public static Aj0002Configuration Strict { get; } = new(true, Mode.Strict);
    public static Aj0002Configuration Relaxed { get; } = new(true, Mode.Relaxed);

    public bool IsEnabled { get; }
    public Mode Mode { get; }
    public ConfigurationError? ConfigurationError { get; }

    public Aj0002Configuration(bool isEnabled, Mode mode)
    {
        IsEnabled = isEnabled;
        Mode = mode;
    }

    public Aj0002Configuration(ConfigurationError configurationError)
    {
        ConfigurationError = configurationError;
        IsEnabled = false;
    }

    public static class KeyNames
    {
        public const string IsEnabled = "AJ0002.is_enabled";
        public const string ParameterOrderingFlat = "AJ0002.mode";
    }
}
