using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Logging;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
[SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")]
[SuppressMessage("Major Code Smell", "S6566:Use \"DateTimeOffset\" instead of \"DateTime\"")]
internal static class LoggerExtensions
{
    public static void ReportDiagnostic<TAnalyzer>(this ILogger<TAnalyzer>? logger, DiagnosticDescriptor rule, Location location, params object?[] messageArgs)
        where TAnalyzer : class
        => logger.WriteLine(LogLevel.Full, $"Reporting diagnostic {rule.Id} at location {location} with the following message arguments: {string.Join(", ", messageArgs)}");

    public static void AnalyzerIsDisabled<TAnalyzer>(this ILogger<TAnalyzer>? logger)
        where TAnalyzer : class
        => logger.WriteLine(LogLevel.Full, "The analyzer is disabled");

    public static void WriteLine<TContext>(this ILogger<TContext>? logger, LogLevel logLevel, string message, [CallerMemberName] string memberName = "")
        where TContext : class
    {
        if (logger is null || logger.LogLevel < logLevel)
        {
            return;
        }

        var line = $"{DateTime.UtcNow:u} PID={DefaultLogger.ProcessId,-8} TID={Environment.CurrentManagedThreadId,-8} Context={typeof(TContext).Name.PadRight(DefaultLogger.MaxAnalyzerClassNameLength)} Method={memberName} Message={message}{Environment.NewLine}";

        DefaultLogger.EnsureLogDirectoryExists();
        File.AppendAllText(GetLogFilePath(logLevel), line);
    }

    private static string GetLogFilePath(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Full     => DefaultLogger.FilePath,
        LogLevel.Duration => DefaultLogger.DurationMeasurementFilePath,
        _                 => throw new InvalidOperationException($"The logger type '{logLevel}' is not known")
    };
}
