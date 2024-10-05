using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S4144:Methods should not have identical implementations", Justification = "Splitted up the test into different methods for different categories")]
[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class MissingEqualityComparerAnalyzerTests : TestBase<MissingEqualityComparerAnalyzer>
{
    [Theory]
    //
    // Enumerable.Contains() for RefType
    //
    [InlineData("/* 0000 */  refTypeCollection.Contains( new(), refTypeEqualityComparer );")]
    [InlineData("/* 0001 */  refTypeCollection.{|AJ0001:Contains|}( new() );")]
    //
    // Enumerable.Distinct() for RefType
    //
    [InlineData("/* 0002 */  refTypeCollection.Distinct( refTypeEqualityComparer );")]
    [InlineData("/* 0003 */  refTypeCollection.{|AJ0001:Distinct|}();")]
    //
    // Enumerable.DistinctBy() for RefType
    //
    [InlineData("/* 0004 */  refTypeCollection.DistinctBy( a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0005 */  refTypeCollection.{|AJ0001:DistinctBy|}( a => a );")]
    //
    // Enumerable.Except() for RefType
    //
    [InlineData("/* 0006 */  refTypeCollection.Except( [], refTypeEqualityComparer );")]
    [InlineData("/* 0007 */  refTypeCollection.{|AJ0001:Except|}( [] );")]
    //
    // Enumerable.ExceptBy() for RefType
    //
    [InlineData("/* 0008 */  refTypeCollection.ExceptBy( [], a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0009 */  refTypeCollection.{|AJ0001:ExceptBy|}( [], a => a );")]
    //
    // Enumerable.GroupBy() for RefType
    //
    [InlineData("/* 0010 */  refTypeCollection.GroupBy( a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0011 */  refTypeCollection.{|AJ0001:GroupBy|}( a => a);")]
    //
    // Enumerable.GroupJoin() for RefType
    //
    [InlineData("/* 0012 */  refTypeCollection.GroupJoin( new RefType[0], a => a, a => a, (a,b) => a, refTypeEqualityComparer );")]
    [InlineData("/* 0013 */  refTypeCollection.{|AJ0001:GroupJoin|}( new RefType[0], a => a, a => a, (a,b) => a );")]
    //
    // Enumerable.Intersect() for RefType
    //
    [InlineData("/* 0014 */  refTypeCollection.Intersect( [], refTypeEqualityComparer );")]
    [InlineData("/* 0015 */  refTypeCollection.{|AJ0001:Intersect|}( [] );")]
    //
    // Enumerable.IntersectBy() for RefType
    //
    [InlineData("/* 0016 */  refTypeCollection.IntersectBy( [], a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0017 */  refTypeCollection.{|AJ0001:IntersectBy|}( [], a => a );")]
    //
    // Enumerable.Join() for RefType
    //
    [InlineData("/* 0018 */  refTypeCollection.Join( new RefType[0], a => a, a => a, (a,b) => a, refTypeEqualityComparer );")]
    [InlineData("/* 0019 */  refTypeCollection.{|AJ0001:Join|}( new RefType[0], a => a, a => a, (a,b) => a );")]
    //
    // Enumerable.SequenceEqual() for RefType
    //
    [InlineData("/* 0020 */  refTypeCollection.SequenceEqual( [], refTypeEqualityComparer );")]
    [InlineData("/* 0021 */  refTypeCollection.{|AJ0001:SequenceEqual|}( [] );")]
    //
    // Enumerable.ToDictionary() for RefType
    //
    [InlineData("/* 0022 */  refTypeCollection.ToDictionary( a => a, a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0023 */  refTypeCollection.{|AJ0001:ToDictionary|}( a => a, a => a );")]
    //
    // Enumerable.ToHashSet() for RefType
    //
    [InlineData("/* 0024 */  refTypeCollection.ToHashSet( refTypeEqualityComparer );")]
    [InlineData("/* 0025 */  refTypeCollection.{|AJ0001:ToHashSet|}();")]
    //
    // Enumerable.ToDictionary() for RefType
    //
    [InlineData("/* 0026 */  refTypeCollection.ToLookup( a => a, a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0027 */  refTypeCollection.{|AJ0001:ToLookup|}( a => a, a => a );")]
    //
    // Enumerable.Union() for RefType
    //
    [InlineData("/* 0028 */  refTypeCollection.Union( [], refTypeEqualityComparer );")]
    [InlineData("/* 0029 */  refTypeCollection.{|AJ0001:Union|}( [] );")]
    //
    // Enumerable.UnionBy() for RefType
    //
    [InlineData("/* 0030 */  refTypeCollection.UnionBy( [], a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0031 */  refTypeCollection.{|AJ0001:UnionBy|}( [], a => a );")]
    //
    // ImmutableDictionary.Create() method for RefType
    //
    [InlineData("/* 0100 */  ImmutableDictionary.Create<RefType,int>(refTypeEqualityComparer);")]
    [InlineData("/* 0101 */  ImmutableDictionary.{|AJ0001:Create<RefType,int>|}();")]
    //
    // ImmutableDictionary.CreateRange() method for RefType
    //
    [InlineData("/* 0102 */  ImmutableDictionary.CreateRange<RefType,int>( refTypeEqualityComparer, [] );")]
    [InlineData("/* 0103 */  ImmutableDictionary.{|AJ0001:CreateRange<RefType,int>|}( [] );")]
    //
    // ImmutableDictionary.CreateBuilder() method for RefType
    //
    [InlineData("/* 0104 */  ImmutableDictionary.CreateBuilder<RefType,int>( refTypeEqualityComparer );")]
    [InlineData("/* 0105 */  ImmutableDictionary.{|AJ0001:CreateBuilder<RefType,int>|}( );")]
    //
    // ImmutableDictionary.ToImmutableDictionary() for RefType
    //
    [InlineData("/* 0106 */  refTypeCollection.ToImmutableDictionary( a => a, a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0107 */  refTypeCollection.{|AJ0001:ToImmutableDictionary|}( a => a, a => a );")]
    //
    // ImmutableHashSet.Create() method for RefType
    //
    [InlineData("/* 0108 */  ImmutableHashSet.Create<RefType>(refTypeEqualityComparer);")]
    [InlineData("/* 0109 */  ImmutableHashSet.{|AJ0001:Create<RefType>|}();")]
    //
    // ImmutableHashSet.CreateRange() method for RefType
    //
    [InlineData("/* 0110 */  ImmutableHashSet.CreateRange<RefType>( refTypeEqualityComparer, [] );")]
    [InlineData("/* 0111 */  ImmutableHashSet.{|AJ0001:CreateRange<RefType>|}( [] );")]
    //
    // ImmutableHashSet.CreateBuilder() method for RefType
    //
    [InlineData("/* 0112 */  ImmutableHashSet.CreateBuilder<RefType>( refTypeEqualityComparer );")]
    [InlineData("/* 0113 */  ImmutableHashSet.{|AJ0001:CreateBuilder<RefType>|}( );")]
    //
    // ImmutableHashSet.ToImmutableHashSet() for RefType
    //
    [InlineData("/* 0114 */  refTypeCollection.ToImmutableHashSet( refTypeEqualityComparer );")]
    [InlineData("/* 0115 */  refTypeCollection.{|AJ0001:ToImmutableHashSet|}( );")]
    //
    // FrozenDictionary.ToFrozenDictionary() for RefType
    //
    [InlineData("/* 0200 */  refTypeCollection.ToFrozenDictionary( a => a, a => a, refTypeEqualityComparer );")]
    [InlineData("/* 0201 */  refTypeCollection.{|AJ0001:ToFrozenDictionary|}( a => a, a => a );")]
    //
    // FrozenSet.ToFrozenSet() for RefType
    //
    [InlineData("/* 0300 */  refTypeCollection.ToFrozenSet( refTypeEqualityComparer );")]
    [InlineData("/* 0301 */  refTypeCollection.{|AJ0001:ToFrozenSet|}( );")]
    public async Task Theory_Various_Method_Invocations(string insertionCode)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Theory]
    [InlineData("/* 1000 */  new Dictionary<RefType,int>( refTypeEqualityComparer );")]
    [InlineData("/* 1001 */  {|AJ0001:new Dictionary<RefType,int>|}();")]
    [InlineData("/* 1002 */  Dictionary<RefType,int> dict = new ( refTypeEqualityComparer );")] // implicit creation
    [InlineData("/* 1003 */  Dictionary<RefType,int> dict = {|AJ0001:new|} ( );")] // implicit creation
    public async Task Theory_DictionaryCreation(string insertionCode)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Theory]
    [InlineData("/* 9001 */  refTypeCollection.ToHashSet( refTypeEqualityComparer );")] // variable
    [InlineData("/* 9002 */  refTypeCollection.ToHashSet( (IEqualityComparer<RefType>) refTypeEqualityComparer );")] // variable with cast
    [InlineData("/* 9003 */  refTypeCollection.ToHashSet( RefType.EqualityComparers.Default );")] // property
    [InlineData("/* 9004 */  refTypeCollection.ToHashSet( GetRefTypeEqualityComparer() );")] // method
    [InlineData("/* 9005 */  refTypeCollection.{|AJ0001:ToHashSet|}( null );")] // null reference
    public async Task Theory_CheckVariousWaysOfPassingEqualityComparer(string insertionCode)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenInvocationOnValueType_ThenOk()
    {
        // value types perform structural comparison by default -> no equality comparer required
        var code = CreateTestCode("valueTypeCollection.ToHashSet();");

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenPassingNullEqualityComparer_ThenDiagnose()
    {
        // value types perform structural comparison by default -> no equality comparer required

        var code = CreateTestCode("refTypeCollection.{|AJ0001:ToHashSet|}( );");

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenFullyEquatableRefType_ThenOk()
    {
        // FullEquatableRefType implements IEquatable<T> and overrides GetHashCode() => no equality comparer required

        var code = CreateTestCode("fullEquatableRefTypeCollection.Distinct();");

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenPartialEquatableRefType_ThenOk()
    {
        // PartialEquatableRefType implements IEquatable<T> but does not override GetHashCode() => equality comparer required

        var code = CreateTestCode("partialEquatableRefTypeCollection.{|AJ0001:Distinct|}();");

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenNonEquatableRefType_ThenOk()
    {
        // RefType does not implement IEquatable<T> and does not override GetHashCode() => equality comparer required

        var code = CreateTestCode("partialEquatableRefTypeCollection.{|AJ0001:Distinct|}();");

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    private static string CreateTestCode(string insertionCode) =>
        $$"""
          using System;
          using System.Collections;
          using System.Collections.Generic;
          using System.Collections.Immutable;
          using System.Collections.Frozen;
          using System.Linq;

          namespace Tests;

          public static class Test
          {
              public static void TestMethod()
              {
                  var refTypeCollection = new RefType[0];
                  var refTypeEqualityComparer = new RefTypeEqualityComparer();
          
                  var partialEquatableRefTypeCollection = new PartialEquatableRefType[0];
                  var partialEquatableRefTypeEqualityComparer = new PartialEquatableRefTypeEqualityComparer();
          
                  var fullEquatableRefTypeCollection = new FullEquatableRefType[0];
                  var fullEquatableRefTypeEqualityComparer = new FullEquatableRefTypeEqualityComparer();
          
                  var valueTypeCollection = new ValueType[0];
                  
                  {{insertionCode}}
              }
              
              private static IEqualityComparer<RefType> GetRefTypeEqualityComparer() => new RefTypeEqualityComparer();
              private static IEqualityComparer<PartialEquatableRefType> GePartialEquatableRefTypeEqualityComparer() => new PartialEquatableRefTypeEqualityComparer();
              private static IEqualityComparer<FullEquatableRefType> GetFullEquatableRefTypeEqualityComparer() => new FullEquatableRefTypeEqualityComparer();
              
          }

          ////////////////////////////////////////////////////////////////
          // Type definitions
          ////////////////////////////////////////////////////////////////
          public sealed class RefType
          {
              public string StringValue { get; set; }
              public int IntValue { get; set; }
              
              public static class EqualityComparers
              {
                  public static IEqualityComparer<RefType> Default { get; } = new RefTypeEqualityComparer();
              } 
          }

          public struct ValueType
          {
              public string StringValue { get; set; }
              public int IntValue { get; set; }
          }

          public sealed class PartialEquatableRefType : IEquatable<PartialEquatableRefType>
          {
              public string StringValue { get; set; }
              public int IntValue { get; set; }
              
              public bool Equals(PartialEquatableRefType? other)
              {
                  return other is not null
                      && IntValue == other.IntValue
                      && StringValue == other.StringValue;
              }
              
              public static class EqualityComparers
              {
                  public static IEqualityComparer<PartialEquatableRefType> Default { get; } = new PartialEquatableRefTypeEqualityComparer();
              }
          }

          public sealed class FullEquatableRefType : IEquatable<FullEquatableRefType>
          {
              public string StringValue { get; set; }
              public int IntValue { get; set; }
              
              public bool Equals(FullEquatableRefType? other)
              {
                  return other is not null
                      && IntValue == other.IntValue
                      && StringValue == other.StringValue;
              }
              
              public override int GetHashCode()
              {
                  return StringComparer.Ordinal.GetHashCode(StringValue) ^ IntValue;
              }
              
              public static class EqualityComparers
              {
                  public static IEqualityComparer<FullEquatableRefType> Default { get; } = new FullEquatableRefTypeEqualityComparer();
              }
          }

          ////////////////////////////////////////////////////////////////
          // EqualityComparer definitions
          ////////////////////////////////////////////////////////////////
          public sealed class RefTypeEqualityComparer : IEqualityComparer<RefType>
          {
              // we don't care about the actual correct implementation
              public bool Equals(RefType? x, RefType? y) => true;
              public int GetHashCode(RefType item) => 0;
          }

          public sealed class PartialEquatableRefTypeEqualityComparer : IEqualityComparer<PartialEquatableRefType>
          {
              // we don't care about the actual correct implementation
              public bool Equals(PartialEquatableRefType? x, PartialEquatableRefType? y) => true;
              public int GetHashCode(PartialEquatableRefType item) => 0;
          }

          public sealed class FullEquatableRefTypeEqualityComparer : IEqualityComparer<FullEquatableRefType>
          {
              // we don't care about the actual correct implementation
              public bool Equals(FullEquatableRefType? x, FullEquatableRefType? y) => true;
              public int GetHashCode(FullEquatableRefType item) => 0;
          }
          """;
}
