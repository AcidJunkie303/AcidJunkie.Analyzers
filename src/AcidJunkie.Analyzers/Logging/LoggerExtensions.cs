using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Logging;

internal static class LoggerExtensions
{
    public static void LogReportDiagnostic<TAnalyzer>(this ILogger<TAnalyzer> logger, DiagnosticDescriptor rule, Location location, params object?[] messageArgs)
        where TAnalyzer : DiagnosticAnalyzer => logger.WriteLine(()
        => $"Reporting diagnostic {rule.Id} at location {location} with the following message arguments: {string.Join(", ", messageArgs)}");

    public static void LogAnalyzerIsDisabled<TAnalyzer>(this ILogger<TAnalyzer> logger)
        where TAnalyzer : DiagnosticAnalyzer
        => logger.WriteLine(() => "The analyzer is disabled");
}
