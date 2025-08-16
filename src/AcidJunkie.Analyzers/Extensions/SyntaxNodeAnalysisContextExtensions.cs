using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class SyntaxNodeAnalysisContextExtensions
{
    private static readonly FrozenDictionary<string, bool> BooleanValuesByValue = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
    {
        {
            "false", false
        },
        {
            "0", false
        },
        {
            "disable", false
        },
        {
            "disabled", false
        },
        {
            "no", false
        },
        {
            "true", true
        },
        {
            "1", true
        },
        {
            "enable", true
        },
        {
            "enabled", true
        },
        {
            "yes", true
        }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static ILogger<TAnalyzer> CreateLogger<TAnalyzer>(this SyntaxNodeAnalysisContext analysisContext)
        where TAnalyzer : class
        => LoggerFactory.CreateLogger<TAnalyzer>(analysisContext);

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to get the type argument for the logger")]
    public static ILogger<TAnalyzer> CreateLogger<TAnalyzer>(this SyntaxNodeAnalysisContext analysisContext, TAnalyzer analyzer)
        where TAnalyzer : class
        => LoggerFactory.CreateLogger<TAnalyzer>(analysisContext);

    public static string? GetOptionsValueOrDefault(this SyntaxNodeAnalysisContext context, string key)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        options.TryGetValue(key, out var value);
        return value;
    }

    public static bool GetOptionsBooleanValue(this SyntaxNodeAnalysisContext context, string key, bool defaultValue = true)
    {
        var value = context.GetOptionsValueOrDefault(key);
        if (value.IsNullOrWhiteSpace())
        {
            return defaultValue;
        }

        return BooleanValuesByValue.TryGetValue(value, out var result)
            ? result
            : defaultValue;
    }
}
