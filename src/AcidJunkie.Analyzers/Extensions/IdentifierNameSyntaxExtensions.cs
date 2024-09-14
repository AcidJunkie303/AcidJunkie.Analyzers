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
        SyntaxNode? currentNode = node;

        var variableName = node.GetVariableName();

        while (currentNode is not null)
        {
            foreach (var childNode in currentNode.ChildNodes())
            {
                if (childNode is LocalDeclarationStatementSyntax variableDeclaration)
                {
                    var matchingVariable = variableDeclaration.Declaration.Variables.FirstOrDefault(a => variableName.EqualsOrdinal(a.Identifier.Text));

                    if (matchingVariable is not null)
                    {
                        return matchingVariable;
                    }
                }
            }

            if (stoppingTypes.Contains(currentNode.GetType()))
            {
                return null;
            }

            currentNode = currentNode.Parent;
        }

        return null;
    }
}
