using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers;

internal abstract class SyntaxNodeAnalyzerImplementationBase<TImplementation>
    where TImplementation : class
{
    protected ILogger<TImplementation>? Logger { get; }
    protected SyntaxNodeAnalysisContext Context { get; }

    protected SyntaxNodeAnalyzerImplementationBase(in SyntaxNodeAnalysisContext context)
    {
        Context = context;
        Logger = context.CreateLogger<TImplementation>();
    }
}
