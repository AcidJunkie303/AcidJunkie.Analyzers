using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class CachedConfigurationProvider
{
    public static readonly TimeSpan ValidityPeriod = TimeSpan.FromSeconds(10);
    public static bool IsCachingEnabled { get; set; }
}

internal sealed class CachedConfigurationProvider<T> : IConfigurationProvider<T>
    where T : class
{
    private readonly object _lock = new();
    private readonly IConfigurationProvider<T> _innerConfigurationProvider;
    private DateTime _lastPublished = DateTime.MinValue;
    private T? _config;

    public CachedConfigurationProvider(IConfigurationProvider<T> innerConfigurationProvider)
    {
        _innerConfigurationProvider = innerConfigurationProvider;
    }

    public (T Configuration, bool IsDefault) GetConfiguration(AnalyzerOptions options)
    {
        return GetAndReloadIfRequired(options);
    }

    private (T Configuration, bool IsDefault) GetAndReloadIfRequired(AnalyzerOptions options)
    {
        if (!CachedConfigurationProvider.IsCachingEnabled)
        {
            return _innerConfigurationProvider.GetConfiguration(options);
        }

        if (IsReloadRequired())
        {
            lock (_lock)
            {
                if (IsReloadRequired())
                {
                    var (configuration, isDefault) = _innerConfigurationProvider.GetConfiguration(options);
                    // if it is the default settings, do not cache it
                    // this is because some weird behavior that at the first time the analyzer loads, there's no configuration. Therefore, we don't cache this false data
                    if (isDefault)
                    {
                        return (configuration, isDefault);
                    }

                    _lastPublished = DateTime.UtcNow;
                    _config = configuration;
                }
            }
        }

        return (_config!, false);

        bool IsReloadRequired() => _config is null || DateTime.UtcNow > _lastPublished + CachedConfigurationProvider.ValidityPeriod;
    }
}
