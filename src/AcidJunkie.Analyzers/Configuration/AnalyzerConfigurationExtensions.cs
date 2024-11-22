namespace AcidJunkie.Analyzers.Configuration;

internal static class AnalyzerConfigurationExtensions
{
    public static bool IsValid(this IAnalyzerConfiguration configuration) => configuration.ConfigurationError is null;
}
