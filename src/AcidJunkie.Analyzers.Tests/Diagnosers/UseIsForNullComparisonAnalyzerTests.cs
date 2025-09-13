using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.UseIsForNullComparison;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class UseIsForNullComparisonAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<UseIsForNullComparisonAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData(@"_ = value == ""a"";")]
    [InlineData(@"_ = value != ""a"";")]
    [InlineData("int? a = 303; _ = a == null;")]
    [InlineData("int? a = 303; _ = a != null;")]
    [InlineData("_ = value {|AJ0011:==|} null;")]
    [InlineData("_ = value {|AJ0011:!=|} null;")]
    public Task Theory(string insertionCode)
    {
        var code = $$"""
                     public static class Test
                     {
                         public static void DoSomething(string value)
                         {
                             {{insertionCode}}
                         }
                     }
                     """;

        return ValidateAsync(code);
    }

    [Fact]
    public Task WhenIsInQueryableWhere_ThenOk()
    {
        const string code = """
                            using System.Linq;

                            public static class Test
                            {
                                public static void DoSomething( string value)
                                {
                                    var _ = GetQueryable().Where(a => a == null);
                                }

                                private static IQueryable<string> GetQueryable()
                                   => new string[0].AsQueryable();
                            }
                            """;

        return ValidateAsync(code);
    }

    [Theory]
    [InlineData(true, "var isNull = value {|AJ0011:==|} null;")]
    [InlineData(false, "var isNull = value == null;")]
    public Task Theory_IsEnabled(bool isEnabled, string insertionCode)
    {
        var code = $$"""
                     public static class Test
                     {
                         public static void DoSomething( string value)
                         {
                            {{insertionCode}}
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
          .SetEnabled(isEnabled, "AJ0011")
          .Build()
          .RunAsync();
}
