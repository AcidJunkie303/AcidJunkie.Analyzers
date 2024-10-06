using System.Collections.Immutable;

namespace AcidJunkie.Analyzers.Configuration;

public sealed class Aj0002Configuration
{
    public static Aj0002Configuration Default { get; } = new(true, Defaults.IgnoredObjects);
    public static Aj0002Configuration Disabled { get; } = new(false, ImmutableHashSet<string>.Empty);

    public bool IsEnabled { get; }

    public ImmutableHashSet<string> IgnoredObjects { get; }

    public Aj0002Configuration(bool isEnabled, ImmutableHashSet<string> ignoredObjects)
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

        public static readonly ImmutableHashSet<string> IgnoredObjects = new[]
        {
            "a", "b"
        }.ToImmutableHashSet(StringComparer.Ordinal);
    }
}
