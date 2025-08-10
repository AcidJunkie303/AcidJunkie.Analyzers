using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterialisedCollectionAsEnumerable;

namespace AcidJunkie.Analyzers.Logging;

#pragma warning disable

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035: Do not use APIs banned for analyzers.", Justification = "We need to do file system access for logging")]
internal static class DefaultLogger
{
    private const string ProcessIdPlaceholder = "{ProcessId}";
    private const string ThreadIdPlaceholder = "{ThreadId}";
    private const string LogFileNamePattern = $"AcidJunkie.Analyzers.PDI-{ProcessIdPlaceholder}.TID-{ThreadIdPlaceholder}.log";

    public static int ProcessId { get; } = GetCurrentProcessId();
    public static int MaxAnalyzerClassNameLength { get; } = GetMaxAnalyzerClassNameLength();
    public static string LogDirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "AcidJunkie.Analyzers");

    public static string LogFilePath
    {
        get
        {
            var logFileName = LogFileNamePattern.Replace(ProcessIdPlaceholder, ProcessId.ToString(CultureInfo.InvariantCulture))
                                                .Replace(ThreadIdPlaceholder, Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));

            return Path.Combine(LogDirectoryPath, logFileName);
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
        => nameof(TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzerImplementation).Length;
}

internal sealed class DefaultLogger<TContext> : ILogger<TContext>
    where TContext : class
{
    public bool IsLoggingEnabled => true;

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035: Do not use APIs banned for analyzers.", Justification = "We need to do file system access for logging")]
    public void WriteLine(Func<string> messageFactory, [CallerMemberName] string memberName = "")
    {
        var message = messageFactory();
        var line = $"{DateTime.UtcNow:u} PID={DefaultLogger.ProcessId,-8} TID={Environment.CurrentManagedThreadId,-8} Context={typeof(TContext).Name.PadRight(DefaultLogger.MaxAnalyzerClassNameLength)} Method={memberName} Message={message}{Environment.NewLine}";

        EnsureLogDirectoryExists();
        File.AppendAllText(DefaultLogger.LogFilePath, line);
    }

    private static void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(DefaultLogger.LogDirectoryPath))
        {
            Directory.CreateDirectory(DefaultLogger.LogDirectoryPath);
        }
    }
}
