using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.DoNotAwaitTaskFromResult;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class DoNotAwaitTaskFromResultAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<DoNotAwaitTaskFromResultAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("await {|AJ0008:Task.FromResult(303)|};")]
    [InlineData("await OtherMethod();")]
    public async Task Theory(string insertionCode) => await RunTestAsync(insertionCode);

    [Theory]
    [InlineData(true, "await {|AJ0008:Task.FromResult(303)|};")]
    [InlineData(false, "await OtherMethod();")]
    public async Task Theory_IsEnabled(bool isEnabled, string methodContents) => await RunTestAsync(methodContents, isEnabled);

    private static string CreateTestCode(string insertionCode)
    {
        return $$"""
                 using System;
                 using System.Threading.Tasks;
                 using System.Collections.Generic;
                 using System.Collections.Immutable;

                 public static class Test
                 {
                     public static async Task TestMethod()
                     {
                         {{insertionCode}}
                     }

                     private static Task<int> OtherMethod()
                     {
                        return Task.FromResult(303);
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
             .SetEnabled(isEnabled, "AJ0008")
             .Build()
             .RunAsync();
    }
}
