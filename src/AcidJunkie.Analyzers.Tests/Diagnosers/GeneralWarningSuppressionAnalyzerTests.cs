using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.WarningSuppression;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class GeneralWarningSuppressionAnalyzerTests(ITestOutputHelper testOutputHelper) : TestBase<GeneralWarningSuppressionAnalyzer>(testOutputHelper)
{
    [Fact]
    public async Task WhenUsingGeneralWarningSuppression_ThenDiagnose()
    {
        const string code = "{|AJ0005:#pragma warning disable|}";

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public Task WhenUsingSpecificWarningSuppression_ThenOk()
    {
        const string code = "#pragma warning disable TB303";

        return ValidateAsync(code);
    }

    [Theory]
    [InlineData(true, "{|AJ0005:#pragma warning disable|}")]
    [InlineData(false, "#pragma warning disable")]
    public Task Theory_IsEnabled(bool isEnabled, string code) => ValidateAsync(code, isEnabled);

    private Task ValidateAsync(string code)
        => ValidateAsync(code, true);

    private Task ValidateAsync(string code, bool isEnabled)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .SetEnabled(isEnabled, "AJ0005")
          .Build()
          .RunAsync();
}
