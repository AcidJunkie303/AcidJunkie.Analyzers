using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.WarningSuppression;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class EnforceEntityFrameworkTrackingTypeAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<GeneralWarningSuppressionAnalyzer>(testOutputHelper)
{
    [Fact]
    public async Task First() => await RunTestAsync("");

    private static string CreateTestCode(string insertionCode)
    {
        return $$"""
                 #nullable enable

                 using System;
                 using System.Collections;
                 using System.Collections.Generic;
                 using System.Collections.Immutable;
                 using System.Collections.Frozen;
                 using System.Linq;
                 using Microsoft.EntityFrameworkCore;

                 namespace Tests;

                 public sealed class Entity
                 {
                     public int                     Id                  { get; set; }
                     public string                  Name                { get; set; } = null!;

                     public List<ProjectionEntity>  ProjectionEntities  { get; set; } = null!;
                     public ProjectionEntity        ProjectionEntity    { get; set; } = null!;
                 }

                 public sealed class ProjectionEntity
                 {
                     public int                     Id                  { get; set; }
                     public string                  Name                { get; set; } = null!;
                 }

                 public sealed class TestContext : DbContext
                 {
                     public DbSet<Entity>           Entities            { get; set; } = null!;
                     public DbSet<ProjectionEntity> ProjectionEntities  { get; set; } = null!;
                 }

                 public static class Test
                 {
                     public static void TestMethod()
                     {
                         {{insertionCode}}
                     }
                 }
                 """;
    }

    private async Task RunTestAsync(string insertionCode)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
             .WithTestCode(code)
             .WithNugetPackage("Microsoft.EntityFrameworkCore", "9.0.8")
             .Build()
             .RunAsync();
    }
}
