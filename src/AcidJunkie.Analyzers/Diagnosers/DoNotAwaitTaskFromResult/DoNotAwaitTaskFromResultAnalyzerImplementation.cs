using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.DoNotAwaitTaskFromResult;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class DoNotAwaitTaskFromResultAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<DoNotAwaitTaskFromResultAnalyzerImplementation>
{
    private readonly IAnalyzerConfiguration _configuration;

    public DoNotAwaitTaskFromResultAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeAwait()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var awaitExpression = (AwaitExpressionSyntax)Context.Node;

        if (Context.SemanticModel.GetSymbolInfo(awaitExpression.Expression).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.ContainingType is null)
        {
            return;
        }

        if (!methodSymbol.ContainingType.Name.EqualsOrdinal("Task")
            || !methodSymbol.Name.EqualsOrdinal("FromResult")
            || !methodSymbol.ContainingType.ContainingNamespace.ToString().EqualsOrdinal("System.Threading.Tasks"))
        {
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, awaitExpression.Expression.GetLocation());
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, awaitExpression.Expression.GetLocation()));
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Performance";
            public const string DiagnosticId = "AJ0008";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Do not await `Task.FromResult()`";
            public static readonly LocalizableString MessageFormat = "Do not await `Task.FromResult()`";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
