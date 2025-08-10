using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class SyntaxNodeAnalysisContextExtensions
{
    public static ILogger<TAnalyzer> CreateLogger<TAnalyzer>(this SyntaxNodeAnalysisContext analysisContext)
        where TAnalyzer : class
        => LoggerFactory.CreateLogger<TAnalyzer>(analysisContext);

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to get the type argument for the logger")]
    public static ILogger<TAnalyzer> CreateLogger<TAnalyzer>(this SyntaxNodeAnalysisContext analysisContext, TAnalyzer analyzer)
        where TAnalyzer : class
        => LoggerFactory.CreateLogger<TAnalyzer>(analysisContext);
}
