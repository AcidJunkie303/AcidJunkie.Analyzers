using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Diagnosers.ObjectNotDisposed;

internal sealed class OptimisticWalker : CSharpSyntaxWalker, IWalker
{
    private readonly SemanticModel _semanticModel;
    private readonly SyntaxNode _assignmentNode;
    private readonly SyntaxNode _scopeNode;
    private readonly Stack<Scope> _branches = new();
    private readonly string _variableName;
    private bool _hasPassedAssignmentNode;
    private bool _foundBranchWhereObjectIsNotDisposedOrReturned;

    public OptimisticWalker(SemanticModel semanticModel, SyntaxNode scopeNode, string variableName, SyntaxNode assignmentNode)
    {
        _semanticModel = semanticModel;
        _scopeNode = scopeNode;
        _variableName = variableName;
        _assignmentNode = assignmentNode;
    }

    public bool Evaluate()
    {
        BeginScope();

        this.Visit(_scopeNode);

        return EndScope() && !_foundBranchWhereObjectIsNotDisposedOrReturned;
    }

    public override void Visit(SyntaxNode? node)
    {
        if (IsDisposedInAnyPath)
        {
            // no need to pursue this any further 
            return;
        }

        if (_foundBranchWhereObjectIsNotDisposedOrReturned)
        {
            return;
        }

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
        // if the condition is always true, we treat the loops as executed at least once
        // otherwise we take the defensive approach: It never loops
        if (IsSimpleAlwaysTrueCondition(node.Condition))
        {
            base.VisitWhileStatement(node);
        }
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        if (_hasPassedAssignmentNode)
        {
            Visit(node.Condition);

            BeginScope();
            Visit(node.Statement);
            var wasDisposedInStatement = EndScope();

            var wasDisposedInElse = false;
            if (node.Else is not null)
            {
                BeginScope();
                Visit(node.Else);
                wasDisposedInElse = EndScope();
            }

            GetCurrentScope().IsDisposed |= wasDisposedInStatement && wasDisposedInElse;
            return;
        }

        BeginScope();

        base.Visit(node.Statement);

        if (!EndScope())
        {
            _foundBranchWhereObjectIsNotDisposedOrReturned = true;
            return;
        }

        if (node.Else is not null)
        {
            BeginScope();

            this.Visit(node.Else);


            if (!EndScope())
            {
                _foundBranchWhereObjectIsNotDisposedOrReturned = true;
                return;
            }

            EndScope();
        }
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (_hasPassedAssignmentNode && IsDisposeCallOnOurVariable())
        {
            GetCurrentScope().IsDisposed = true;
            return;
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
        if (IsReturningOurVariable())
        {
            GetCurrentScope().IsDisposed = true;
        }

        base.VisitReturnStatement(node);


        bool IsReturningOurVariable()
        {
            if (node.Expression is null)
            {
                return false;
            }

            var symbolInfo = _semanticModel.GetSymbolInfo(node.Expression).Symbol;
            if (symbolInfo is null)
            {
                return false;
            }

            return symbolInfo.Name.EqualsOrdinal(_variableName);
        }
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        var hasDefaultLabel = node.Sections.Any(a => a.IsDefault());
        if (!hasDefaultLabel)
        {
            _foundBranchWhereObjectIsNotDisposedOrReturned = true;
            return;
        }

        var allArmsDisposeOrReturnVariable = true;
        foreach (var section in node.Sections)
        {
            BeginScope();

            base.VisitSwitchSection(section);

            if (!EndScope())
            {
                allArmsDisposeOrReturnVariable = false;
            }
        }

        if (allArmsDisposeOrReturnVariable)
        {
            GetCurrentScope().IsDisposed = true;
        }
    }

    public override void VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        foreach (var arm in node.Arms)
        {
            BeginScope();

            base.VisitSwitchExpressionArm(arm);

            if (!EndScope())
            {
                _foundBranchWhereObjectIsNotDisposedOrReturned = true;
                return;
            }
        }
    }

    public override void VisitTryStatement(TryStatementSyntax node)
    {
        // TODO:
        //continue here: for the properties Block, Catch and Finally, Create a scope and check

        var disposedInTry = VisitAndCheckIfDisposedOnAllBranches(node.Block);
        if (disposedInTry)
        {
            GetCurrentScope().IsDisposed = true;
            return;
        }

        if (node.Finally is not null && VisitAndCheckIfDisposedOnAllBranches(node.Finally))
        {
            GetCurrentScope().IsDisposed = true;
            return;
        }

        // TODO: To clarify: Do really need to check all catch arms
    }

    private bool VisitAndCheckIfDisposedOnAllBranches(SyntaxNode node)
    {
        BeginScope();
        this.Visit(node);
        return EndScope();
    }

    private static bool IsSimpleAlwaysTrueCondition(ExpressionSyntax node)
    {
        switch (node)
        {
            case LiteralExpressionSyntax literal:
                return literal.IsKind(SyntaxKind.TrueLiteralExpression);

            case BinaryExpressionSyntax binaryExpression:
                var left = binaryExpression.Left.ToString();
                var right = binaryExpression.Right.ToString();
                return left.EqualsOrdinal(right);

            default:
                return false;
        }
    }

    private void BeginScope()
    {
        _branches.Push(new());
    }

    private bool EndScope()
    {
        var isDisposedInAnyPath = IsDisposedInAnyPath;
        _branches.Pop();

        return isDisposedInAnyPath;
    }

    private bool IsDisposedInAnyPath => _branches.Any(a => a.IsDisposed);
    private Scope GetCurrentScope() => _branches.Peek();

    private sealed class Scope
    {
        public bool IsDisposed { get; set; }
    }
}
