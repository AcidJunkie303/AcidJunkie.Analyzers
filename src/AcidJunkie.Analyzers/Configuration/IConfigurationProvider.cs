using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration;

internal interface IConfigurationProvider<T>
{
    (T Configuration, bool IsDefault) GetConfiguration(AnalyzerOptions options);
}
