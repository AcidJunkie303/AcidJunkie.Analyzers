using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0007;
using AcidJunkie.Analyzers.Diagnosers.ParameterOrdering;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

#pragma warning disable S125 // TODO: remove

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class ParameterOrderingAnalyzerTests(ITestOutputHelper testOutputHelper) : TestBase<ParameterOrderingAnalyzer>(testOutputHelper)
{
#pragma warning disable S125

    [Theory]
    [InlineData("string value")]
    [InlineData("ILogger logger, {|AJ00071:string value|}")]
    public Task Theory_OnMethod(string parameters)
    {
        var code = $$"""
                     using Microsoft.Extensions.Logging;

                     public class TestClass
                     {
                         public void Test({{parameters}})
                         {
                         }
                     }
                     """;

        return CreateTester(code).RunAsync();
    }

    /*
    [Fact]
    public Task WhenMethod_WithNoConfig_WhenNoParameters_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public void Test()
                                {
                                }
                            }

                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WhenMethod_WithNoConfig_WhenOneLoggerParameter_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public void Test(ILogger<TestClass> logger)
                                {
                                }
                            }

                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WhenMethod_WithNoConfig_WhenTwoLoggerParameters_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public void Test(ILogger<TestClass> logger1, ILogger<TestClass> logger2)
                                {
                                }
                            }

                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WhenMethod_WithNoConfig_WhenLoggerIsLastParameter_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public void Test(string value, ILogger<TestClass> logger)
                                {
                                }
                            }

                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WhenMethod_WithNoConfig_WhenLoggerAndThenCancellationTokenParameter_ThenOk()
    {
        const string code = """
                            using System.Threading;
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public void Test(string value, ILogger<TestClass> logger, CancellationToken cancellationToken)
                                {
                                }
                            }

                            """;

        return CreateTester(code).RunAsync();
    }



    /*

        [Fact]
    public Task AllTypes__WithNoConfig_WhenNoParameter_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass()
                                {
                                }
                                public void Bla()
                                {
                                }
                            }

                            public class TestClass2()
                            {
                            }

                            public interface ITestClass
                            {
                                public void Bla();
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

*/
    /*

    [Fact]
    public Task WithConstructor_WithNoConfig_WhenLoggerIsOnlyParameter_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass(ILogger<TestClass> logger)
                                {
                                }
                                public void Bla(ILogger<TestClass> logger)
                                {
                                }
                            }

                            public class TestClass2(ILogger<TestClass> logger)
                            {
                            }

                            public interface ITestClass
                            {
                                public void Bla(ILogger<TestClass> logger);
                            }
                            """;

        return CreateTester(code).RunAsync();
    }

    [Fact]
    public Task WithConstructor_WithNoConfig_WhenLoggerIsOnlyParameter_ThenOk()
    {
        const string code = """
                            using Microsoft.Extensions.Logging;

                            public class TestClass
                            {
                                public TestClass(ILogger<TestClass> logger)
                                {
                                }
                                public void Bla(ILogger<TestClass> logger)
                                {
                                }
                            }

                            public class TestClass2(ILogger<TestClass> logger)
                            {
                            }

                            public interface ITestClass
                            {
                                public void Bla(ILogger<TestClass> logger);
                            }
                            """;

        return CreateTester(code).RunAsync();
    }
    */

    /*
    ParameterList
        when immediate parent is ConstructorDeclarationSyntax or MethodDeclarationSyntax or ClassDeclarationSyntax

*/

    private CSharpAnalyzerTest<ParameterOrderingAnalyzer, DefaultVerifier> CreateTester(string code, string? configValueForLoggerParameterPlacement = null)
    {
        return CreateTesterBuilder()
            .WithTestCode(code)
            .WithNugetPackage("Microsoft.Extensions.Logging.Abstractions", "8.0.2")
            .WithGlobalOptions($"{Aj0007Configuration.KeyNames.ParameterOrderingFlat} = {configValueForLoggerParameterPlacement ?? string.Empty}")
            .Build();
    }
}
