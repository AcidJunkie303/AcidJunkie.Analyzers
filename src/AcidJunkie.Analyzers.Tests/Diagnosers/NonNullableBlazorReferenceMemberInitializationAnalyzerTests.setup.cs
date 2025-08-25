using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0008;
using AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

public sealed partial class NonNullableBlazorReferenceMemberInitializationAnalyzerTests : TestBase<NonNullableBlazorReferenceMemberInitializationAnalyzer>
{
    public NonNullableBlazorReferenceMemberInitializationAnalyzerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    private static string CreateTestCode(Nullability nullability, string insertionCode)
    {
        var nullableDirective = nullability switch
        {
            Nullability.Enabled  => "#nullable enable",
            Nullability.Disabled => "#nullable disable",
            _                    => string.Empty
        };

        return $$"""
                 using System;
                 using System.Threading.Tasks;
                 using Microsoft.AspNetCore.Components;

                 {{nullableDirective}}

                 public class TestComponent : ComponentBase
                 {
                     {{insertionCode}}
                 }
                 """;
    }

    // TODO: remove
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private Task RunTestAsync(Nullability nullability, string insertionCode, params MethodKinds[] methodsToCheck)
        => RunTestAsync(nullability, insertionCode, (IReadOnlyList<MethodKinds>)methodsToCheck);

    private Task RunTestAsync(Nullability nullability, string insertionCode, IReadOnlyList<MethodKinds> methodsToCheck)
    {
        var configurationValue = methodsToCheck.Count == 0
            ? string.Empty
            : string.Join('|', methodsToCheck);

        return CreateTesterBuilder()
              .WithTestCode(CreateTestCode(nullability, insertionCode))
              .WithNugetPackage("Microsoft.AspNetCore.Components.Web", "9.0.8")
              .WithEditorConfigLine($"AJ0008.methods_to_check = {configurationValue}")
              .Build()
              .RunAsync();
    }

    [SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
    public enum Nullability
    {
        None = 0,
        Enabled = 1,
        Disabled = 2
    }
}
