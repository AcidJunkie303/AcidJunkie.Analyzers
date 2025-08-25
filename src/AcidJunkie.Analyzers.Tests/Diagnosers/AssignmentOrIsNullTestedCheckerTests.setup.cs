using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

public sealed partial class AssignmentOrIsNullTestedCheckerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AssignmentOrIsNullTestedCheckerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private static string CreateTestCode(string methodContents, string memberDefinition)
        => $$"""
             using System;

             public sealed class TestClass
             {
                {{memberDefinition}}

                public void Method()
                {
                    {{methodContents}}
                }
             }
             """;

    private void RunAndValidate(string code, string memberDeclaration, bool expectedResult)
    {
        var (semanticModel, member, method) = CreateTestObjects(code, memberDeclaration);
        AssignmentOrIsNullTestedChecker.IsMemberAssignedOrNullCheckedOnAllExecutionPaths(semanticModel, method, member).ShouldBe(expectedResult);
    }

    private (SemanticModel SemanticModel, SyntaxNode Member, MethodDeclarationSyntax Method) CreateTestObjects(string code, string memberDefinition)
    {
        var (root, semanticModel) = CreateTestSyntaxTree(code, memberDefinition);
        var walker = new Walker();
        walker.Visit(root);

        if (walker.Method is null)
        {
            throw new InvalidOperationException("Unable to extract test method called 'Method'");
        }

        if (walker.Member is null)
        {
            throw new InvalidOperationException("Unable to extract test member called 'PropertyOrField'");
        }

        return (semanticModel, walker.Member, walker.Method);
    }

    [SuppressMessage("Design", "MA0045:Do not use blocking calls in a sync method (need to make calling method async)")]
    private (SyntaxNode Root, SemanticModel SemanticModel) CreateTestSyntaxTree(string methodContents, string memberDefinition)
    {
        var code = CreateTestCode(methodContents, memberDefinition);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var error = syntaxTree.GetDiagnostics().FirstOrDefault(d => d.Severity == DiagnosticSeverity.Error);
        if (error is not null)
        {
            throw new ArgumentException($"Syntax tree contains error: {error}", nameof(memberDefinition));
        }

        LogCodeAndSyntaxTree(code);
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ArgumentNullException).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create("TestCompilation")
                                           .AddReferences(references)
                                           .AddSyntaxTrees(syntaxTree);
        return (syntaxTree.GetRoot(), compilation.GetSemanticModel(syntaxTree));
    }

    private void LogCodeAndSyntaxTree(string code)
    {
        _testOutputHelper.WriteLine("TestCode");
        _testOutputHelper.WriteLine("================================================================");
        _testOutputHelper.WriteLine(code);
        _testOutputHelper.WriteLine("================================================================");

        TestFileMarkupParser.GetSpans(code, out var markupFreeCode, out ImmutableArray<TextSpan> _);
        var tree = CSharpSyntaxTree.ParseText(markupFreeCode);
        var root = tree.GetCompilationUnitRoot();
        var hierarchy = SyntaxTreeVisualizer.VisualizeHierarchy(root);
        _testOutputHelper.WriteLine(hierarchy);
        _testOutputHelper.WriteLine("================================================================");
    }

    private sealed class Walker : CSharpSyntaxWalker
    {
        public MethodDeclarationSyntax? Method { get; private set; }
        public SyntaxNode? Member { get; private set; }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Identifier.Text.EqualsOrdinal("Method"))
            {
                Method = node;
                return;
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Identifier.Text.EqualsOrdinal("PropertyOrField"))
            {
                Member = node;
                return;
            }

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            foreach (var declarator in node.Declaration.Variables)
            {
                if (declarator.Identifier.Text.EqualsOrdinal("PropertyOrField"))
                {
                    Member = declarator;
                    return;
                }
            }

            base.VisitFieldDeclaration(node);
        }
    }
}
