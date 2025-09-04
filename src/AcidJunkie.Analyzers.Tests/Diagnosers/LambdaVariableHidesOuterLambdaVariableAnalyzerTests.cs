using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.LambdaVariableHidesOuterLambdaVariable;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class LambdaVariableHidesOuterLambdaVariableAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<LambdaVariableHidesOuterLambdaVariableAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("/* 01 */ Enumerable.Range(0,10).Where(x => true).Where(x => true);")]
    [InlineData("/* 02 */ Enumerable.Range(0,10).Where(x => true).Select( (i,x) => true);")]
    [InlineData("/* 03 */ new[] {string.Empty}.Select(x => x.Select({|AJ0009:x|} => x));")]
    [InlineData("/* 04 */ new[] {string.Empty}.Select((x,i) => x.Select(({|AJ0009:x|},{|AJ0009:i|}) => x));")]
    [InlineData("/* 05 */ new[] {string.Empty}.Select((x,i) => x.Select({|AJ0009:x|} =>x));")]
    [InlineData("/* 06 */ new[] {string.Empty}.Select((x,i) => x.Select((_,{|AJ0009:i|}) => i));")]
    [InlineData("/* 07 */ new[] {string.Empty}.Select((x,_) => x.Select((y,_) => x));")]
    public async Task Theory(string insertionCode)
        => await ValidateAsync(insertionCode);

    [Theory]
    [InlineData(true, "new[] {string.Empty}.Select(x => x.Select({|AJ0009:x|} => x));")]
    [InlineData(false, "new[] {string.Empty}.Select(x => x.Select(x => x));")]
    public Task Theory_IsEnabled(bool isEnabled, string insertionCode)
        => ValidateAsync(insertionCode, isEnabled);

    private static string CreateTestCode(string insertionCode)
    {
        return $$"""
                 using System;
                 using System.Linq;
                 using System.Collections.Generic;

                 public class Test
                 {
                     public static void TestMethod()
                     {
                         {{insertionCode}}
                     }
                 }
                 """;
    }

    private Task ValidateAsync(string insertionCode)
        => ValidateAsync(insertionCode, true);

    private async Task ValidateAsync(string insertionCode, bool isEnabled)
    {
        await CreateTesterBuilder()
             .WithTestCode(CreateTestCode(insertionCode))
             .SetEnabled(isEnabled, "AJ0009")
             .Build()
             .RunAsync();
    }
}
