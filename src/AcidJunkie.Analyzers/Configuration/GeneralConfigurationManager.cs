using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal static class GeneralConfigurationManager
{
    private const string LoggingEnabledKeyName = "AcidJunkie_Analyzers.is_logging_enabled";

    public static bool IsLoggingEnabled(in SyntaxNodeAnalysisContext context)
        => context.GetOptionsBooleanValue(LoggingEnabledKeyName);
}
