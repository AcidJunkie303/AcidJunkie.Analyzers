using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ParameterOrdering;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterOrderingAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCompilationAction(CompilationAction);
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndCheck<ParameterOrderingAnalyzer>(AnalyzeParameterList, SyntaxKind.ParameterList);
    }

    private static void CompilationAction(CompilationAnalysisContext context)
    {
        var config = ConfigurationManager.GetAj0007Configuration(context.Options);
        if (config.Validate(context..re))
        {
            logger.AnalyzerIsDisabled();
        }
    }

    private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context, ILogger<ParameterOrderingAnalyzer> logger)
    {
        var config = ConfigurationManager.GetAj0007Configuration(context.Options);
        if (!config.IsEnabled)
        {
            logger.AnalyzerIsDisabled();
        }

        var parameterList = (ParameterListSyntax)context.Node;
        if (parameterList.Parameters.Count == 0)
        {
            logger.WriteLine(() => "No parameters");
            return;
        }

        if (parameterList.Parent is not (MethodDeclarationSyntax or ClassDeclarationSyntax or ConstructorDeclarationSyntax))
        {
            logger.WriteLine(() => "Node parent is not a method declaration, class declaration or constructor declaration node");
            return;
        }

        var parameters = GetParameters(context, parameterList);
    }

    private static void ReportParameter(SyntaxNodeAnalysisContext context, ParameterSyntax parameter, string insertionString, ILogger<ParameterOrderingAnalyzer> logger)
    {
        var location = parameter.Type?.GetLocation() ?? parameter.GetLocation();
        logger.ReportDiagnostic(DiagnosticRules.Default.Rule, location);
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, location, insertionString));
    }

    private static List<Parameter> GetParameters(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList) =>
        parameterList.Parameters
            .Select(param =>
            {
                if (param.Type is null)
                {
                    return new Parameter(param, null);
                }

                if (context.SemanticModel.GetTypeInfo(param.Type).Type is not INamedTypeSymbol parameterType)
                {
                    return new Parameter(param, null);
                }

                if (parameterType.Name.EqualsOrdinal(nameof(CancellationToken)) && parameterType.GetFullNamespace().EqualsOrdinal("System.Threading"))
                {
                    return new Parameter(param, ParameterKind.CancellationToken);
                }

                if (parameterType.Name.EqualsOrdinal("ILogger") && parameterType.GetFullNamespace().EqualsOrdinal("Microsoft.Extensions.Logging"))
                {
                    return new Parameter(param, ParameterKind.Logger);
                }

                return new Parameter(param, ParameterKind.Other);
            })
            .ToList();

    private enum ParameterKind
    {
        Other,
        Logger,
        CancellationToken
    }

    private sealed record Parameter
    {
        public ParameterSyntax Node { get; }
        public string? FullTypeName { get; }

        public Parameter(ParameterSyntax node, string? fullTypeName)
        {
            Node = node;
            FullTypeName = fullTypeName;
        }
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            private const string Category = "Standardization";
            public const string DiagnosticId = "AJ0007";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0007.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "The ILogger parameter placement";
            public static readonly LocalizableString MessageFormat = "The ILogger parameter should be the {0} parameter";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
