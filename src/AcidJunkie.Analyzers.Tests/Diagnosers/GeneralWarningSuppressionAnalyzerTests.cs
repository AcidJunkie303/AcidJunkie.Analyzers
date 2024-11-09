using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.WarningSuppression;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class GeneralWarningSuppressionAnalyzerTests : TestBase<GeneralWarningSuppressionAnalyzer>
{
    public GeneralWarningSuppressionAnalyzerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

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
    public async Task WhenUsingSpecificWarningSuppression_ThenOk()
    {
        const string code = "#pragma warning disable TB303";

        await CreateTesterBuilder()
                .WithTestCode(code)
                .Build()
                .RunAsync();
    }
}
