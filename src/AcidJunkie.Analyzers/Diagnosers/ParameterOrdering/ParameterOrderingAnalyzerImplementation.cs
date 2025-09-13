using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0007;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ParameterOrdering;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class ParameterOrderingAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<ParameterOrderingAnalyzerImplementation>
{
    private readonly Aj0007Configuration _configuration;

    public ParameterOrderingAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = Aj0007ConfigurationProvider.Instance.GetConfiguration(context);
    }

    public void AnalyzeParameterList()
    {
        if (!_configuration.IsEnabled)
        {
            Logger.AnalyzerIsDisabled();
            return;
        }

        var parameterList = (ParameterListSyntax)Context.Node;
        if (parameterList.Parameters.Count == 0)
        {
            Logger.WriteLine(LogLevel.Full, "No parameters");
            return;
        }

        if (parameterList.Parent is not (MethodDeclarationSyntax or ClassDeclarationSyntax or ConstructorDeclarationSyntax))
        {
            Logger.WriteLine(LogLevel.Full, "Node parent is not a method declaration, class declaration or constructor declaration node");
            return;
        }

        var fallbackIndex = _configuration.ParameterDescriptions.IndexOf(a => a.IsOther);
        if (fallbackIndex < 0)
        {
            // Should not happen when we enforce the configuration to contain '{other}'
            return;
        }

        var previousIndex = -1;
        foreach (var parameter in GetParameters(parameterList))
        {
            if (!parameter.IsParams)
            {
                var index = GetOrderIndex(parameter, _configuration, fallbackIndex);
                if (index < previousIndex)
                {
                    Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, parameterList.GetLocation(), _configuration.ParameterOrderFlat));
                    return;
                }

                previousIndex = index;
            }
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

    private List<Parameter> GetParameters(ParameterListSyntax parameterList) =>
        parameterList.Parameters
                     .Select(param =>
                      {
                          if (param.Type is null)
                          {
                              return new Parameter(param, null, false);
                          }

                          var isParams = param.IsParams();
                          return Context.SemanticModel.GetTypeInfo(param.Type).Type is not INamedTypeSymbol parameterType
                              ? new Parameter(param, null, isParams)
                              : new Parameter(param, parameterType.GetSimplifiedName(), isParams);
                      })
                     .ToList();

    private sealed record Parameter
    {
        public ParameterSyntax Node { get; }
        public string? FullTypeName { get; }
        public bool IsParams { get; }

        public Parameter(ParameterSyntax node, string? fullTypeName, bool isParams)
        {
            Node = node;
            FullTypeName = fullTypeName;
            IsParams = isParams;
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
            private const string Category = "Standardization";
            public const string DiagnosticId = "AJ0007";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Non-compliant parameter order";
            public static readonly LocalizableString MessageFormat = "Parameter order should be {0}";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
