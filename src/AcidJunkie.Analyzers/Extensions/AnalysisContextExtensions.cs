using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers;
using AcidJunkie.Analyzers.Logging;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class AnalysisContextExtensions
{
    [SuppressMessage("Roslynator", "RCS1175:Unused \'this\' parameter")]
    public static void EnableConcurrentExecutionInReleaseMode(this AnalysisContext context)
    {
#pragma warning disable RS0030 // usage of banned symbol -> This is the only allowed place
#if !DEBUG
        context.EnableConcurrentExecution();
#endif
#pragma warning restore RS0030
    }

    public static void RegisterSyntaxNodeActionAndCheck<TAnalyzer>(this AnalysisContext context, Action<SyntaxNodeAnalysisContext, ILogger<TAnalyzer>?> action, params SyntaxKind[] syntaxKinds)
        where TAnalyzer : DiagnosticAnalyzer
    {
#pragma warning disable RS0030 // this is banned but this is the extension method to replace it
        context.RegisterSyntaxNodeAction(InvokeActionChecked, syntaxKinds);
#pragma warning restore RS0030

        void InvokeActionChecked(SyntaxNodeAnalysisContext ctx)
        {
            var logger = ctx.CreateLogger<TAnalyzer>();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                action(ctx, logger);
                var durationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                ctx
                   .CreateLogger<TAnalyzer>()
                   .WriteLine(LogLevel.Duration, $"Completed analysis. Duration {durationMs}ms");
            }
#pragma warning disable CA1031 // we need to catch everything
            catch (Exception ex)
#pragma warning restore CA1031
            {
                var durationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                logger.WriteLine(LogLevel.Full, $"Unhandled exception occurred after {durationMs}ms. {ex}");
                ctx.ReportDiagnostic(Diagnostic.Create(CommonRules.UnhandledError.Rule, location: null));
            }
        }
    }

    public static void RegisterSyntaxNodeActionAndAnalyze<TAnalyzer>(this AnalysisContext context, Func<TAnalyzer, Action> getAnalyzeMethod, params SyntaxKind[] syntaxKinds)
        where TAnalyzer : class
    {
#pragma warning disable RS0030 // this is banned but this is the extension method to replace it
        context.RegisterSyntaxNodeAction(InvokeActionChecked, syntaxKinds);
#pragma warning restore RS0030

        void InvokeActionChecked(SyntaxNodeAnalysisContext ctx)
        {
            var analyzer = AnalyzerFactory<TAnalyzer>.Create(ctx);
            var analyze = getAnalyzeMethod(analyzer);

            var logger = ctx.CreateLogger<TAnalyzer>();
            logger.WriteLine(LogLevel.Full, $"Start analyzing {ctx.Node}");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                analyze();
                var durationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                ctx
                   .CreateLogger<TAnalyzer>()
                   .WriteLine(LogLevel.Duration, $"Completed analysis. Duration {durationMs}ms");
            }
#pragma warning disable CA1031 // we need to catch everything
            catch (Exception ex)
#pragma warning restore CA1031
            {
                var durationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                logger.WriteLine(LogLevel.Full, $"Unhandled exception occurred after {durationMs}ms. {ex}");
                ctx.ReportDiagnostic(Diagnostic.Create(CommonRules.UnhandledError.Rule, location: null));
            }
        }
    }
}
