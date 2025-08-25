using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;

public static class AssignmentOrIsNullTestedChecker
{
    private static readonly ImmutableHashSet<string> ArgumentNullCheckMethodNames = ImmutableHashSet.Create
    (
        StringComparer.Ordinal,
        "ThrowIfNull",
        "ThrowIfNullOrEmpty",
        "ThrowIfNullOrWhiteSpace"
    );

    public static bool IsMemberAssignedOrNullCheckedOnAllExecutionPaths(SemanticModel semanticModel, MethodDeclarationSyntax method, SyntaxNode declaration)
        => declaration switch
        {
            PropertyDeclarationSyntax d => IsMemberAssignedOrNullCheckedOnAllExecutionPaths(semanticModel, method, d.Identifier.Text),
            VariableDeclaratorSyntax d  => IsMemberAssignedOrNullCheckedOnAllExecutionPaths(semanticModel, method, d.Identifier.Text),
            _                           => throw new ArgumentOutOfRangeException(nameof(declaration), $"Type '{declaration.GetType().FullName}' is not supported!")
        };

    public static bool IsMemberAssignedOrNullCheckedOnAllExecutionPaths(SemanticModel semanticModel, MethodDeclarationSyntax method, string memberName)
        => new Visitor(semanticModel, memberName, method).Check();

    private sealed class Visitor : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly string _memberName;
        private readonly MethodDeclarationSyntax _method;
        private readonly Stack<ScopeData> _isHandledOnBranchLevel = [];
        private readonly INamedTypeSymbol _argumentNullExceptionTypeSymbol;
        private readonly INamedTypeSymbol _argumentExceptionTypeSymbol;

        public Visitor(SemanticModel semanticModel, string memberName, MethodDeclarationSyntax method)
        {
            _semanticModel = semanticModel;
            _memberName = memberName;
            _method = method;

            _argumentExceptionTypeSymbol = GetTypeSymbol(semanticModel.Compilation, "System.ArgumentException");
            _argumentNullExceptionTypeSymbol = GetTypeSymbol(semanticModel.Compilation, "System.ArgumentNullException");
        }

        public bool Check()
        {
            BeginScope();
            Visit(_method);
            return EndScope(false);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node != _method)
            {
                // we do not check local functions
                return;
            }

            BeginScope();
            base.VisitMethodDeclaration(node);
            EndScope();
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            var isHandledInTry = IsHandledInBranch(node.Block);
            var (hasFinally, isHandledInFinally) = node.Finally is null
                ? (false, false)
                : (true, IsHandledInBranch(node.Finally));

            var (hasCatch, isHandledInAllCatchBlocks) = node.Catches.Any()
                ? (true, IsHandledInAllBranches(node.Catches))
                : (false, false);

            if (hasFinally && isHandledInFinally)
            {
                SetHandledInCurrentScope();
                return;
            }

            if (isHandledInTry && hasCatch && isHandledInAllCatchBlocks)
            {
                SetHandledInCurrentScope();
            }
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (node.Else is null)
            {
                return; // if we have no else branch, there's no point of further checking
            }

            BeginScope();

            var isHandledInBothPaths = IsHandledInBranch(node.Statement) && IsHandledInBranch(node.Else.Statement);
            if (isHandledInBothPaths)
            {
                SetHandledInCurrentScope();
            }

            EndScope();
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var isAssignmentToSearchedMember = node.Left is IdentifierNameSyntax identifier && identifier.Identifier.ValueText.EqualsOrdinal(_memberName);
            if (isAssignmentToSearchedMember)
            {
                SetHandledInCurrentScope();
                return;
            }

            base.VisitAssignmentExpression(node);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            if (IsHandledInAllBranches(node.Sections))
            {
                SetHandledInCurrentScope();
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsCheckedForNullThroughArgumentNullExcaptionCall())
            {
                SetHandledInCurrentScope();
            }
            else
            {
                base.VisitInvocationExpression(node);
            }

            bool IsCheckedForNullThroughArgumentNullExcaptionCall()
            {
                if (node.ArgumentList.Arguments.Count != 1)
                {
                    return false;
                }

                if (node.Expression is not MemberAccessExpressionSyntax memberAccess
                    || memberAccess.Expression is not IdentifierNameSyntax identifierName
                    || memberAccess.Name is not IdentifierNameSyntax methodName)
                {
                    return false;
                }

                if (!IsArgumentExceptionOrArgumentNullException(identifierName))
                {
                    return false;
                }

                var isNullCheckMethodName = ArgumentNullCheckMethodNames.Contains(methodName.Identifier.Text);
                if (!isNullCheckMethodName)
                {
                    return false;
                }

                if (node.ArgumentList.Arguments[0].Expression is not IdentifierNameSyntax memberName)
                {
                    return false;
                }

                return memberName.Identifier.Text.EqualsOrdinal(_memberName);
            }
        }

        public override void Visit(SyntaxNode? node)
        {
            if (node is ThrowStatementSyntax or ReturnStatementSyntax or ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or YieldStatementSyntax or BreakStatementSyntax)
            {
                // there's no point of continue the check because it might be a dead end (like return or throw)
                // or, in case of a loop like for, foreach, while they might never be executed.
                return;
            }

            base.Visit(node);
        }

        private static INamedTypeSymbol GetTypeSymbol(Compilation compilation, string fullTypeName)
            => compilation.GetTypeByMetadataName(fullTypeName)
               ?? throw new InvalidOperationException($"Type '{fullTypeName}' not found in compilation.");

        private bool IsArgumentExceptionOrArgumentNullException(IdentifierNameSyntax identifierName)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(identifierName);
            return SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, _argumentExceptionTypeSymbol)
                   || SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, _argumentNullExceptionTypeSymbol);
        }

        private bool IsHandledInBranch(SyntaxNode node)
        {
            BeginScope();
            Visit(node);
            return EndScope(false);
        }

        private bool IsHandledInAllBranches<T>(IReadOnlyList<T> nodes) where T : SyntaxNode
        {
            if (nodes.Count == 0)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                var isHandled = IsHandledInBranch(node);
                if (!isHandled)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetHandledInCurrentScope() => _isHandledOnBranchLevel.Peek().IsAssigned = true;

        private void BeginScope() => _isHandledOnBranchLevel.Push(new ScopeData());

        private bool EndScope(bool propagateHandledStateToParent = true)
        {
            var isAssigned = _isHandledOnBranchLevel.Pop().IsAssigned;
            if (isAssigned && propagateHandledStateToParent)
            {
                _isHandledOnBranchLevel.Peek().IsAssigned = true;
            }

            return isAssigned;
        }
    }

    private sealed class ScopeData
    {
        public bool IsAssigned { get; set; }
    }
}
