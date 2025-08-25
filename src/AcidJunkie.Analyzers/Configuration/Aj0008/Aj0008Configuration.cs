namespace AcidJunkie.Analyzers.Configuration.Aj0008;

internal sealed class Aj0008Configuration : IAnalyzerConfiguration
{
    public static Aj0008Configuration Default { get; } = new(true, MethodKinds.OnInitialized | MethodKinds.OnInitializedAsync);
    public static Aj0008Configuration Disabled { get; } = new(false, MethodKinds.None);

    public bool IsEnabled { get; }
    public ConfigurationError? ConfigurationError { get; }
    public MethodKinds MethodsToCheck { get; }

    public Aj0008Configuration(bool isEnabled, MethodKinds methodsToCheck)
    {
        IsEnabled = isEnabled;
        MethodsToCheck = methodsToCheck;
    }

    public Aj0008Configuration(ConfigurationError configurationError)
    {
        ConfigurationError = configurationError;
        IsEnabled = false;
        MethodsToCheck = MethodKinds.None;
    }

    public static class KeyNames
    {
        public const string IsEnabled = "AJ0008.is_enabled";
        public const string MethodsToCheck = "AJ0008.methods_to_check";
    }
}
