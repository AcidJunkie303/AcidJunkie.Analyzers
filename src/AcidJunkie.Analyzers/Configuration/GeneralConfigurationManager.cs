using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class GeneralConfigurationManager
{
    private const string LoggingTypeKeyName = "AcidJunkie_Analyzers.log_level";

    private static readonly ImmutableDictionary<string, LogLevel> LoggingTypesByConfigValue = ImmutableDictionary.CreateRange
    (
        StringComparer.OrdinalIgnoreCase,
        new[]
        {
            new KeyValuePair<string, LogLevel>(nameof(LogLevel.Full), LogLevel.Full),
            new KeyValuePair<string, LogLevel>("true", LogLevel.Full),
            new KeyValuePair<string, LogLevel>("all", LogLevel.Full),
            new KeyValuePair<string, LogLevel>(nameof(LogLevel.Duration), LogLevel.Duration)
        }
    );

    public static LogLevel GetLogLevel(in SyntaxNodeAnalysisContext context)
    {
        var value = context.GetOptionsValueOrDefault(LoggingTypeKeyName);
        return value.IsNullOrWhiteSpace()
            ? LogLevel.None
            : LoggingTypesByConfigValue.GetValueOrDefault(value, LogLevel.None);
    }
}
