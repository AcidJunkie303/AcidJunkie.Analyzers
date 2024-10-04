using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

public static class AnalysisContextExtensions
{
    public static void EnableConcurrentExecutionInReleaseMode(this AnalysisContext context)
    {
#if !DEBUG
#pragma warning disable RS0030 // usage of banned symbol -> This is the only allowed place
        context.EnableConcurrentExecution();
#pragma warning restore RS0030
#endif
    }
}
