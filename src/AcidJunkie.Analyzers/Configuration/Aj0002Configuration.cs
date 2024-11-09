using System.Collections.Frozen;
using System.Collections.Immutable;

namespace AcidJunkie.Analyzers.Configuration;

public sealed class Aj0002Configuration
{
    public static Aj0002Configuration Default { get; } = new(true, Defaults.IgnoredObjects);
    public static Aj0002Configuration Disabled { get; } = new(false, FrozenSet<string>.Empty);

    public bool IsEnabled { get; }

    public FrozenSet<string> IgnoredObjects { get; }

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
