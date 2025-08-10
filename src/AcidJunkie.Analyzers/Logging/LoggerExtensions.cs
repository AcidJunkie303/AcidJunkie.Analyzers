using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Logging;

internal static class LoggerExtensions
{
    // TODO: remove
    public static void ReportDiagnostic<TAnalyzer>(this ILogger<TAnalyzer> logger, DiagnosticDescriptor rule, Location location, params object?[] messageArgs)
        where TAnalyzer : DiagnosticAnalyzer => logger.WriteLine(()
        => $"Reporting diagnostic {rule.Id} at location {location} with the following message arguments: {string.Join(", ", messageArgs)}");

    public static void ReportDiagnostic2<TAnalyzer>(this ILogger<TAnalyzer> logger, DiagnosticDescriptor rule, Location location, params object?[] messageArgs)
        where TAnalyzer : class
        => logger.WriteLine(() => $"Reporting diagnostic {rule.Id} at location {location} with the following message arguments: {string.Join(", ", messageArgs)}");

    public static void AnalyzerIsDisabled<TAnalyzer>(this ILogger<TAnalyzer> logger)
        where TAnalyzer : class
        => logger.WriteLine(static () => "The analyzer is disabled");
}
