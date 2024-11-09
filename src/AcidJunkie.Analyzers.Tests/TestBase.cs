using AcidJunkie.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests;

// ReSharper disable once UnusedMember.Global -> These are methods which can be used during development time to check the syntax tree

public abstract class TestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected ITestOutputHelper TestOutputHelper { get; }

    protected TestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    protected  CSharpAnalyzerTestBuilder<TAnalyzer> CreateTesterBuilder() => CSharpAnalyzerTestBuilder.Create<TAnalyzer>(TestOutputHelper);
}
