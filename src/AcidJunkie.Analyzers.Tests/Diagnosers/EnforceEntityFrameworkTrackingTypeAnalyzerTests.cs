using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.EnforceEntityFrameworkTrackingType;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class EnforceEntityFrameworkTrackingTypeAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<EnforceEntityFrameworkTrackingTypeAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData(true, "Strict", ".AsTracking()")]
    [InlineData(true, "Strict", ".AsNoTracking()")]
    [InlineData(false, "Strict", "")]
    [InlineData(true, "Relaxed", ".AsTracking()")]
    [InlineData(true, "Relaxed", ".AsNoTracking()")]
    [InlineData(false, "Relaxed", "")]
    public async Task Theory_ReturningEntity(bool isOk, string mode, string trackingPart)
    {
        var entityPart = isOk ? "dbContext.Entities" : "{|AJ0002:dbContext.Entities|}";
        var code = $"""
                    using var dbContext = new TestContext();
                    {entityPart}{trackingPart}.ToList();
                    """;

        await RunTestAsync(code, mode);
    }

    [Theory]
    [InlineData(true, "Strict", ".AsTracking()")]
    [InlineData(true, "Strict", ".AsNoTracking()")]
    [InlineData(false, "Strict", "")]
    [InlineData(true, "Relaxed", ".AsTracking()")]
    [InlineData(true, "Relaxed", ".AsNoTracking()")]
    [InlineData(false, "Relaxed", "")]
    public async Task Theory_NotReturningEntity(bool isOk, string mode, string trackingPart)
    {
        var entityPart = isOk ? "dbContext.Entities" : "{|AJ0002:dbContext.Entities|}";
        var code = $"""
                    using var dbContext = new TestContext();
                    {entityPart}{trackingPart}.Select(a=>a.Id).ToList();
                    """;

        await RunTestAsync(code, mode);
    }

    [Theory]
    [InlineData(true, "Strict", ".AsTracking()")]
    [InlineData(true, "Strict", ".AsNoTracking()")]
    [InlineData(false, "Strict", "")]
    [InlineData(true, "Relaxed", ".AsTracking()")]
    [InlineData(true, "Relaxed", ".AsNoTracking()")]
    [InlineData(false, "Relaxed", "")]
    public async Task Theory_ReturningAnonymousTypeWithSubPropertyOfEntityType(bool isOk, string mode, string trackingPart)
    {
        var entityPart = isOk ? "dbContext.Entities" : "{|AJ0002:dbContext.Entities|}";
        var code = $$"""
                     using var dbContext = new TestContext();
                     {{entityPart}}{{trackingPart}}.Select(a=> new { Id = a.Id, Sub = new { MyEntity = a } }).ToList();
                     """;

        await RunTestAsync(code, mode);
    }

    [Theory]
    [InlineData(true, "Strict", ".AsTracking()")]
    [InlineData(true, "Strict", ".AsNoTracking()")]
    [InlineData(false, "Strict", "")]
    [InlineData(true, "Relaxed", ".AsTracking()")]
    [InlineData(true, "Relaxed", ".AsNoTracking()")]
    [InlineData(false, "Relaxed", "")]
    public async Task Theory_ReturningAnonymousTypeWithEntityCollectionProperty(bool isOk, string mode, string trackingPart)
    {
        var entityPart = isOk ? "dbContext.Entities" : "{|AJ0002:dbContext.Entities|}";
        var code = $$"""
                     using var dbContext = new TestContext();
                     {{entityPart}}{{trackingPart}}.Select(a=> new { Entities = a.ProjectionEntities.ToList() }).ToList();
                     """;

        await RunTestAsync(code, mode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Theory_IsEnabled(bool isEnabled)
    {
        var entityPart = isEnabled ? "{|AJ0002:dbContext.Entities|}" : "dbContext.Entities";
        var code = $$"""
                     using var dbContext = new TestContext();
                     {{entityPart}}.ToList();
                     """;

        await RunTestAsync(code, "Strict", isEnabled);
    }

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

    private static string CreateModeConfigurationLine(string mode) => $"dotnet_diagnostic.AJ0002.mode = {mode}";
    private static string CreateIsEnabledConfigurationLine(bool isEnabled) => $"AJ0002.is_enabled = {(isEnabled ? "true" : "false")}";

    private Task RunTestAsync(string insertionCode, string mode)
        => RunTestAsync(insertionCode, mode, true);

    private async Task RunTestAsync(string insertionCode, string mode, bool isEnabled)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
             .WithTestCode(code)
             .WithEditorConfigLine(CreateModeConfigurationLine(mode))
             .WithEditorConfigLine(CreateIsEnabledConfigurationLine(isEnabled))
             .WithNugetPackage("Microsoft.EntityFrameworkCore", "9.0.8")
             .Build()
             .RunAsync();
    }
}
