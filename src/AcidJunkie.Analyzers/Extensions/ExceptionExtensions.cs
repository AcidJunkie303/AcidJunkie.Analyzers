using AcidJunkie.Analyzers.Configuration;

namespace AcidJunkie.Analyzers.Extensions;

internal static class ExceptionExtensions
{
    public static ConfigurationError CreateConfigurationError(this Exception exception, string keyName)
        => new(keyName, Constants.EditorConfigFileName, exception.Message);
}
