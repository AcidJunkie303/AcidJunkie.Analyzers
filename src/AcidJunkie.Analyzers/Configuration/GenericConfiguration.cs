namespace AcidJunkie.Analyzers.Configuration;

internal sealed class GenericConfiguration : IAnalyzerConfiguration
{
    public static GenericConfiguration Enabled { get; } = new(true);
    public static GenericConfiguration Disabled { get; } = new(false);

    public ConfigurationError? ConfigurationError { get; }
    public bool IsEnabled { get; }

    public GenericConfiguration(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
