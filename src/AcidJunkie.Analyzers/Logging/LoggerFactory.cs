using AcidJunkie.Analyzers.Configuration;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Logging;

internal static class LoggerFactory
{
    public static ILogger<TContext>? CreateLogger<TContext>(in SyntaxNodeAnalysisContext context)
        where TContext : class
    {
        var logLevel = GeneralConfigurationManager.GetLogLevel(context);
        return logLevel == LogLevel.None
            ? null
            : new DefaultLogger<TContext>(logLevel);
    }
}
