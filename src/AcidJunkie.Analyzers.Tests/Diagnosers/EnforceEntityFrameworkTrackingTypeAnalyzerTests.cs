using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.EnforceEntityFrameworkTrackingType;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class EnforceEntityFrameworkTrackingTypeAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<EnforceEntityFrameworkTrackingTypeAnalyzer>(testOutputHelper)
{
    [Fact]
    public async Task WithoutAnyTracking_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.Where(a => a.Id > 303).ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithAsTracking_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(a => a.Id > 303).AsTracking().ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithAsNoTracking_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(a => a.Id > 303).AsNoTracking().ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithNoTracking_WhenSelectingNonEntity_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(a => a.Id > 303).Select(a => a.Name).ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithNoTracking_WhenSelectingAnonymousTypeWithoutEntity_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(a => a.Id > 303).Select(a => new {a.Name}).ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithNoTracking_WhenSelectingAnonymousTypeWithEntity_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.Where(a => a.Id > 303).Select(a => new { Related = a.ProjectionEntity} ).ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithNoTracking_WhenSelectingAnonymousTypeWithSubTypeAndEntity_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.Where(a => a.Id > 303).Select(a => new { Sub = new { Related = a.ProjectionEntity} } ).ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithNoTracking_WhenSelectingAnonymousTypeWithEntityEnumerable_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.Where(a => a.Id > 303).Select(a => new { Prop1 = (IEnumerable<ProjectionEntity>) a.ProjectionEntities } ).ToList();
                            """;
        await RunTestAsync(code);
    }

    [Fact]
    public async Task WithNoTracking_WhenSelectingAnonymousTypeWithEntityCollection_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            {|AJ0002:dbContext.Entities|}.Where(a => a.Id > 303).Select(a => new { Prop1 = a.ProjectionEntities } ).ToList();
                            """;
        await RunTestAsync(code);
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
