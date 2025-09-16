#pragma warning disable

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.UseIsForNullComparison;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class UseIsForNullComparisonAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<UseIsForNullComparisonAnalyzer>
{
    private readonly IAnalyzerConfiguration _configuration;

    public UseIsForNullComparisonAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeBinaryExpression()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var equalsExpression = (BinaryExpressionSyntax)Context.Node;
        if (!equalsExpression.IsKind(SyntaxKind.EqualsExpression) && !equalsExpression.IsKind(SyntaxKind.NotEqualsExpression))
        {
            return;
        }

        if (!equalsExpression.Right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return;
        }

        var typeInfo = Context.SemanticModel.GetTypeInfo(equalsExpression.Left).Type;
        if (typeInfo is null || !typeInfo.IsReferenceType)
        {
            return;
        }

        if (IsWithinQueryableWhere(equalsExpression))
        {
            return;
        }

        var isOrIsNot = equalsExpression.IsKind(SyntaxKind.EqualsExpression) ? "is" : "is not";
        var intention = equalsExpression.IsKind(SyntaxKind.EqualsExpression) ? "null" : "not null";

        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, equalsExpression.OperatorToken.GetLocation(), isOrIsNot, intention, equalsExpression.OperatorToken.Text));
    }

    private bool IsWithinQueryableWhere(BinaryExpressionSyntax binaryExpression)
    {
        return binaryExpression
              .Ancestors()
              .OfType<InvocationExpressionSyntax>()
              .Any(IsQueryableWhere);

        bool IsQueryableWhere(InvocationExpressionSyntax invocationExpression)
        {
            if (Context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            {
                return false;
            }

            return methodSymbol.Name.EqualsOrdinal("Where") && methodSymbol.ContainingType.ToDisplayString().EqualsOrdinal("System.Linq.Queryable");
        }
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Intention";
            public const string DiagnosticId = "AJ0011";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Use `is` or `is not` for null-comparison";
            public static readonly LocalizableString MessageFormat = "Use `{0}` to compare for `{1}` instead of `{2}`";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
