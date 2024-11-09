using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class SyntaxNodeAnalysisContextExtensions
{
    public static ILogger<TContext> CreateLogger<TContext>(this SyntaxNodeAnalysisContext analysisContext)
        where TContext : class
        => LoggerFactory.CreateLogger<TContext>(analysisContext);
}
