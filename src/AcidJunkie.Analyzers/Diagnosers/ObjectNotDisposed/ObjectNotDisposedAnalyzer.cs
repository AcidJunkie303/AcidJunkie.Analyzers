using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ObjectNotDisposed;

#if DEBUG
#pragma warning disable RS1026 // Enable concurrent execution -> for easier debugging, we disable concurrent executions
#endif

// TODO: remove
#pragma warning disable S125

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ObjectNotDisposedAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.ObjectNotDisposedOnAllPaths.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
#if !DEBUG
        context.EnableConcurrentExecution();
#endif
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

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol)
        {
            return;
        }

        var returnedTypeInfo = context.SemanticModel.GetTypeInfo(invocationExpression).Type;
        if (returnedTypeInfo is null)
        {
            return;
        }

        if (!IsDisposable(returnedTypeInfo))
        {
            return;
        }

        // TODO: get the returned type and check whether the type is ignored
        // TODO: get the type the method is contained in and check whether the method is ignored

        var firstNonMemberAccessOrInvocationExpression = invocationExpression
            .GetParents()
            .FirstOrDefault(a => a is not MemberAccessExpressionSyntax and not InvocationExpressionSyntax);

        if (firstNonMemberAccessOrInvocationExpression is null)
        {
            return;
        }

        if (IsEmbeddedInUsingStatement(firstNonMemberAccessOrInvocationExpression))
        {
            return;
        }

        var (assignmentNode, variableDeclaration) = GetAssignmentAndDeclaration(context, firstNonMemberAccessOrInvocationExpression);
        if (assignmentNode is null || variableDeclaration is null)
        {
            return;
        }

        var variableDeclarator = variableDeclaration.ChildNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
        if (variableDeclarator is null)
        {
            return;
        }

        var variableName = variableDeclarator.Identifier.Text;

        var scopeNode = variableDeclaration.GetParents().FirstOrDefault(a => a is not LocalDeclarationStatementSyntax);
        if (scopeNode is null)
        {
            return;
        }

        var walker = new Walker(context.SemanticModel, scopeNode, variableDeclaration, variableName, assignmentNode);
        // if assigned to a variable, then use walker with the parent declaration syntax and its parent and skill all nodes in the walker until we hit the invocation method
        //      after that, check all paths if the variable is disposed or returned (return statement or arrow operator)
        if (!walker.Evaluate())
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.ObjectNotDisposedOnAllPaths.Rule,
                invocationExpression.GetLocation()));
        }

        // if we are here, we need to check if the returned IDisposable object is assigned to a variable or returned through the return statement or by the arrow operator

        // TODO:
        // find parent of type 'LocalDeclarationStatementSyntax' and check
        //     if using statement -> good, no further checking
        //     if variable assignment, use walker with scoping
        // find parent of type 'UsingStatementSyntax' -> find variable assignment. if ok -> we're good

        //SymbolFinder.FindDeclarationsAsync(context)
        //context.SemanticModel.GetDeclaredSymbol
        static (SyntaxNode? AssignmentNode, VariableDeclarationSyntax? VariableDeclaration) GetAssignmentAndDeclaration(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            VariableDeclarationSyntax? variableDeclaration = null;
            SyntaxNode? assignmentNode = null;

            if (node is EqualsValueClauseSyntax equalsValueClause)
            {
                variableDeclaration = equalsValueClause.GetParents().OfType<VariableDeclarationSyntax>().FirstOrDefault();
                assignmentNode = variableDeclaration;
            }
            else if (node is AssignmentExpressionSyntax assignment)
            {
                var declaration = GetDeclarationOfAssignmentTarget(context, assignment);
                if (declaration?.Parent is FieldDeclarationSyntax or PropertyDeclarationSyntax)
                {
                    // for now, we quit checking.
                    // idea for future: if the field is non-static, then check if there's a dispose method in the class where the field or property is disposed.
                    return (null, null);
                }

                assignmentNode = assignment;
                variableDeclaration = declaration as VariableDeclarationSyntax;
            }

            return (assignmentNode, variableDeclaration);
        }
    }

    private static SyntaxNode? GetDeclarationOfAssignmentTarget(SyntaxNodeAnalysisContext context, AssignmentExpressionSyntax assignmentExpression)
    {
        var assignedTo = context.SemanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol;
        if (assignedTo is null)
        {
            return null;
        }

        var declaringReference = assignedTo.DeclaringSyntaxReferences.FirstOrDefault();
        if (declaringReference is null)
        {
            return null;
        }

#pragma warning disable MA0045 // we're not in async context here...
        var declaration = declaringReference.GetSyntax(context.CancellationToken);
#pragma warning restore MA0045
        if (declaration is null)
        {
            return null;
        }

        return declaration
            .GetParents()
            .FirstOrDefault(a => a is DeclarationExpressionSyntax or FieldDeclarationSyntax or PropertyDeclarationSyntax or VariableDeclarationSyntax);
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
        if (node is UsingStatementSyntax)
        {
            return true;
        }

        if (node is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDeclaration } })
        {
            switch (variableDeclaration.Parent)
            {
                case UsingStatementSyntax:
                case LocalDeclarationStatementSyntax localDeclarationStatement when localDeclarationStatement.UsingKeyword.Text.EqualsOrdinal("using"):
                    return true;
            }
        }

        return false;
        /*
        switch (node.Parent)
        {
            case UsingStatementSyntax: // using( new DisposableObject())
            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: UsingStatementSyntax u } } }: // using (var aa = new DisposableObject())
                return true;
            default:
                return false;
        }
        */
    }

    internal static class DiagnosticRules
    {
        internal static class ObjectNotDisposedOnAllPaths
        {
            public const string Category = "Reliability";
            public const string DiagnosticId = "AJ0002";

            public static readonly LocalizableString Title = "Object not disposed on all paths";
            public static readonly LocalizableString MessageFormat = "The disposable object is not disposed on all code paths";
            public static readonly LocalizableString Description = MessageFormat + ".";
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
        }
    }
}
