using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

internal static class IdentifierNameSyntaxExtensions
{
    public static string GetVariableName(this IdentifierNameSyntax node) => node.Identifier.Text;

    public static string? GetVariableValue(this IdentifierNameSyntax node, params Type[] stoppingTypes)
    {
        var declarator = node.FindLocalDeclaratorAndCreation(stoppingTypes);
        return declarator?.Initializer?.Value.ToString();
    }

    public static string? GetVariableValue(this VariableDeclaratorSyntax node) => node?.Initializer?.Value.ToString();

    public static VariableDeclaratorSyntax? FindLocalDeclaratorAndCreation(this IdentifierNameSyntax node, params Type[] stoppingTypes)
    {
        SyntaxNode? n = node;

        var variableName = node.GetVariableName();

        while (n is not null)
        {
            foreach (var childNode in n.ChildNodes())
            {
                if (childNode is LocalDeclarationStatementSyntax variableDeclaration)
                {
                    foreach (var variable in variableDeclaration.Declaration.Variables)
                    {
                        if (variableName.EqualsOrdinal(variable.Identifier.Text))
                        {
                            return variable;
                        }
                    }
                }
            }

            if (stoppingTypes.Contains(n.GetType()))
            {
                return null;
            }

            n = n.Parent;
        }

        return null;
    }
}
