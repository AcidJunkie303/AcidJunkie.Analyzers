using AcidJunkie.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Tests;

public abstract class TestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{

    /// <summary>
    /// Used to traverse the code for debugging purposes
    /// </summary>
    /// <param name="code"></param>
    protected static void Traverse(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var walker = new Walker();
        walker.Visit(tree.GetCompilationUnitRoot());
    }

    /// <summary>
    /// Returns a formatted string showing the code hierarchy
    /// </summary>
    /// <param name="node"></param>
    protected static string ShowTree(SyntaxNode node) => SyntaxTreeVisualizer.GetHierarchy(node);

    protected static CSharpAnalyzerTestBuilder<TAnalyzer> CreateTesterBuilder() => CSharpAnalyzerTestBuilder.Create<TAnalyzer>();
}
