using System.Collections.Immutable;
using AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace AcidJunkie.Analyzers.Diagnosers.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WrongLoggerTypeArgumentAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndCheck<MissingEqualityComparerAnalyzer>(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeActionAndCheck<MissingEqualityComparerAnalyzer>(AnalyzeField, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeActionAndCheck<MissingEqualityComparerAnalyzer>(AnalyzeParameterList, SyntaxKind.ParameterList);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
        {
            return;
        }

        if (propertySymbol.Type is not INamedTypeSymbol propertyType)
        {
            return;
        }

        var containingType = property.GetContainingType(context);
        if (containingType is null)
        {
            return;
        }

        Analyze(context, propertyType, containingType, property.Type.GetLocation(), logger);
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        var field = (FieldDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetTypeInfo(field.Declaration.Type).Type is not INamedTypeSymbol fieldType)
        {
            return;
        }

        var containingType = field.GetContainingType(context.SemanticModel);
        if (containingType is null)
        {
            return;
        }

        Analyze(context, fieldType, containingType, field.Declaration.Type.GetLocation(), logger);
    }

    private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        var parameterList = (ParameterListSyntax)context.Node;

        if (parameterList.Parameters.Count == 0)
        {
            return;
        }

        var typeDeclaration = GetTypeDeclaration(parameterList);
        if (typeDeclaration is null)
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol containerType)
        {
            return;
        }

        var loggerParameters = GetTypedLoggerParameters(context, parameterList);
        foreach (var loggerParameter in loggerParameters)
        {
            HandleParameter(context, containerType, loggerParameter, logger);
        }
    }

    private static void Analyze(SyntaxNodeAnalysisContext context, INamedTypeSymbol nodeType, INamedTypeSymbol containerType, Location location, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        if (nodeType.Arity != 1)
        {
            logger.WriteLine(() => $"Arity for {nodeType.Name} in {containerType.Name} is {nodeType.Arity}");
            return;
        }

        var typeParameterType = nodeType.TypeArguments.FirstOrDefault();
        if (typeParameterType is null)
        {
            logger.WriteLine(() => $"Unable to get type argument for {nodeType.Name}");
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(containerType, typeParameterType))
        {
            logger.WriteLine(() => $"Logger type argument is the same as the enclosing type {typeParameterType.Name}");
            return;
        }

        logger.LogReportDiagnostic(DiagnosticRules.Default.Rule, location);
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, location));
    }

    private static void HandleParameter(SyntaxNodeAnalysisContext context, INamedTypeSymbol containerType, ParameterSyntax loggerParameter, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        if (loggerParameter.Type is null)
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(loggerParameter.Type).Type is not INamedTypeSymbol argumentType)
        {
            return;
        }

        Analyze(context, argumentType, containerType, loggerParameter.Type.GetLocation(), logger);
    }

    private static TypeDeclarationSyntax? GetTypeDeclaration(ParameterListSyntax parameterList)
        => parameterList
            .GetParents()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault(static a => a is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax);

    private static IEnumerable<ParameterSyntax> GetTypedLoggerParameters(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList)
        => parameterList.Parameters
            .Where(a => a.Type is not null)
            .Where(a =>
            {
                if (context.SemanticModel.GetTypeInfo(a.Type!, context.CancellationToken).Type is not INamedTypeSymbol typeSymbol)
                {
                    return false;
                }

                if (typeSymbol.Arity != 1)
                {
                    return false;
                }

                return typeSymbol.Name.EqualsOrdinal("ILogger") && typeSymbol.GetFullNamespace().EqualsOrdinal("Microsoft.Extensions.Logging");
            });

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            private const string Category = "Code Smell";
            public const string DiagnosticId = "AJ0006";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0006.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Probably wrong type argument for ILogger<TContext>";
            public static readonly LocalizableString MessageFormat = "When injecting ILogger<TContext> through the constructor, the type argument of ILogger<TContext> should be the same type as the type it is injected into";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
