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
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {

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

        if (IsAnyParameterEqualityComparer())
        {
            return;
        }

        string? GetKeyTypeParameterName()
        {
            return owningTypeNameSpace is not null && owningTypeName is not null && methodName is not null
                ? GenericKeyParameterNameProvider.GetKeyParameterName(owningTypeNameSpace, owningTypeName, methodName)
                : null;
        }

        bool IsAnyParameterEqualityComparer()
        {
            foreach (var argument in invocationExpression.ArgumentList.Arguments)
            {
                var argumentType = argument.GetArgumentType(context.SemanticModel);
                if (argumentType is null)
                {
                    return false;
                }

                if (argumentType.ImplementsOrIsInterface("System.Collections.Generic", "IEqualityComparer", keyType))
                {
                    return true;
                }
            }
            return false;
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
