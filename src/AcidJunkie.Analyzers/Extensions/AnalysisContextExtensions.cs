using System.Diagnostics;
using AcidJunkie.Analyzers.Diagnosers;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class AnalysisContextExtensions
{
    public static void EnableConcurrentExecutionInReleaseMode(this AnalysisContext context)
    {
#if !DEBUG
#pragma warning disable RS0030 // usage of banned symbol -> This is the only allowed place
        context.EnableConcurrentExecution();
#pragma warning restore RS0030
#endif
    }

    public static void RegisterSyntaxNodeActionAndCheck<TAnalyzer>(this AnalysisContext context, Action<SyntaxNodeAnalysisContext, ILogger<TAnalyzer>> action, params SyntaxKind[] syntaxKinds)
        where TAnalyzer : DiagnosticAnalyzer
    {
#pragma warning disable RS0030 // this is banned but this is the extension method to replace it
        context.RegisterSyntaxNodeAction(InvokeActionChecked, syntaxKinds);
#pragma warning restore RS0030

        void InvokeActionChecked(SyntaxNodeAnalysisContext ctx)
        {
            var logger = ctx.CreateLogger<TAnalyzer>();
            logger.WriteLine(() => $"Start analyzing {ctx.Node}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                action(ctx, logger);
                var durationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                logger.WriteLine(() => $"Completed analysis. Duration {durationMs}ms");
            }
#pragma warning disable CA1031 // we need to catch everything
            catch (Exception ex)
#pragma warning restore CA1031
            {
                var durationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                logger.WriteLine(() => $"Unhandled exception occurred after {durationMs}ms. {ex}");
                ctx.ReportDiagnostic(Diagnostic.Create(CommonRules.UnhandledError.Rule, location: null));
            }
        }
    }
}
