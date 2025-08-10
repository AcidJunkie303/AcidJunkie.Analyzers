using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.Logging;

internal sealed class WrongLoggerTypeArgumentAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<WrongLoggerTypeArgumentAnalyzerImplementation>
{
    public WrongLoggerTypeArgumentAnalyzerImplementation(SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeProperty()
    {
        var property = (PropertyDeclarationSyntax)Context.Node;

        if (ModelExtensions.GetDeclaredSymbol(Context.SemanticModel, property) is not IPropertySymbol propertySymbol)
        {
            return;
        }

        if (propertySymbol.Type is not INamedTypeSymbol propertyType)
        {
            return;
        }

        var containingType = property.GetContainingType(Context);
        if (containingType is null)
        {
            return;
        }

        Analyze(propertyType, containingType, property.Type.GetLocation());
    }

    public void AnalyzeField()
    {
        var field = (FieldDeclarationSyntax)Context.Node;

        if (ModelExtensions.GetTypeInfo(Context.SemanticModel, field.Declaration.Type).Type is not INamedTypeSymbol fieldType)
        {
            return;
        }

        var containingType = field.GetContainingType(Context.SemanticModel);
        if (containingType is null)
        {
            return;
        }

        Analyze(fieldType, containingType, field.Declaration.Type.GetLocation());
    }

    public void AnalyzeParameterList()
    {
        var parameterList = (ParameterListSyntax)Context.Node;

        if (parameterList.Parameters.Count == 0)
        {
            return;
        }

        var typeDeclaration = GetTypeDeclaration(parameterList);
        if (typeDeclaration is null)
        {
            return;
        }

        if (ModelExtensions.GetDeclaredSymbol(Context.SemanticModel, typeDeclaration) is not INamedTypeSymbol containerType)
        {
            return;
        }

        var loggerParameters = GetTypedLoggerParameters(parameterList);
        foreach (var loggerParameter in loggerParameters)
        {
            HandleParameter(containerType, loggerParameter);
        }
    }

    private static TypeDeclarationSyntax? GetTypeDeclaration(ParameterListSyntax parameterList)
        => parameterList
          .GetParents()
          .OfType<TypeDeclarationSyntax>()
          .FirstOrDefault(static a => a is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax);

    private void Analyze(INamedTypeSymbol nodeType, INamedTypeSymbol containerType, Location location)
    {
        if (nodeType.Arity != 1)
        {
            Logger.WriteLine(() => $"Arity for {nodeType.Name} in {containerType.Name} is {nodeType.Arity}");
            return;
        }

        var typeParameterType = nodeType.TypeArguments.FirstOrDefault();
        if (typeParameterType is null)
        {
            Logger.WriteLine(() => $"Unable to get type argument for {nodeType.Name}");
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(containerType, typeParameterType))
        {
            Logger.WriteLine(() => $"Logger type argument is the same as the enclosing type {typeParameterType.Name}");
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, location);
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, location));
    }

    private void HandleParameter(INamedTypeSymbol containerType, ParameterSyntax loggerParameter)
    {
        if (loggerParameter.Type is null)
        {
            return;
        }

        if (Context.SemanticModel.GetTypeInfo(loggerParameter.Type).Type is not INamedTypeSymbol argumentType)
        {
            return;
        }

        Analyze(argumentType, containerType, loggerParameter.Type.GetLocation());
    }

    private IEnumerable<ParameterSyntax> GetTypedLoggerParameters(ParameterListSyntax parameterList)
        => parameterList.Parameters
                        .Where(a => a.Type is not null)
                        .Where(a =>
                         {
                             if (Context.SemanticModel.GetTypeInfo(a.Type!, Context.CancellationToken).Type is not INamedTypeSymbol typeSymbol)
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
        internal static ImmutableArray<DiagnosticDescriptor> AllRules { get; } = [Default.Rule];

        internal static class Default
        {
            private const string Category = "Code Smell";
            public const string DiagnosticId = "AJ0006";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0006.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Type argument for ILogger<TContext> should be the enclosing type";
            public static readonly LocalizableString MessageFormat = "When injecting ILogger<TContext> through the constructor, the type argument of ILogger<TContext> should be the same type as the type it is injected into";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
