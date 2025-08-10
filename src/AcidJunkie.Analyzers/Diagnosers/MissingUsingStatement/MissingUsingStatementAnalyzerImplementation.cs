using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingUsingStatement;

internal sealed class MissingUsingStatementAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<MissingUsingStatementAnalyzer>
{
    public MissingUsingStatementAnalyzerImplementation(SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeInvocation()
    {
        var config = ConfigurationManager.GetAj0002Configuration(Context.Options);
        if (!config.IsEnabled)
        {
            Logger.AnalyzerIsDisabled();
            return;
        }

        var invocationExpression = (InvocationExpressionSyntax)Context.Node;

        if (Context.SemanticModel.GetSymbolInfo(invocationExpression, Context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            Logger.WriteLine(() => $"Unable to get {nameof(IMethodSymbol)} from invocation");
            return;
        }

        if (Context.SemanticModel.GetTypeInfo(invocationExpression, Context.CancellationToken).Type is not INamedTypeSymbol returnedType)
        {
            Logger.WriteLine(() => $"Unable to get {nameof(INamedTypeSymbol)} from invocation");
            return;
        }

        if (!IsDisposable(returnedType))
        {
            Logger.WriteLine(() => $"Method return type is not or does not implement {nameof(IDisposable)}");
            return;
        }

        if (IsTypeIgnored(returnedType))
        {
            Logger.WriteLine(() => $"Type {returnedType.GetFullName()} is ignored");
            return;
        }

        if (IsMethodIgnored(methodSymbol))
        {
            Logger.WriteLine(() => $"Method {methodSymbol.GetFullName()} is ignored");
            return;
        }

        if (IsResultStoredInFieldOrProperty(invocationExpression))
        {
            Logger.WriteLine(() => "Disposable object is stored in property or field");
            return;
        }

        var firstNonMemberAccessOrInvocationExpression = invocationExpression
                                                        .GetParents()
                                                        .FirstOrDefault(a => a is not MemberAccessExpressionSyntax and not InvocationExpressionSyntax and not CastExpressionSyntax);

        if (firstNonMemberAccessOrInvocationExpression is null)
        {
            Logger.WriteLine(() => $"{nameof(firstNonMemberAccessOrInvocationExpression)} is null");
            return;
        }

        if (IsEmbeddedInUsingStatement(firstNonMemberAccessOrInvocationExpression))
        {
            Logger.WriteLine(() => "Disposable object is stored in variable with using statement");
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, invocationExpression.GetLocation());
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, invocationExpression.GetLocation()));
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
                             .Where(static member => !member.IsStatic && member is { ReturnsVoid: true, Parameters.Length: 0, TypeParameters.Length: 0 })
                             .Any(static member => member.Name.EqualsOrdinal("Dispose"));
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

    private bool IsTypeIgnored(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        var simplifiedName = namedTypeSymbol.GetSimplifiedName();
        var ignoredObjects = ConfigurationManager.GetAj0002Configuration(Context.Options).IgnoredObjects;

        return ignoredObjects.Contains(simplifiedName);
    }

    private bool IsMethodIgnored(IMethodSymbol methodSymbol)
    {
        var simplifiedName = methodSymbol.GetSimplifiedName();
        var ignoredObjects = ConfigurationManager.GetAj0002Configuration(Context.Options).IgnoredObjects;

        return ignoredObjects.Contains(simplifiedName);
    }

    private bool IsResultStoredInFieldOrProperty(InvocationExpressionSyntax invocationExpression)
    {
        if (invocationExpression
           .GetParents()
           .OfType<AssignmentExpressionSyntax>()
           .FirstOrDefault()
          ?.Left is not IdentifierNameSyntax assignmentTarget)
        {
            return false;
        }

        var symbol = Context.SemanticModel.GetSymbolInfo(assignmentTarget, Context.CancellationToken).Symbol;
        return symbol is IFieldSymbol or IPropertySymbol;
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> AllRules { get; } = [Default.Rule];

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
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
