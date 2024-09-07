using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

internal static class VariableDeclarationSyntaxExtensions
{
    public static bool IsConstDeclaration(this VariableDeclaratorSyntax node)
    {
        if (node.Parent?.Parent is LocalDeclarationStatementSyntax declaration)
        {
            return declaration.IsConst;
        }
        return false;
    }
}
