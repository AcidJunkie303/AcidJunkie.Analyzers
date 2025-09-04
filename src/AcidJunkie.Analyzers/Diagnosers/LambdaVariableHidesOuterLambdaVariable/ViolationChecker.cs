using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Diagnosers.LambdaVariableHidesOuterLambdaVariable;

internal static class ViolationChecker
{
    public static IReadOnlyList<Violation> GetViolations(SyntaxNode node)
    {
        var walker = new Walker();
        walker.Visit(node);
        return walker.Violations;
    }

    private sealed class Walker : CSharpSyntaxWalker
    {
        private readonly Stack<ScopeData> _scopes = new();
        private readonly List<Violation> _violations = new();

        public IReadOnlyList<Violation> Violations => _violations;

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            BeginScope();

            if (!IsDiscardedParameter(node.Parameter))
            {
                RegisterVariableAndCheck(node.Parameter);
            }

            base.VisitSimpleLambdaExpression(node);

            EndScope();
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            BeginScope();

            foreach (var parameter in node.ParameterList.Parameters.WhereNot(IsDiscardedParameter))
            {
                RegisterVariableAndCheck(parameter);
            }

            base.VisitParenthesizedLambdaExpression(node);

            EndScope();
        }

        private static bool IsDiscardedParameter(ParameterSyntax parameter)
            => parameter.Identifier.Text.EqualsOrdinal("_");

        private void RegisterVariableAndCheck(ParameterSyntax parameter)
        {
            var variableName = parameter.Identifier.ValueText;

            var isVariableUsedAlready = _scopes.Any(a => a.IsVariableRegistered(variableName));
            if (!isVariableUsedAlready)
            {
                _scopes.Peek().RegisterVariable(variableName);
                return;
            }

            var violation = new Violation(parameter.Identifier.ValueText, parameter.GetLocation());
            _violations.Add(violation);
        }

        private void BeginScope() => _scopes.Push(new ScopeData());

        private void EndScope() => _scopes.Pop();
    }

    internal sealed class Violation
    {
        public string VariableName { get; }
        public Location Location { get; }

        public Violation(string variableName, Location location)
        {
            VariableName = variableName;
            Location = location;
        }
    }

    private sealed class ScopeData
    {
        private readonly HashSet<string> _variables = new(StringComparer.Ordinal);

        public void RegisterVariable(string variableName) => _variables.Add(variableName);
        public bool IsVariableRegistered(string variableName) => _variables.Contains(variableName);
    }
}
