namespace AcidJunkie.Analyzers.Configuration;

public sealed class ConfigurationError
{
    public string KeyName { get; }
    public string FilePath { get; }
    public string Reason { get; }

    public ConfigurationError(string keyName, string filePath, string reason)
    {
        KeyName = keyName;
        FilePath = filePath;
        Reason = reason;
    }

    public static ConfigurationError CreateWithDefaultEditorConfigFile(string keyName, string reason)
        => new(keyName, Constants.EditorConfigFileName, reason);
}
