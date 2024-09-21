using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal interface IConfigurationProvider<out T>
{
    T GetConfiguration(AnalyzerOptions options);
}
