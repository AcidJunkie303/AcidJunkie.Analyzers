using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AcidJunkie.Analyzers;

internal static class SyntaxTreeVisualizer
{
    public static string GetHierarchy(SyntaxNode node)
    {
        var visitor = new Walker();
        visitor.Visit(node);

        StringBuilder buffer = new();

        var maxKindLength = visitor.Nodes.Max(a => a.Kind.Length);

        foreach (var (level, kind, typeName, _) in visitor.Nodes)
        {
            buffer.Append(kind.PadRight(maxKindLength + 1));
            buffer.Append("| ");
            buffer.Append(new string(' ', level * 2));
            buffer.AppendLine(typeName);
        }

        return buffer.ToString();
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
