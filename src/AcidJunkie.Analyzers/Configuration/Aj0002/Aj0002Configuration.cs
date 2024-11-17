using System.Collections.Frozen;

namespace AcidJunkie.Analyzers.Configuration.Aj0002;

internal sealed class Aj0002Configuration : IAnalyzerConfiguration
{
    public static Aj0002Configuration Default { get; } = new(true, Defaults.IgnoredObjects);
    public static Aj0002Configuration Disabled { get; } = new(false, FrozenSet<string>.Empty);

    public bool IsEnabled { get; }
    public FrozenSet<string> IgnoredObjects { get; }
    public string? ValidationError => null;

    public Aj0002Configuration(bool isEnabled, FrozenSet<string> ignoredObjects)
    {
        IsEnabled = isEnabled;
        IgnoredObjects = ignoredObjects;
    }

    public static class KeyNames
    {
        public const string IsEnabled = "AJ0002.is_enabled";
        public const string IgnoredObjectNames = "AJ0002.ignored_object_types";
    }

    public static class Defaults
    {
        public const string IgnoredObjectsFlat = "";

        public static readonly FrozenSet<string> IgnoredObjects = FrozenSet<string>.Empty;
    }
}
