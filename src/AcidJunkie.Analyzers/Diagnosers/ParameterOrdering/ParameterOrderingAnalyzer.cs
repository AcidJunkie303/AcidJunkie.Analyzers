using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Configuration.Aj0007;
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
        if (config.ConfigurationError is not null)
        {
            context.ReportValidationError(config.ConfigurationError);
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

        var fallbackIndex = config.ParameterDescriptions.IndexOf(a => a.IsOther);
        if (fallbackIndex < 0)
        {
            // TODO: Should not happen when we enforce the configuration to contain '{other}'
            return;
        }

        var previousIndex = -1;
        var parameters = GetParameters(context, parameterList);

        foreach (var parameter in parameters)
        {
            var index = GetOrderIndex(parameter, config, fallbackIndex);
            if (index < previousIndex)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, parameterList.GetLocation(), config.ParameterOrderFlat));
                return;
            }

            previousIndex = index;
        }
    }

    private static int GetOrderIndex(Parameter parameter, Aj0007Configuration configuration, int fallbackIndex)
    {
        if (parameter.FullTypeName is null)
        {
            return fallbackIndex;
        }

#pragma warning disable S3267 // optimize LINQ usage -> cde would look ugly
        foreach (var parameterDescription in configuration.ParameterDescriptions)
#pragma warning restore S3267
        {
            if (parameterDescription.Matcher(parameter.FullTypeName))
            {
                return parameterDescription.Index;
            }
        }

        return fallbackIndex;
    }

    private static List<Parameter> GetParameters(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList) =>
        parameterList.Parameters
            .Select(param =>
            {
                if (param.Type is null)
                {
                    return new Parameter(param, null);
                }

                return context.SemanticModel.GetTypeInfo(param.Type).Type is not INamedTypeSymbol parameterType
                    ? new Parameter(param, null)
                    : new Parameter(param, parameterType.GetSimplifiedName());
            })
            .ToList();

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

            public static readonly LocalizableString Title = "Invalid parameter order";
            public static readonly LocalizableString MessageFormat = "Parameter order should be {0}";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
