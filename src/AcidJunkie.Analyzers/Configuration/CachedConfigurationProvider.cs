using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class CachedConfigurationProvider
{
    public static readonly TimeSpan ValidityPeriod = TimeSpan.FromSeconds(10);
    public static bool IsCachingEnabled { get; set; }
}

internal sealed class CachedConfigurationProvider<T> : IConfigurationProvider<T>
    where T : class, IAnalyzerConfiguration
{
    private readonly IConfigurationProvider<T> _innerConfigurationProvider;
    private readonly object _lock = new();
    private T? _config;
    private DateTime _lastPublished = DateTime.MinValue;

    public CachedConfigurationProvider(IConfigurationProvider<T> innerConfigurationProvider)
    {
        _innerConfigurationProvider = innerConfigurationProvider;
    }

    public (T Configuration, bool CanBeCached) GetConfiguration(AnalyzerOptions options) => GetAndReloadIfRequired(options);

    private (T Configuration, bool CanBeCached) GetAndReloadIfRequired(AnalyzerOptions options)
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
                    var (configuration, canBeCached) = _innerConfigurationProvider.GetConfiguration(options);
                    // if it is the default settings, do not cache it.
                    // this is because some weird behavior that at the first time the analyzer loads, there's no configuration. Therefore, we don't cache this false data
                    if (!canBeCached)
                    {
                        return (configuration, canBeCached);
                    }

                    _lastPublished = DateTime.UtcNow;
                    _config = configuration;

                    return (_config!, false);
                }
            }
        }

        return (_config!, false);

        bool IsReloadRequired()
        {
            return _config is null || DateTime.UtcNow > _lastPublished + CachedConfigurationProvider.ValidityPeriod;
        }
    }
}
