using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.ConstFirst;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class ConstFirstAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<ConstFirstAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("""
                const string value1 = "tb";
                const string value2 = "303";
                TestMethod();
                """)]
    [InlineData("""
                // Some comment
                const string value1 = "tb";
                """)]
    [InlineData("""
                string value1 = "tb";
                {|AJ0010:const string value2 = "303";|}
                TestMethod();
                """)]
    [InlineData("""
                TestMethod();
                string value1 = "tb";
                {|AJ0010:const string value2 = "303";|}
                """)]
    public async Task Theory(string methodContents) => await RunTestAsync(methodContents);

    [Theory]
    [InlineData(false, """
                       TestMethod();
                       const string value2 = "303";
                       """)]
    [InlineData(true,
        """
        TestMethod();
        {|AJ0010:const string value2 = "303";|}
        """)]
    public async Task Theory_IsEnabled(bool isEnabled, string methodContents) => await RunTestAsync(methodContents, isEnabled);

    private static string CreateTestCode(string methodContents)
    {
        return $$"""
                 using System;
                 using System.Threading.Tasks;
                 using System.Collections.Generic;
                 using System.Collections.Immutable;

                 public static class Test
                 {
                     public static void TestMethod()
                     {
                         {{methodContents}}
                     }
                 }
                 """;
    }

    private Task RunTestAsync(string insertionCode)
        => RunTestAsync(insertionCode, true);

    private async Task RunTestAsync(string insertionCode, bool isEnabled)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
             .WithTestCode(code)
             .SetEnabled(isEnabled, "AJ0010")
             .Build()
             .RunAsync();
    }
}
