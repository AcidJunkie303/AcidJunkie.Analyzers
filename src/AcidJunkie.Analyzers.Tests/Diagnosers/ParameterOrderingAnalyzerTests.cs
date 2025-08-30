using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0007;
using AcidJunkie.Analyzers.Diagnosers.ParameterOrdering;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class ParameterOrderingAnalyzerTests(ITestOutputHelper testOutputHelper) : TestBase<ParameterOrderingAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("(string value)")]
    [InlineData("{|AJ0007:(ILogger logger, string value)|}")]
    [InlineData("{|AJ0007:(ILogger<TestClass> logger, string value, CancellationToken cancellationToken)|}")]
    [InlineData("{|AJ0007:(CancellationToken cancellationToken, ILogger logger, string value)|}")]
    [InlineData("(string value, ILogger logger, CancellationToken cancellationToken, params string[] values)")]
    public Task Theory_OnMethod(string parameters)
    {
        var code = $$"""
                     using System.Threading;
                     using Microsoft.Extensions.Logging;

                     public class TestClass
                     {
                         public TestClass{{parameters}} // constructor
                         {
                         }

                         public void Test{{parameters}} // method
                         {
                         }
                     }
                     """;

        return CreateTester(code, true).RunAsync();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Theory_IsEnabled(bool isEnabled)
    {
        const string parameters = "(ILogger<TestClass> logger, string value)";

        var p = isEnabled
            ? "{|AJ0007:" + parameters + "|}"
            : parameters;

        var code = $$"""
                     using System.Threading;
                     using Microsoft.Extensions.Logging;

                     public class TestClass
                     {
                         public TestClass{{p}}
                         {
                         }
                     }
                     """;

        return CreateTester(code, isEnabled).RunAsync();
    }

    private static string CreateIsEnabledConfigurationLine(bool isEnabled) => $"AJ0007.is_enabled = {(isEnabled ? "true" : "false")}";

    private CSharpAnalyzerTest<ParameterOrderingAnalyzer, DefaultVerifier> CreateTester(string code, bool isEnabled, string? configValueForLoggerParameterPlacement = null)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .WithNugetPackage("Microsoft.Extensions.Logging.Abstractions", "9.0.8")
          .WithEditorConfigLine(CreateIsEnabledConfigurationLine(isEnabled))
          .WithEditorConfigLine($"{Aj0007Configuration.KeyNames.ParameterOrderingFlat} = {configValueForLoggerParameterPlacement ?? string.Empty}")
          .Build();
}
