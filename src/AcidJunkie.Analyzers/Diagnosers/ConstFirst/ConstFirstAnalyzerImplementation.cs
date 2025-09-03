using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Logging;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ConstFirst;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class ConstFirstAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<ConstFirstAnalyzerImplementation>
{
    private readonly IAnalyzerConfiguration _configuration;

    public ConstFirstAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeMethod()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var methodDeclaration = (MethodDeclarationSyntax)Context.Node;

        // we only check normal methods (no lambda style methods)
        if (methodDeclaration.Body is null)
        {
            return;
        }

        CheckStatements(methodDeclaration.Body.Statements);
    }

    private void CheckStatements(in SyntaxList<StatementSyntax> statements)
    {
        var hasPassedNonConst = false;

        foreach (var statement in statements)
        {
            var isConst = statement is LocalDeclarationStatementSyntax { IsConst: true };

            if (hasPassedNonConst)
            {
                if (isConst)
                {
                    Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, statement.GetLocation());
                    Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, statement.GetLocation()));
                }
            }
            else if (!isConst)
            {
                hasPassedNonConst = true;
            }
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
            private const string Category = "Style";
            public const string DiagnosticId = "AJ0010";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Declare constants at the top of the method";
            public static readonly LocalizableString MessageFormat = "Declare constants at the top of the method";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
