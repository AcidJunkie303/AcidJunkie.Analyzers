using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal static class ConfigurationProvider
{
    public static TConfiguration GetConfiguration<TProvider, TConfiguration>(in SyntaxNodeAnalysisContext context, TProvider provider)
        where TProvider : class, IConfigurationProvider<TConfiguration>, new()
        where TConfiguration : class, IAnalyzerConfiguration
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        return provider.GetConfiguration(options);
    }

    public static async Task<TConfiguration?> GetConfigurationAsync<TProvider, TConfiguration>(CodeFixContext context)
        where TProvider : class, IConfigurationProvider<TConfiguration>, new()
        where TConfiguration : class, IAnalyzerConfiguration
    {
        var syntaxTree = await context.Document.GetSyntaxTreeAsync().ConfigureAwait(false);
        if (syntaxTree is null)
        {
            return null;
        }

        var options = context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
        return new TProvider().GetConfiguration(options);
    }
}

internal abstract class ConfigurationProvider<T> : IConfigurationProvider<T>
    where T : class, IAnalyzerConfiguration
{
    public async Task<T?> GetConfigurationAsync(CodeFixContext context)
    {
        var syntaxTree = await context.Document.GetSyntaxTreeAsync().ConfigureAwait(false);
        if (syntaxTree is null)
        {
            return null;
        }

        var options = context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
        return GetConfiguration(options);
    }

    public T GetConfiguration(in SyntaxNodeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        return GetConfiguration(options);
    }

    public T GetConfiguration(AnalyzerConfigOptions options)
    {
        var configuration = GetConfigurationCore(options);
        return configuration.ConfigurationError is null ? configuration : null;
    }

    protected abstract T GetConfigurationCore(AnalyzerConfigOptions options);
}
