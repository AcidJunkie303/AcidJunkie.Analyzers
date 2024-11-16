using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.Logging;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class WrongLoggerTypeArgumentAnalyzerTests : TestBase<WrongLoggerTypeArgumentAnalyzer>
{
    public WrongLoggerTypeArgumentAnalyzerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public Task WithConstructor_WhenNoParameters_ThenOk()
    {
        const string code = """
                            public class TestClass
                            {
                                public TestClass()
                                {
                                }
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WithConstructor_WhenNoLoggerParameter_ThenOk()
    {
        const string code = """
                            public class TestClass
                            {
                                public TestClass(string a, string b)
                                {
                                }
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WithConstructor_WhenUntypedLogger_ThenOk() // handled by different analyzer
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass(string a, ILogger logger)
                                {
                                }
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WithConstructor_WhenLoggerOfSameType_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass(string a, ILogger<TestClass> logger)
                                {
                                }
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WithConstructor_WhenLoggerOfDifferentType_ThenDiagnose()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass(string a, {|AJ0006:ILogger<string>|} logger)
                                {
                                }
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WithPrimaryConstructor_WhenLoggerOfDifferentType_ThenDiagnose()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass(string a, {|AJ0006:ILogger<string>|} logger)
                            {
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    // TODO: continue here
    //unit tests for this are complete
    //continue with  code fixer

    private CSharpAnalyzerTest<WrongLoggerTypeArgumentAnalyzer, DefaultVerifier> CreateTester(string code)
        => CreateTesterBuilder()
            .WithTestCode(code)
            .WithNugetPackage("Microsoft.Extensions.Logging.Abstractions", "8.0.2")
            .Build();
}
