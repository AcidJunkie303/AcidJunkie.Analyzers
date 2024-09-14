using AcidJunkie.Analyzers.Tests.Helpers;
using AcidJunkie.Analyzers.Tests.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Data.SqlClient;

namespace AcidJunkie.Analyzers.Tests;

public abstract class TestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected static DiagnosticResult CreateExpectedDiagnostic(string diagnosticId, int startLine, int startCharacter, int endLine, int endCharacter, string? expectedMessage = null)
    {
        var verifier = CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>
            .Diagnostic(diagnosticId)
            .WithSpan(startLine, startCharacter, endLine, endCharacter);

        if (expectedMessage is not null)
        {
            verifier = verifier.WithMessage(expectedMessage);
        }

        return verifier;
    }

    protected static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateAnalyzerTest(string code, params DiagnosticResult[] expectedDiagnosticResults)
    {
        var analyzerTest = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources = {code},
#if NET8_0
                ReferenceAssemblies = Net.Assemblies.Net80,
#elif NET6_0
                ReferenceAssemblies = Net.Assemblies.Net60,
#elif NETCOREAPP3_1_OR_GREATER
                ReferenceAssemblies = Net.Assemblies.NetCore31,
#else
                .NET framework not handled!
#endif

                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(SqlDataReader).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(TestBase<>).Assembly.Location)
                },
            },
        };

        analyzerTest.ExpectedDiagnostics.AddRange(expectedDiagnosticResults);

        return analyzerTest;
    }

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
