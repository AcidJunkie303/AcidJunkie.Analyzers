using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class ConfigurationManager
{
    private static CachedConfigurationProvider<Aj0002Configuration>? Aj0002Provider;

    public static Aj0002Configuration GetAj0002Configuration(AnalyzerOptions options)
    {
        Aj0002Provider ??= new CachedConfigurationProvider<Aj0002Configuration>(new Aj0002ConfigurationProvider());
        return Aj0002Provider.GetConfiguration(options).Configuration;
    }
}
