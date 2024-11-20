using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal interface IConfigurationProvider<T>
    where T : IAnalyzerConfiguration
{
    (T Configuration, bool CanBeCached) GetConfiguration(AnalyzerOptions options);
}
