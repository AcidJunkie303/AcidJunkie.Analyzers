using AcidJunkie.Analyzers.Diagnosers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AcidJunkie.Analyzers.Configuration;

internal interface IAnalyzerConfiguration
{
    string? ValidationError { get; }
}

public static class AnalyzerConfigurationExtensions
{
    public static void ReportValidationError(this IAnalyzerConfiguration configuration, SyntaxNodeAnalysisContext context) => ReportValidationError(configuration, context.Options, context.ReportDiagnostic);

    public static void ReportValidationError(this IAnalyzerConfiguration configuration, CompilationAnalysisContext context) => ReportValidationError(configuration, context.Options, context.ReportDiagnostic);

    private static void ReportValidationError(IAnalyzerConfiguration configuration, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic)
    {
        if (configuration.ValidationError is null)
        {
            return;
        }

        var path = options.AdditionalFiles.FirstOrDefault(a => a.Path.Contains(".globalconfig"))?.Path ?? ".globalconfig";
        var location = Location.Create(path, TextSpan.FromBounds(0, 0), new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)));

        var rule = Diagnostic.Create(CommonRules.InvalidConfigurationValue.Rule, location, "config-key", path, configuration.ValidationError);
    }
}
//       errors.Add(Diagnostic.Create(RuleInvalid, CreateLocation(file, sourceText, line), typeName));
