using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AcidJunkie.Analyzers;

internal static class SyntaxTreeVisualizer
{
    public static string GetHierarchy(SyntaxNode rootNode)
    {
        var visitor = new Walker();
        visitor.Visit(rootNode);

        StringBuilder buffer = new();
        Dictionary<int, string> paddingsByLevel = [];
        var maxLevel = visitor.Nodes.Max(a => a.Level);
        var maxTypeLength = visitor.Nodes.Max(a => a.TypeName.Length);

        var indexDivider = (maxLevel * 2) + maxTypeLength + 2;

        foreach (var (level, _, typeName, node) in visitor.Nodes)
        {
            var currentLineLength = 0;
            var firstPadding = GetPaddingChars(level * 2);
            currentLineLength += firstPadding.Length;
            buffer.Append(firstPadding);

            buffer.Append(typeName);
            currentLineLength += typeName.Length;
            var secondPaddingLength = indexDivider - currentLineLength;

            var secondPadding = GetPaddingChars(secondPaddingLength);
            buffer.Append(secondPadding);
            buffer.Append("| ");
            buffer.AppendLine(node.ToString()?.Replace("\r\n", "\\n").Replace("\n", "\\n") ?? string.Empty);
        }

        return buffer.ToString();

        string GetPaddingChars(int count)
        {
            if (paddingsByLevel.TryGetValue(count, out var padding))
            {
                return padding;
            }

            padding = new string(' ', count);
            paddingsByLevel[count] = padding;

            return padding;
        }

    }

    private sealed class Walker : CSharpSyntaxWalker
    {
        private int _level;

        public readonly List<(int Level, string Kind, string TypeName, SyntaxNode Node)> Nodes = new(250);

        public override void Visit(SyntaxNode? node)
        {
            _level++;

            if (node is not null)
            {
                Nodes.Add((_level, node.Kind().ToString(), node.GetType().Name, node));
            }

            base.Visit(node);
            _level--;
        }
    }
}
