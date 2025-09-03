using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.ExtensionClassName;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class ExtensionClassNameAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<ExtensionClassNameAnalyzer>(testOutputHelper)
{
    [Fact]
    public Task WithExtensionMethods_WhenClassNameHasExtensionsSuffix_ThenOk()
    {
        const string code = """
                            public static class MyExtensions
                            {
                                public static void DoSomething(this string input)
                                {
                                }
                            }
                            """;

        return ValidateAsync(code);
    }

    [Fact]
    public Task WithExtensionMethods_WhenClassNameHasNoExtensionsSuffix_ThenDiagnose()
    {
        const string code = """
                            public static class {|AJ0006:My|}
                            {
                                public static void DoSomething(this string input)
                                {
                                }
                            }
                            """;

        return ValidateAsync(code);
    }

    [Fact]
    public Task WithoutExtensionMethods_WhenClassNameHasNoExtensionsSuffix_ThenOk()
    {
        const string code = """
                            public static class My
                            {
                                public static void DoSomething(string input)
                                {
                                }
                            }
                            """;

        return ValidateAsync(code);
    }

    [Theory]
    [InlineData(true, "{|AJ0006:My|}")]
    [InlineData(false, "My")]
    public Task Theory_IsEnabled(bool isEnabled, string className)
    {
        var code = $$"""
                     public static class {{className}}
                     {
                         public static void DoSomething(this string input)
                         {
                         }
                     }
                     """;

        return ValidateAsync(code, isEnabled);
    }

    private Task ValidateAsync(string code)
        => ValidateAsync(code, true);

    private Task ValidateAsync(string code, bool isEnabled)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .SetEnabled(isEnabled, "AJ0006")
          .Build()
          .RunAsync();
}
