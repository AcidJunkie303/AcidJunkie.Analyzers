using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingEqualityComparerAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndCheck<MissingEqualityComparerAnalyzer>(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeActionAndCheck<MissingEqualityComparerAnalyzer>(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeActionAndCheck<MissingEqualityComparerAnalyzer>(AnalyzeImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);
    }

    private static void AnalyzeImplicitObjectCreation(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        var implicitObjectCreation = (ImplicitObjectCreationExpressionSyntax)context.Node;

        var typeSymbol = context.SemanticModel.GetTypeInfo(implicitObjectCreation, context.CancellationToken).Type;
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        AnalyzeObjectCreationCore(context, logger, implicitObjectCreation.ArgumentList, implicitObjectCreation.NewKeyword.GetLocation(), namedTypeSymbol);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var typeSymbol = context.SemanticModel.GetSymbolInfo(objectCreation.Type, context.CancellationToken).Symbol;
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        var nameNode = objectCreation.ChildNodes().OfType<NameSyntax>().FirstOrDefault();
        var location = nameNode is null
            ? objectCreation.NewKeyword.GetLocation()
            : context.Node.SyntaxTree.CreateLocationSpan(objectCreation.NewKeyword.GetLocation(), nameNode.GetLocation());

        AnalyzeObjectCreationCore(context, logger, objectCreation.ArgumentList, location, namedTypeSymbol);
    }

    private static void AnalyzeObjectCreationCore(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger, ArgumentListSyntax? argumentList, Location locationToReport, INamedTypeSymbol objectTypeBeingCreated)
    {
        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            logger.WriteLine(() => $"Unable to determine generic type parameter name for the creation of type '{objectTypeBeingCreated.GetFullName()}'");
            return;
        }

        var keyParameterIndex = objectTypeBeingCreated.TypeParameters.IndexOf(a => a.Name.EqualsOrdinal(keyTypeParameterName));
        if (keyParameterIndex < 0)
        {
            logger.WriteLine(() => $"Unable to determine the index of the key type parameter for type {objectTypeBeingCreated.GetFullName()}");
            return;
        }

        if (objectTypeBeingCreated.TypeArguments.Length != objectTypeBeingCreated.TypeParameters.Length)
        {
            logger.WriteLine(() => $"Found mismatch between type argument count and type parameter count for {objectTypeBeingCreated.GetFullName()} which should not be the case!");
            return;
        }

        var keyType = objectTypeBeingCreated.TypeArguments[keyParameterIndex];
        if (keyType.IsValueType)
        {
            logger.WriteLine(() => $"Key type for {objectTypeBeingCreated.GetFullName()} is {keyType.GetFullName()} which is a struct.");
            return; // Value types use structural comparison
        }

        if (keyType.ImplementsGenericEquatable() && keyType.IsGetHashCodeOverridden())
        {
            logger.WriteLine(() => $"Key type {keyType.GetFullName()} not not implement IEquatable<{keyType.GetFullName()}> or does not override {nameof(object.GetHashCode)}()");
            return;
        }

        if (IsAnyParameterEqualityComparer(context, keyType, argumentList))
        {
            logger.WriteLine(() => "No parameter is a equality comparer");
            return;
        }

        logger.LogReportDiagnostic(DiagnosticRules.Default.Rule, locationToReport);
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, locationToReport));

        string? GetKeyTypeParameterName()
        {
            var ns = objectTypeBeingCreated.ContainingNamespace?.ToString() ?? string.Empty;

            return GenericKeyParameterNameProvider.GetKeyParameterNameForCreation(ns, objectTypeBeingCreated.Name, objectTypeBeingCreated.TypeParameters.Length);
        }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ILogger<MissingEqualityComparerAnalyzer> logger)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        var (owningTypeNameSpace, owningTypeName, methodName, memberAccess) = invocationExpression.GetInvokedMethod(context.SemanticModel, context.CancellationToken);
        if (memberAccess is null)
        {
            return;
        }

        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            logger.WriteLine(() => $"Unable to determine generic type parameter name for invocation of '{owningTypeNameSpace}.{owningTypeName}.{methodName}'");
            return;
        }

        var keyType = invocationExpression.GetTypeForTypeParameter(context.SemanticModel, keyTypeParameterName, context.CancellationToken);
        if (keyType is null)
        {
            logger.WriteLine(() => $"Unable to determine key type parameter type");
            return;
        }

        if (keyType.IsValueType)
        {
            logger.WriteLine(() => $"Key type for '{owningTypeNameSpace}.{owningTypeName}.{methodName}' is {keyType.GetFullName()} which is a struct.");
            return; // Value types use structural comparison
        }

        if (keyType.ImplementsGenericEquatable() && keyType.IsGetHashCodeOverridden())
        {
            logger.WriteLine(() => $"Key type {keyType.GetFullName()} not not implement IEquatable<{keyType.GetFullName()}> or does not override {nameof(object.GetHashCode)}()");
            return;
        }

        if (IsAnyParameterEqualityComparer(context, keyType, invocationExpression.ArgumentList))
        {
            logger.WriteLine(() => "No parameter is a equality comparer");
            return;
        }

        string? GetKeyTypeParameterName()
        {
            return owningTypeNameSpace is not null && owningTypeName is not null && methodName is not null
                ? GenericKeyParameterNameProvider.GetKeyParameterNameForInvocation(owningTypeNameSpace, owningTypeName, methodName)
                : null;
        }

        logger.LogReportDiagnostic(DiagnosticRules.Default.Rule, memberAccess.Name.GetLocation());
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, memberAccess.Name.GetLocation()));
    }

    private static bool IsAnyParameterEqualityComparer(SyntaxNodeAnalysisContext context, ITypeSymbol keyType, ArgumentListSyntax? argumentList)
    {
        if (argumentList is null)
        {
            return false;
        }

        foreach (var argument in argumentList.Arguments)
        {
            var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression, context.CancellationToken).Type;
            if (argumentType is null)
            {
                continue;
            }

            if (argumentType.ImplementsOrIsInterface("System.Collections.Generic", "IEqualityComparer", keyType))
            {
                return true;
            }
        }

        return false;
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            private const string Category = "Predictability";
            public const string DiagnosticId = "AJ0001";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0001.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Provide an equality comparer argument";
            public static readonly LocalizableString MessageFormat = "To prevent unexpected results, use a IEqualityComparer argument because the type used for hash-matching does not fully implement IEquatable<T> together with GetHashCode()";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
        }
    }
}
