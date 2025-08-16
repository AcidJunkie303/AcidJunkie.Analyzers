using System.Collections.Frozen;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0002;

internal sealed class Aj0002ConfigurationProvider : IConfigurationProvider<Aj0002Configuration>
{
    private static readonly char[] IgnoredObjectNamesDelimiter = ['|'];
    public static Aj0002ConfigurationProvider Instance { get; } = new();

    public Aj0002Configuration GetConfiguration(SyntaxNodeAnalysisContext context)
    {
        if (!IsEnabled(context))
        {
            return Aj0002Configuration.Disabled;
        }

        var ignoredObjectTypes = GetIgnoredObjectTypes(context);

        var isDefault = ignoredObjectTypes.Count == 0;
        if (isDefault)
        {
            return Aj0002Configuration.Default;
        }

        var configuration = new Aj0002Configuration(true, ignoredObjectTypes);
        return configuration;
    }

    private static FrozenSet<string> GetIgnoredObjectTypes(SyntaxNodeAnalysisContext context)
    {
        var value = context.GetOptionsValueOrDefault(Aj0002Configuration.KeyNames.IgnoredObjectNames);
        if (value.IsNullOrWhiteSpace())
        {
            return FrozenSet<string>.Empty;
        }

        return value
              .Replace("{default}", Aj0002Configuration.Defaults.IgnoredObjectsFlat)
              .Split(IgnoredObjectNamesDelimiter, StringSplitOptions.RemoveEmptyEntries)
              .Select(static a => a.Trim())
              .Where(static a => a.Length > 0)
              .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsEnabled(SyntaxNodeAnalysisContext context)
        => context.GetOptionsBooleanValue(Aj0002Configuration.KeyNames.IsEnabled);
}
