using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal abstract class ConfigurationProviderBase<T> : IConfigurationProvider<T>
    where T : IAnalyzerConfiguration
{
    public T GetConfiguration(SyntaxNodeAnalysisContext context)
    {
        var configuration = GetConfigurationCore(context);
        if (configuration.ConfigurationError is not null)
        {
            context.ReportValidationError(configuration.ConfigurationError);
        }

        return configuration;
    }

    protected abstract T GetConfigurationCore(in SyntaxNodeAnalysisContext context);
}
