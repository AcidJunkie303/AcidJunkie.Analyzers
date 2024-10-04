using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

public static class SyntaxNodeExtensions
{

    public static T? FindFirstParentOfType<T>(this SyntaxNode node)
        where T : SyntaxNode
    {
        var current = node;

        while (current is not null)
        {
            if (current is T nodeToFind)
            {
                return nodeToFind;
            }

            current = current.Parent;
        }

        return null;
    }

    public static SyntaxNode? GetFirstParentNotOfType(this SyntaxNode node, params Type[] types)
    {
        if (types.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(types), "Must contain at least one type");
        }

        var current = node.Parent;

        while (current is not null)
        {
            if (!types.Contains(current.GetType()))
            {
                return current;
            }

            current = current.Parent;
        }

        return null;
    }

    public static IEnumerable<SyntaxNode> GetParents(this SyntaxNode node)
    {
        var current = node.Parent;

        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public static IReadOnlyList<SyntaxNode> Children(this SyntaxNode node) => node.ChildNodes().ToList();

    public static IEnumerable<SyntaxNode> GetSubsequentSiblings(this SyntaxNode node)
    {
        if (node.Parent is null)
        {
            yield break;
        }

        var foundSelf = false;
        foreach (var sibling in node.Parent.ChildNodes())
        {
            if (!foundSelf && ReferenceEquals(sibling, node))
            {
                foundSelf = true;
                continue;
            }

            if (foundSelf)
            {
                yield return sibling;
            }
        }
    }
}
