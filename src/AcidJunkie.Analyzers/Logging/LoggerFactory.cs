using AcidJunkie.Analyzers.Configuration;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Logging;

internal static class LoggerFactory
{
    public static ILogger<TContext> CreateLogger<TContext>(SyntaxNodeAnalysisContext context)
        where TContext : class
        => GeneralConfigurationManager.IsLoggingEnabled(context)
            ? new DefaultLogger<TContext>()
            : NullLogger<TContext>.Default;
}
