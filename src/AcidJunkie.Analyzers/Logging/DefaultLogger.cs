using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Logging;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035: Do not use APIs banned for analyzers.", Justification = "We need to do file system access for logging")]
internal static class DefaultLogger
{
    public const string LogFileName = "AcidJunkie.Analyzers.log";
    public static int ProcessId { get; } = GetCurrentProcessId();
    public static int MaxAnalyzerClassNameLength { get; } = GetMaxAnalyzerClassNameLength();
    public static string LogFilePath { get; } = Path.Combine(Path.GetTempPath(), LogFileName);

    private static int GetCurrentProcessId()
    {
        using var currentProcess = Process.GetCurrentProcess();
        return currentProcess.Id;
    }

    private static int GetMaxAnalyzerClassNameLength()
        => Assembly
            .GetAssembly(typeof(MissingEqualityComparerAnalyzer))!
            .GetTypes()
            .Where(a => !a.IsAbstract)
            .Where(IsAnalyzerClass)
            .Max(a => a.Name.Length);

    private static bool IsAnalyzerClass(Type type)
    {
        if (type.BaseType is null)
        {
            return false;
        }

#pragma warning disable MA0026
        // TODO: add code fixers as well
#pragma warning restore MA0026
        return type.BaseType == typeof(DiagnosticAnalyzer) || IsAnalyzerClass(type.BaseType);
    }
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

        using var _ = DefaultLoggerLockProvider.AcquireLock();
        File.AppendAllText(DefaultLogger.LogFilePath, line);
    }
}
