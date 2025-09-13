using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterializedCollectionAsEnumerable;
using AcidJunkie.Analyzers.Extensions;

namespace AcidJunkie.Analyzers.Logging;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035: Do not use APIs banned for analyzers.", Justification = "We need to do file system access for logging")]
internal static class DefaultLogger
{
    private const string ProcessIdPlaceholder = "{ProcessId}";
    private const string ThreadIdPlaceholder = "{ThreadId}";

    [ThreadStatic]
    private static string? ThreadStaticFilePath;

    [ThreadStatic]
    private static string? ThreadStaticDurationMeasurementFilePath;

    public static int ProcessId { get; } = GetCurrentProcessId();
    public static int MaxAnalyzerClassNameLength { get; } = GetMaxAnalyzerClassNameLength();
    public static string LogDirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "AcidJunkie.Analyzers");

    public static string FilePath
    {
        get
        {
            const string logFileNamePattern = $"AcidJunkie.Analyzers.PDI-{ProcessIdPlaceholder}.TID-{ThreadIdPlaceholder}.log";

            if (!ThreadStaticFilePath.IsNullOrWhiteSpace())
            {
                return ThreadStaticFilePath;
            }

            var logFileName = logFileNamePattern.Replace(ProcessIdPlaceholder, ProcessId.ToString(CultureInfo.InvariantCulture))
                                                .Replace(ThreadIdPlaceholder, Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));

            return ThreadStaticFilePath = Path.Combine(LogDirectoryPath, logFileName);
        }
    }

    public static string DurationMeasurementFilePath
    {
        get
        {
            const string durationMeasurementLogFileNamePattern = $"AcidJunkie.Analyzers.DurationMeasurement.PDI-{ProcessIdPlaceholder}.TID-{ThreadIdPlaceholder}.log";

            if (!ThreadStaticDurationMeasurementFilePath.IsNullOrWhiteSpace())
            {
                return ThreadStaticDurationMeasurementFilePath;
            }

            var logFileName = durationMeasurementLogFileNamePattern.Replace(ProcessIdPlaceholder, ProcessId.ToString(CultureInfo.InvariantCulture))
                                                                   .Replace(ThreadIdPlaceholder, Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));

            return ThreadStaticDurationMeasurementFilePath = Path.Combine(LogDirectoryPath, logFileName);
        }
    }

    public static void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(LogDirectoryPath))
        {
            Directory.CreateDirectory(LogDirectoryPath);
        }
    }

    private static int GetCurrentProcessId()
    {
        using var currentProcess = Process.GetCurrentProcess();
        return currentProcess.Id;
    }

    private static int GetMaxAnalyzerClassNameLength()
        // That's the longest class name we have in the analyzers right now
        // doing the very same using reflection fails because some DLLs cannot be loaded in the analyzer context.
        // No idea why... Therefore, we go with this simple approach.
        => nameof(TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerImplementation).Length;
}

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
[SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal sealed class DefaultLogger<TContext> : ILogger<TContext>
    where TContext : class
{
    public LogLevel LogLevel { get; }
    public bool IsLoggingEnabled => true;

    public DefaultLogger(LogLevel logLevel)
    {
        LogLevel = logLevel;
    }

    public void WriteLine(LogLevel logLevel, string message, [CallerMemberName] string memberName = "")
        => WriteLineCore(logLevel, message, memberName);

    private static string GetLogFilePath(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Full     => DefaultLogger.FilePath,
        LogLevel.Duration => DefaultLogger.DurationMeasurementFilePath,
        _                 => throw new InvalidOperationException($"The logger type '{logLevel}' is not known")
    };

    [SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider")]
    [SuppressMessage("Major Code Smell", "S6566:Use \"DateTimeOffset\" instead of \"DateTime\"")]
    private void WriteLineCore(LogLevel logLevel, string message, string memberName)
    {
        if (logLevel < LogLevel)
        {
            return;
        }

        var line = $"{DateTime.UtcNow:u} PID={DefaultLogger.ProcessId,-8} TID={Environment.CurrentManagedThreadId,-8} Context={typeof(TContext).Name.PadRight(DefaultLogger.MaxAnalyzerClassNameLength)} Method={memberName} Message={message}{Environment.NewLine}";

        DefaultLogger.EnsureLogDirectoryExists();
        File.AppendAllText(GetLogFilePath(logLevel), line);
    }
}
