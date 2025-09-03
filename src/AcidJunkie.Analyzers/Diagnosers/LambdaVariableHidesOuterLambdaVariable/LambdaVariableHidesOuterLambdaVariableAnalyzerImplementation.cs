using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.LambdaVariableHidesOuterLambdaVariable;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class LambdaVariableHidesOuterLambdaVariableAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<LambdaVariableHidesOuterLambdaVariableAnalyzer>
{
    private readonly IAnalyzerConfiguration _configuration;

    public LambdaVariableHidesOuterLambdaVariableAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeClassDeclaration()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var classDeclaration = (ClassDeclarationSyntax)Context.Node;
        foreach (var lambda in TopLevelLambdaFinder.Find(classDeclaration))
        {
            CheckLambda(lambda);
        }
    }

    private void CheckLambda(SyntaxNode lambda)
    {
        foreach (var violation in ViolationChecker.GetViolations(lambda))
        {
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, violation.Location, violation.VariableName));
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
            private const string Category = "Readability";
            public const string DiagnosticId = "AJ0009";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Lambda variable declaration hides outer lambda variable that share the same name";
            public static readonly LocalizableString MessageFormat = "The lambda variable `{0}` hides outer lambda variable that share the same name";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
