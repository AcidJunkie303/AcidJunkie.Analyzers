using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

#if DEBUG
#pragma warning disable RS1026 // Enable concurrent execution -> for easier debugging, we disable concurrent executions
#endif

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingEqualityComparerAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.MissingEqualityComparer.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
#if !DEBUG
        context.EnableConcurrentExecution();
#endif
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);
    }

    private void AnalyzeImplicitObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var implicitObjectCreation = (ImplicitObjectCreationExpressionSyntax)context.Node;

        var namedTypeSymbol = context.SemanticModel.GetTypeInfo(implicitObjectCreation).Type as INamedTypeSymbol;
        if (namedTypeSymbol is null)
        {
            return;
        }

        AnalyzeObjectCreationCore(context, implicitObjectCreation.ArgumentList, implicitObjectCreation.NewKeyword.GetLocation(), namedTypeSymbol);

    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var namedTypeSymbol = context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol as INamedTypeSymbol;
        if (namedTypeSymbol is null)
        {
            return;
        }

        var nameNode = objectCreation.ChildNodes().OfType<NameSyntax>().FirstOrDefault();
        var location = nameNode is null
            ? objectCreation.NewKeyword.GetLocation()
            : context.Node.SyntaxTree.CreateLocationSpan(objectCreation.NewKeyword.GetLocation(), nameNode.GetLocation());

        AnalyzeObjectCreationCore(context, objectCreation.ArgumentList, location, namedTypeSymbol);

        /*
        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            return;
        }

        var keyParameterIndex = namedTypeSymbol.TypeParameters.IndexOf(a => a.Name.EqualsOrdinal(keyTypeParameterName));
        if (keyParameterIndex < 0)
        {
            return;
        }

        if (namedTypeSymbol.TypeArguments.Length != namedTypeSymbol.TypeParameters.Length)
        {
            return;
        }

        var keyType = namedTypeSymbol.TypeArguments[keyParameterIndex];
        if (keyType.IsValueType)
        {
            return; // Value types use structural comparison
        }

        if (keyType.ImplementsGenericEquatable() && keyType.IsGetHashCodeOverridden())
        {
            return;
        }

        if (IsAnyParameterEqualityComparer(context, keyType, objectCreation.ArgumentList))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.MissingEqualityComparer.Rule, objectCreation.GetLocation()));

        string? GetKeyTypeParameterName()
        {
            var ns = namedTypeSymbol.ContainingNamespace?.ToString() ?? string.Empty;

            return GenericKeyParameterNameProvider.GetKeyParameterNameForCreation(ns, namedTypeSymbol.Name, namedTypeSymbol.TypeParameters.Length);
        }
        */
    }

    private static void AnalyzeObjectCreationCore(SyntaxNodeAnalysisContext context, ArgumentListSyntax? argumentList, Location locationToReport, INamedTypeSymbol objectTypeBeingCreated)
    {
        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            return;
        }

        var keyParameterIndex = objectTypeBeingCreated.TypeParameters.IndexOf(a => a.Name.EqualsOrdinal(keyTypeParameterName));
        if (keyParameterIndex < 0)
        {
            return;
        }

        if (objectTypeBeingCreated.TypeArguments.Length != objectTypeBeingCreated.TypeParameters.Length)
        {
            return;
        }

        var keyType = objectTypeBeingCreated.TypeArguments[keyParameterIndex];
        if (keyType.IsValueType)
        {
            return; // Value types use structural comparison
        }

        if (keyType.ImplementsGenericEquatable() && keyType.IsGetHashCodeOverridden())
        {
            return;
        }

        if (IsAnyParameterEqualityComparer(context, keyType, argumentList))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.MissingEqualityComparer.Rule, locationToReport));

        string? GetKeyTypeParameterName()
        {
            var ns = objectTypeBeingCreated.ContainingNamespace?.ToString() ?? string.Empty;

            return GenericKeyParameterNameProvider.GetKeyParameterNameForCreation(ns, objectTypeBeingCreated.Name, objectTypeBeingCreated.TypeParameters.Length);
        }

#pragma warning disable S125
        /*
        var (owningTypeNameSpace, owningTypeName, methodName, memberAccess) = invocationExpression.GetInvokedMethod(context.SemanticModel);
        if (memberAccess is null)
        {
            return;
        }

        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            return;
        }

        var keyType = invocationExpression.GetTypeForTypeParameter(context.SemanticModel, keyTypeParameterName);
        if (keyType is null)
        {
            return;
        }

        if (keyType.IsValueType)
        {
            return; // Value types use structural comparison
        }
        */

    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        var (owningTypeNameSpace, owningTypeName, methodName, memberAccess) = invocationExpression.GetInvokedMethod(context.SemanticModel);
        if (memberAccess is null)
        {
            return;
        }

        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            return;
        }

        var keyType = invocationExpression.GetTypeForTypeParameter(context.SemanticModel, keyTypeParameterName);
        if (keyType is null)
        {
            return;
        }

        if (keyType.IsValueType)
        {
            return; // Value types use structural comparison
        }

        if (keyType.ImplementsGenericEquatable() && keyType.IsGetHashCodeOverridden())
        {
            return;
        }

        if (IsAnyParameterEqualityComparer(context, keyType, invocationExpression.ArgumentList))
        {
            return;
        }

        string? GetKeyTypeParameterName()
        {
            return owningTypeNameSpace is not null && owningTypeName is not null && methodName is not null
                ? GenericKeyParameterNameProvider.GetKeyParameterNameForInvocation(owningTypeNameSpace, owningTypeName, methodName)
                : null;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.MissingEqualityComparer.Rule, memberAccess.Name.GetLocation()));

        /*

        (INamedTypeSymbol?)typeInfo.Type)

    }

    var fullTypeName = node.GetFullTypeName(context, node.Type);
    if (!fullTypeName.EqualsOrdinal("Microsoft.Data.SqlClient.SqlParameter"))
    {
        return;
    }

    if (node.ArgumentList is null || node.ArgumentList.Arguments.Count == 0)
    {
        return;
    }

    var firstArgument = node.ArgumentList.Arguments[0];
    var firstArgumentChild = firstArgument.ChildNodes().First();

    if (firstArgumentChild is not LiteralExpressionSyntax literalExpression)
    {
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.MissingEqualityComparer.Rule, firstArgumentChild.GetLocation()));
        return;
    }

    var argumentData = literalExpression.ToFullString();
    if (!argumentData.StartsWith("\"@", StringComparison.Ordinal))
    {
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.MissingEqualityComparer.Rule, firstArgumentChild.GetLocation()));
        return;
    }

    if (argumentData.Length <= 3 || !char.IsLetter(argumentData[2]) || !char.IsUpper(argumentData[2]))
    {
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.MissingEqualityComparer.Rule, firstArgumentChild.GetLocation()));
    }
    */
    }

    private static bool IsAnyParameterEqualityComparer(SyntaxNodeAnalysisContext context, ITypeSymbol keyType, ArgumentListSyntax? argumentList)
    {
        if (argumentList is null)
        {
            return false;
        }

        foreach (var argument in argumentList.Arguments)
        {
            var argumentType = argument.GetArgumentType(context.SemanticModel);
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
        internal static class MissingEqualityComparer
        {
            public const string Category = "Design";
            public const string DiagnosticId = "AJ0001";

            public static readonly LocalizableString Title = "Provide an IEqualityComparer argument";
            public static readonly LocalizableString MessageFormat = "To prevent unexpected results, use a IEqualityComparer argument because the type used for hash-matching does not fully implement IEquatable<T> together with GetHashCode()";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        }
    }
}
