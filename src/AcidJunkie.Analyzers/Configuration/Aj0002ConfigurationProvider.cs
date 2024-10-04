using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal sealed class Aj0002ConfigurationProvider : IConfigurationProvider<Aj0002Configuration>
{
    private static readonly char[] IgnoredObjectNamesDelimiter = ['|'];

    public (Aj0002Configuration, bool) GetConfiguration(AnalyzerOptions options)
    {
        if (!IsEnabled(options))
        {
            return (Aj0002Configuration.Disabled, false);
        }

        var ignoredObjectTypes = GetIgnoredObjectTypes(options);

        var isDefault = ignoredObjectTypes.Count == 0;
        if (isDefault)
        {
            return (Aj0002Configuration.Default, true);
        }

        var configuration = new Aj0002Configuration(isEnabled: true, ignoredObjectTypes);
        return (configuration, false);
    }

    private static ImmutableHashSet<string> GetIgnoredObjectTypes(AnalyzerOptions options)
    {
        var value = options.GetGlobalOptionsValueOrDefault(Aj0002Configuration.KeyNames.IgnoredObjectNames);
        if (value.IsNullOrWhiteSpace())
        {
            return ImmutableHashSet<string>.Empty;
        }

        return value
            .Replace("{default}", Aj0002Configuration.Defaults.IgnoredObjectsFlat)
            .Split(IgnoredObjectNamesDelimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .Where(a => a.Length > 0)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsEnabled(AnalyzerOptions options) => options.GetGlobalOptionsBooleanValue(Aj0002Configuration.KeyNames.IsEnabled);
}
