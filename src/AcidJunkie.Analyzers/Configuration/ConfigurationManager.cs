using AcidJunkie.Analyzers.Configuration.Aj0002;
using AcidJunkie.Analyzers.Configuration.Aj0007;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class ConfigurationManager
{
    private static CachedConfigurationProvider<Aj0002Configuration>? Aj0002Provider;
    private static CachedConfigurationProvider<Aj0007Configuration>? Aj0007Provider;

    public static Aj0002Configuration GetAj0002Configuration(AnalyzerOptions options)
    {
        Aj0002Provider ??= new CachedConfigurationProvider<Aj0002Configuration>(new Aj0002ConfigurationProvider());
        return Aj0002Provider.GetConfiguration(options).Configuration;
    }

    public static Aj0007Configuration GetAj0007Configuration(AnalyzerOptions options)
    {
        Aj0007Provider ??= new CachedConfigurationProvider<Aj0007Configuration>(new Aj0007ConfigurationProvider());
        return Aj0007Provider.GetConfiguration(options).Configuration;
    }
}
