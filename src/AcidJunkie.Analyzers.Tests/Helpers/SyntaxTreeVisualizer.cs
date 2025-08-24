using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AcidJunkie.Analyzers.Tests.Helpers;

internal static class SyntaxTreeVisualizer
{
    public static string VisualizeHierarchy(SyntaxNode rootNode)
    {
        var visitor = new Walker();
        visitor.Visit(rootNode);

        var items = visitor.Nodes
                           .ConvertAll(static a =>
                                (
                                    IndentedTypeName: new string(' ', a.Level * 2) + a.TypeName,
                                    a.Kind,
                                    a.Node)
                            );

        var maxTypeNameLength = items.Max(static a => a.IndentedTypeName.Length);
        var maxKindLength = items.Max(static a => a.Kind.Length);

        StringBuilder buffer = new();

        foreach (var (indentedTypeName, kind, node) in items)
        {
            var code = node
                      .ToString()
                      .Replace("\r\n", @"\r\n", StringComparison.Ordinal)
                      .Replace("\n", @"\n", StringComparison.Ordinal);

            buffer
               .Append(indentedTypeName.PadRight(maxTypeNameLength, ' '))
               .Append(" | ")
               .Append(kind.PadRight(maxKindLength))
               .Append(" | ")
               .AppendLine(code);
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
