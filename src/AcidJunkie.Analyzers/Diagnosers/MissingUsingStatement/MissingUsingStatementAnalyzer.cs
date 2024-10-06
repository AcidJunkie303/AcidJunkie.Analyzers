using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingUsingStatement;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingUsingStatementAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        var config = ConfigurationManager.GetAj0002Configuration(context.Options);
        if (!config.IsEnabled)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var returnedType = context.SemanticModel.GetTypeInfo(invocationExpression).Type as INamedTypeSymbol;
        if (returnedType is null)
        {
            return;
        }

        if (!IsDisposable(returnedType))
        {
            return;
        }

        if (IsTypeIgnored(context, returnedType))
        {
            return;
        }

        if (IsMethodIgnored(context, methodSymbol))
        {
            return;
        }

        if (IsResultStoredInFieldOrProperty(context, invocationExpression))
        {
            return;
        }

        var firstNonMemberAccessOrInvocationExpression = invocationExpression
            .GetParents()
            .FirstOrDefault(a => a is not MemberAccessExpressionSyntax and not InvocationExpressionSyntax and not CastExpressionSyntax);

        if (firstNonMemberAccessOrInvocationExpression is null)
        {
            return;
        }

        if (IsEmbeddedInUsingStatement(firstNonMemberAccessOrInvocationExpression))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, invocationExpression.GetLocation()));
    }

    private static bool IsDisposable(ITypeSymbol typeSymbol)
    {
        return IsDirectDisposable(typeSymbol) || ImplementsDisposable(typeSymbol) || IsRefStructWithDisposeMethod(typeSymbol);

        static bool IsDirectDisposable(ITypeSymbol typeSymbol) => typeSymbol.IsContainedInNamespace("System") && typeSymbol.Name.EqualsOrdinal(nameof(IDisposable));

        static bool ImplementsDisposable(ITypeSymbol typeSymbol) => typeSymbol.AllInterfaces.Any(IsDirectDisposable);

        static bool IsRefStructWithDisposeMethod(ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsRefLikeType)
            {
                return false;
            }

            return typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(member => !member.IsStatic && member is { ReturnsVoid: true, Parameters.Length: 0, TypeParameters.Length: 0 })
                .Any(member => member.Name.EqualsOrdinal("Dispose"));
        }
    }

    private static bool IsEmbeddedInUsingStatement(SyntaxNode node)
    {
        switch (node)
        {
            case UsingStatementSyntax:
                return true;

            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDeclaration } }:
                switch (variableDeclaration.Parent)
                {
                    case UsingStatementSyntax:
                    case LocalDeclarationStatementSyntax localDeclarationStatement when localDeclarationStatement.UsingKeyword.Text.EqualsOrdinal("using"):
                        return true;
                }

                break;
        }

        return false;
    }

    private static bool IsTypeIgnored(SyntaxNodeAnalysisContext context, ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        var simplifiedName = namedTypeSymbol.GetSimplifiedName();
        var ignoredObjects = ConfigurationManager.GetAj0002Configuration(context.Options).IgnoredObjects;

        return ignoredObjects.Contains(simplifiedName);
    }

    private static bool IsMethodIgnored(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var simplifiedName = methodSymbol.GetSimplifiedName();
        var ignoredObjects = ConfigurationManager.GetAj0002Configuration(context.Options).IgnoredObjects;

        return ignoredObjects.Contains(simplifiedName);
    }

    private static bool IsResultStoredInFieldOrProperty(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
    {
        var assignmentTarget = invocationExpression
            .GetParents()
            .OfType<AssignmentExpressionSyntax>()
            .FirstOrDefault()
            ?.Left as IdentifierNameSyntax;

        if (assignmentTarget is null)
        {
            return false;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(assignmentTarget).Symbol;
        return symbol is IFieldSymbol or IPropertySymbol;
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            private const string Category = "Reliability";
            public const string DiagnosticId = "AJ0002";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0002.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Missing using statement";
            public static readonly LocalizableString MessageFormat = "The disposable object is disposed via the using statement";
            public static readonly LocalizableString Description = MessageFormat + ".";
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
        }
    }
}
