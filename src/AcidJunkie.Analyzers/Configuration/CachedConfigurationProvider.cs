using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal sealed class CachedConfigurationProvider<T> : IConfigurationProvider<T>
    where T : class
{
    private readonly object _lock = new();
    private readonly IConfigurationProvider<T> _innerConfigurationProvider;
    private DateTime _lastPublished = DateTime.MinValue;
    private static readonly TimeSpan ValidityPeriod = TimeSpan.FromSeconds(10);
    private T? _config;

    public CachedConfigurationProvider(IConfigurationProvider<T> innerConfigurationProvider)
    {
        _innerConfigurationProvider = innerConfigurationProvider;
    }

    public T GetConfiguration(AnalyzerOptions options)
    {
        return GetAndReloadIfRequired(options);
    }

    private T GetAndReloadIfRequired(AnalyzerOptions options)
    {
        if (IsReloadRequired())
        {
            lock (_lock)
            {
                if (IsReloadRequired())
                {
                    _lastPublished = DateTime.UtcNow;
                    _config = _innerConfigurationProvider.GetConfiguration(options);
                }
            }
        }

        return _config!;

        bool IsReloadRequired() => _config is null || DateTime.UtcNow > _lastPublished + ValidityPeriod;
    }
}

internal static class ConfigurationManager
{
    private static CachedConfigurationProvider<Aj0002Configuration>? Aj0002Provider;

    public static Aj0002Configuration GetAj0002Configuration(AnalyzerOptions options)
    {
        Aj0002Provider ??= new CachedConfigurationProvider<Aj0002Configuration>(new Aj0002ConfigurationProvider());
        return Aj0002Provider.GetConfiguration(options);
    }
}
