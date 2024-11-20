namespace AcidJunkie.Analyzers.Configuration;

internal interface IAnalyzerConfiguration
{
    ConfigurationError? ConfigurationError { get; }
}
