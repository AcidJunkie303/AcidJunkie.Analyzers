namespace AcidJunkie.Analyzers.Configuration;

public sealed class ConfigurationError
{
    public string EntryName { get; }
    public string FilePath { get; }
    public string Reason { get; }

    public ConfigurationError(string entryName, string filePath, string reason)
    {
        EntryName = entryName;
        FilePath = filePath;
        Reason = reason;
    }
}
