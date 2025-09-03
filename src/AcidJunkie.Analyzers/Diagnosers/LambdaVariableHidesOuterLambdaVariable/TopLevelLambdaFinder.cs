using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Diagnosers.LambdaVariableHidesOuterLambdaVariable;

internal static class TopLevelLambdaFinder
{
    public static IReadOnlyList<SyntaxNode> Find(SyntaxNode node)
    {
        var walker = new Walker();
        walker.Visit(node);
        return walker.Nodes;
    }

    private sealed class Walker : CSharpSyntaxWalker
    {
        private readonly List<SyntaxNode> _nodes = new();

        public IReadOnlyList<SyntaxNode> Nodes => _nodes;

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => _nodes.Add(node);

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => _nodes.Add(node);
    }
}
