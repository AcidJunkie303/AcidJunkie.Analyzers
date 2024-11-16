using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.Logging;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.VerifyCodeFixAsync()")]
public sealed class WrongLoggerTypeArgumentFixProviderTests(ITestOutputHelper testOutputHelper) : CodeFixTestBase<WrongLoggerTypeArgumentAnalyzer, WrongLoggerTypeArgumentFixProvider>(testOutputHelper)
{
    [Fact]
    public Task WhenLoggerWithDifferentTypeArgumentThanEnclosingType_ThenFix()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass({|AJ0006:ILogger<object>|} logger)
                                {
                                }
                            }
                            """;

        const string fixedCode = """
                                 using Microsoft.Extensions.Logging;

                                 public class TestClass
                                 {
                                     public TestClass(ILogger<TestClass> logger)
                                     {
                                     }
                                 }
                                 """;

        return CreateTester(code, fixedCode).RunAsync();
    }

    private CSharpCodeFixTest<WrongLoggerTypeArgumentAnalyzer, WrongLoggerTypeArgumentFixProvider, DefaultVerifier> CreateTester(string code, string fixedCode)
    {
        return CreateTestBuilder()
            .WithTestCode(code)
            .WithFixedCode(fixedCode)
            .WithNugetPackage("Microsoft.Extensions.Logging.Abstractions", "9.0.0")
            .Build();
    }
}
