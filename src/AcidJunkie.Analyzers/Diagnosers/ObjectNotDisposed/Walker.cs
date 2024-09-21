using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Diagnosers.ObjectNotDisposed;

// TODO: remove
#pragma warning disable
#pragma warning disable S125


internal sealed class Walker : CSharpSyntaxWalker
{
    // LocalDeclarationStatementSyntax is the type of variable declarations -> we need to get it's parent so we know when the variable goes out of scope.

    private readonly SemanticModel _semanticModel;
    private readonly SyntaxNode _invocationWhichReturnsDisposable;
    private readonly VariableDeclarationSyntax _variableDeclaration;
    private readonly SyntaxNode _assignmentNode;
    private readonly SyntaxNode _scopeNode;
    private readonly SyntaxNode _scopeEndNode;
    private readonly Stack<Branch> _branches = new();
    private bool _pathFoundWhereNotDisposed;
    private bool _hasPassedAssignmentNode;
    private bool _abort;

    private bool _variableIsDisposedOnAllPaths = true;

    public Walker(SemanticModel semanticModel, SyntaxNode scopeNode, VariableDeclarationSyntax variableDeclaration, SyntaxNode assignmentNode)
    {
        _semanticModel = semanticModel;
        _scopeNode = scopeNode;
        _variableDeclaration = variableDeclaration;
        _assignmentNode = assignmentNode;

        //        _scopeNode = scopeNode;
        //        _scopeEndNode = scopeNode.Parent ?? throw new InvalidOperationException("Variable declaration found without parent node!");
    }

    public bool Evaluate()
    {
        BeginScope();

        this.Visit(_scopeNode);

        var isDisposedOnAllPaths = IsDisposedInAnyPath();

        EndScope();

        return isDisposedOnAllPaths;
    }

    public override void Visit(SyntaxNode? node)
    {
        base.Visit(node);

        if (ReferenceEquals(node, _assignmentNode))
        {
            _hasPassedAssignmentNode = true;
        }
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        // defensive approach: We assume that this for loop never loops
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        // defensive approach: We assume that this for loop never loops
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        // defensive approach: We assume that this for loop never loops
        return;

        // TODO: Check if the while condition always evaluates to true like:
        // - while(true)
        // - while(!false)
        // - while(1==1)
    }



    public override void VisitIfStatement(IfStatementSyntax node)
    {
        if (_hasPassedAssignmentNode)
        {
            Visit(node.Condition);

            BeginScope();
            Visit(node.Statement);
            var wasDisposedInStatement = IsDisposedInAnyPath();
            EndScope();

            var wasDisposedInElse = false;
            if (node.Else is not null)
            {
                BeginScope();
                Visit(node.Else);
                wasDisposedInElse = IsDisposedInAnyPath();
                EndScope();
            }

            GetCurrentScope().IsDisposed |= wasDisposedInStatement && wasDisposedInElse;
            return;
        }

        BeginScope();

        base.Visit(node.Statement);

        if (!IsDisposedInAnyPath())
        {
            AbortAndSetNotDisposedOnAllPaths();
            return;
        }

        EndScope();

        if (node.Else is not null)
        {
            BeginScope();

            this.Visit(node.Else);

            if (!IsDisposedInAnyPath())
            {
                AbortAndSetNotDisposedOnAllPaths();
                return;
            }

            EndScope();
        }
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (_hasPassedAssignmentNode)
        {
            if (IsDisposeCallOnOurVariable())
            {
                GetCurrentScope().IsDisposed = true;
                return;
            }
        }

        base.VisitInvocationExpression(node);

        bool IsDisposeCallOnOurVariable()
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node.Expression).Symbol;
            if (symbolInfo is null)
            {
                return false;
            }

            var memberAccess = node.ChildNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
            if (memberAccess?.Expression is null)
            {
                return false;
            }

            if (_semanticModel.GetSymbolInfo(memberAccess).Symbol is not IMethodSymbol methodSymbol)
            {
                return false;
            }

            if (methodSymbol.Arity != 0 || !methodSymbol.ReturnsVoid || !methodSymbol.Name.EqualsOrdinal(nameof(IDisposable.Dispose)))
            {
                return false;
            }

            return true;
        }
    }

    public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        if (!_hasPassedAssignmentNode)
        {
            return;
        }

        if (IsDisposeCallOnOurVariable())
        {
            GetCurrentScope().IsDisposed = true;
            return;
        }

        base.VisitConditionalAccessExpression(node);

        bool IsDisposeCallOnOurVariable()
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node.Expression).Symbol;
            if (symbolInfo is null)
            {
                return false;
            }

            var invocationExpression = node.ChildNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocationExpression?.Expression is null)
            {
                return false;
            }

            if (_semanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            {
                return false;
            }

            if (methodSymbol.Arity != 0 || !methodSymbol.ReturnsVoid || !methodSymbol.Name.EqualsOrdinal(nameof(IDisposable.Dispose)))
            {
                return false;
            }

            return true;
        }
    }

    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        // we don't recurse into local methods
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        base.VisitReturnStatement(node);
    }

    public override void VisitThrowStatement(ThrowStatementSyntax node)
    {
        base.VisitThrowStatement(node);
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {

        base.VisitVariableDeclaration(node);
    }


    private void AbortAndSetNotDisposedOnAllPaths()
    {
        _variableIsDisposedOnAllPaths = false;
        _abort = true;
    }

    private void BeginScope()
    {
        _branches.Push(new());
    }

    private void EndScope()
    {
        _branches.Pop();
    }

    private int ScopeLevel => _branches.Count;
    private Branch GetCurrentScope() => _branches.Peek();

    private bool IsDisposedInAnyPath() => _branches.Any(a => a.IsDisposed);

    private static bool IsExitNode(SyntaxNode node)
        => node is ThrowExpressionSyntax or ReturnStatementSyntax;

    private static bool IsLeaf(SyntaxNode node)
        => !node.ChildNodes().Any();

    private sealed class Branch
    {
        public bool IsDisposed { get; set; }
    }
}
