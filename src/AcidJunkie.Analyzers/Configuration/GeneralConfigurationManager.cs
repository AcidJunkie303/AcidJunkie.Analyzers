using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class GeneralConfigurationManager
{
    public static bool IsLoggingEnabled(AnalyzerOptions options)
        => options.GetGlobalOptionsBooleanValue(KeyNames.LoggingEnabled, defaultValue: false);

    private static class KeyNames
    {
        public const string LoggingEnabled = "AJ.logging_enabled";
    }
}
