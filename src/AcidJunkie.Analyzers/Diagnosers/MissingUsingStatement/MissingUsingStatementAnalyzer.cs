using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
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
        context.RegisterSyntaxNodeActionAndCheck<MissingUsingStatementAnalyzer>(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ILogger<MissingUsingStatementAnalyzer> logger)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        var config = ConfigurationManager.GetAj0002Configuration(context.Options);
        if (!config.IsEnabled)
        {
            logger.LogAnalyzerIsDisabled();
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            logger.WriteLine(() => $"Unable to get {nameof(IMethodSymbol)} from invocation");
            return;
        }

        if (context.SemanticModel.GetTypeInfo(invocationExpression, context.CancellationToken).Type is not INamedTypeSymbol returnedType)
        {
            logger.WriteLine(() => $"Unable to get {nameof(INamedTypeSymbol)} from invocation");
            return;
        }

        if (!IsDisposable(returnedType))
        {
            logger.WriteLine(() => $"Method return type is not or does not implement {nameof(IDisposable)}");
            return;
        }

        if (IsTypeIgnored(context, returnedType))
        {
            logger.WriteLine(() => $"Type {returnedType.GetFullName()} is ignored");
            return;
        }

        if (IsMethodIgnored(context, methodSymbol))
        {
            logger.WriteLine(() => $"Method {methodSymbol.GetFullName()} is ignored");
            return;
        }

        if (IsResultStoredInFieldOrProperty(context, invocationExpression))
        {
            logger.WriteLine(() => $"Disposable object is stored in property or field");
            return;
        }

        var firstNonMemberAccessOrInvocationExpression = invocationExpression
            .GetParents()
            .FirstOrDefault(a => a is not MemberAccessExpressionSyntax and not InvocationExpressionSyntax and not CastExpressionSyntax);

        if (firstNonMemberAccessOrInvocationExpression is null)
        {
            logger.WriteLine(() => $"{nameof(firstNonMemberAccessOrInvocationExpression)} is null");
            return;
        }

        if (IsEmbeddedInUsingStatement(firstNonMemberAccessOrInvocationExpression))
        {
            logger.WriteLine(() => "Disposable object is stored in variable with using statement");
            return;
        }

        logger.LogReportDiagnostic(DiagnosticRules.Default.Rule, invocationExpression.GetLocation());
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, invocationExpression.GetLocation()));
    }

    private static bool IsDisposable(ITypeSymbol typeSymbol)
    {
        return IsDirectDisposable(typeSymbol) || ImplementsDisposable(typeSymbol) || IsRefStructWithDisposeMethod(typeSymbol);

        static bool IsDirectDisposable(ITypeSymbol typeSymbol)
        {
            return typeSymbol.IsContainedInNamespace("System") && typeSymbol.Name.EqualsOrdinal(nameof(IDisposable));
        }

        static bool ImplementsDisposable(ITypeSymbol typeSymbol)
        {
            return typeSymbol.AllInterfaces.Any(IsDirectDisposable);
        }

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
        if (invocationExpression
            .GetParents()
            .OfType<AssignmentExpressionSyntax>()
            .FirstOrDefault()
            ?.Left is not IdentifierNameSyntax assignmentTarget)
        {
            return false;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(assignmentTarget, context.CancellationToken).Symbol;
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
