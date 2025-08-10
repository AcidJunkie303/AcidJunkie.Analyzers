using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal interface IConfigurationProvider<out T>
    where T : IAnalyzerConfiguration
{
    T GetConfiguration(SyntaxNodeAnalysisContext context);
}
