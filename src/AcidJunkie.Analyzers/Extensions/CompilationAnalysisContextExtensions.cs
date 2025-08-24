using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Diagnosers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace AcidJunkie.Analyzers.Extensions;

internal static class CompilationAnalysisContextExtensions
{
    private const string EditorConfigFileName = ".editorconfig";

    public static void ReportValidationError(this in SyntaxNodeAnalysisContext context, ConfigurationError error)
    {
        var path = context.Options.AdditionalFiles.FirstOrDefault(a => a.Path.Contains(EditorConfigFileName))?.Path ?? EditorConfigFileName;
        var location = Location.Create(path, TextSpan.FromBounds(0, 0), new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)));
        var rule = Diagnostic.Create(CommonRules.InvalidConfigurationValue.Rule, location, error.KeyName, error.FilePath, error.Reason);
        context.ReportDiagnostic(rule);
    }
}
