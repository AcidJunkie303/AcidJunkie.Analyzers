using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class SyntaxNodeAnalysisContextExtensions
{
    public static ILogger<TAnalyzer>? CreateLogger<TAnalyzer>(this in SyntaxNodeAnalysisContext analysisContext)
        where TAnalyzer : class
        => LoggerFactory.CreateLogger<TAnalyzer>(analysisContext);

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to get the type argument for the logger")]
    public static ILogger<TAnalyzer>? CreateLogger<TAnalyzer>(this in SyntaxNodeAnalysisContext analysisContext, TAnalyzer _)
        where TAnalyzer : class
        => LoggerFactory.CreateLogger<TAnalyzer>(analysisContext);

    public static string? GetOptionsValueOrDefault(this in SyntaxNodeAnalysisContext context, string key)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        return options.GetOptionsValueOrDefault(key);
    }

    public static bool GetOptionsBooleanValue(this in SyntaxNodeAnalysisContext context, string key, bool defaultValue)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        return options.GetOptionsBooleanValue(key, defaultValue);
    }

    public static bool IsDiagnosticEnabled(this in SyntaxNodeAnalysisContext context, string diagnosticId)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        return options.IsDiagnosticEnabled(diagnosticId);
    }
}
