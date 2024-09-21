using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal sealed class Aj0002ConfigurationProvider : IConfigurationProvider<Aj0002Configuration>
{
    private static readonly char[] IgnoredObjectNamesDelimiter = ['|'];

    public Aj0002Configuration GetConfiguration(AnalyzerOptions options)
    {
        var isEnabled = options.GetGlobalOptionsBooleanValue(Aj0002Configuration.KeyNames.IsEnabled);
        if (!isEnabled)
        {
            return Aj0002Configuration.Disabled;
        }

        var ignoredObjectTypesFlat = options.GetGlobalOptionsValueOrDefault(Aj0002Configuration.KeyNames.IgnoredObjectNames);
        if (ignoredObjectTypesFlat.IsNullOrWhiteSpace())
        {
            return Aj0002Configuration.Default;
        }

        var ignoredObjectTypes = ignoredObjectTypesFlat
            .Replace("{default}", Aj0002Configuration.Defaults.IgnoredObjectsFlat)
            .Split(IgnoredObjectNamesDelimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .Where(a => a.Length > 0)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        return new(isEnabled: true, ignoredObjectTypes);
    }
}
