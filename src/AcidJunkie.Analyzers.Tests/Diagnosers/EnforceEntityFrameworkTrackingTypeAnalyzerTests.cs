using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.EnforceEntityFrameworkTrackingType;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class EnforceEntityFrameworkTrackingTypeAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<EnforceEntityFrameworkTrackingTypeAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("dbContext.Entities.AsTracking()")]
    [InlineData("dbContext.Entities.AsNoTracking()")]
    [InlineData("{|AJ0002:dbContext.Entities|}")]
    public async Task Theory_ReturningEntity(string insertionCode)
    {
        var code = $"""
                    using var dbContext = new TestContext();
                    {insertionCode}.ToList();
                    """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithoutTrackingType_WhenNotReturningAnyEntity_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Select(a=>a.Id).ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task WhenReturningAnonymousTypeWithSubPropertyOfEntityType_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.Select(a=> new { Id = a.Id, Sub = new { MyEntity = a } }).ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task WhenReturningDictionaryContainingEntity_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.ToDictionary(a => a.Id, a => a);
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task WhenReturningDictionaryNotContainingEntity_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.ToDictionary(a => a.Id, a => a.Name);
                            """;

        await RunTestAsync(code);
    }

    [Theory]
    [InlineData(true, "{|AJ0002:dbContext.Entities|}")]
    [InlineData(false, "dbContext.Entities")]
    public async Task Theory_IsEnabled(bool isEnabled, string entityPart)
    {
        var code = $$"""
                     using var dbContext = new TestContext();
                     {{entityPart}}.ToList();
                     """;

        await RunTestAsync(code, isEnabled);
    }

    [Theory]
    [InlineData("dbContext.Entities.Add(entity)")]
    [InlineData("dbContext.Entities.AddAsync(entity)")]
    [InlineData("dbContext.Entities.AddRange(entity)")]
    [InlineData("dbContext.Entities.AddRangeAsync(entity)")]
    [InlineData("dbContext.Entities.Attach(entity)")]
    [InlineData("dbContext.Entities.AttachRange(entity)")]
    [InlineData("dbContext.Entities.Remove(entity)")]
    [InlineData("dbContext.Entities.RemoveRange(entity)")]
    [InlineData("dbContext.Entities.Update(entity)")]
    [InlineData("dbContext.Entities.UpdateRange(entity)")]
    [InlineData("{|AJ0002:dbContext.Entities|}.Where(a=> true)")]
    public async Task WithoutTrackingType_WhenIgnoredMethodIsCalled_ThenOk(string part)
    {
        var code = $$"""
                     var entity = new Entity{ Name = "Test" };
                     using var dbContext = new TestContext();
                     {{part}};
                     """;

        await RunTestAsync(code, true);
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

    private Task RunTestAsync(string insertionCode)
        => RunTestAsync(insertionCode, true);

    private async Task RunTestAsync(string insertionCode, bool isEnabled)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
             .WithTestCode(code)
             .SetEnabled(isEnabled, "AJ0002")
             .WithNugetPackage("Microsoft.EntityFrameworkCore", "9.0.8")
             .Build()
             .RunAsync();
    }
}
