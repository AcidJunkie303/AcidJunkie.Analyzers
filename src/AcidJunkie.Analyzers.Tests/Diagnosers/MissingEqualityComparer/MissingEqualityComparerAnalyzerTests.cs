using AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

namespace AcidJunkie.Analyzers.Tests.Diagnosers.MissingEqualityComparer;

public class MissingEqualityComparerAnalyzerTests : TestBase<MissingEqualityComparerAnalyzer>
{
    // TODO:
    // Change testing strategy. Instead of creating a cartesian product of all possible cases, test separately.

    [Theory]
    //
    // LINQ method ToHashSet()
    // RefType => Does not Implement IEquatable<T> and does not override GetHashCode()
    //
    [InlineData("/* 0000 */  refTypeCollection.[|<AJ0001>ToHashSet|]();")]
    [InlineData("/* 0001 */  refTypeCollection.ToHashSet( refTypeEqualityComparer );")]
    [InlineData("/* 0002 */  refTypeCollection.ToHashSet( new RefTypeEqualityComparer() );")]
    [InlineData("/* 0003 */  refTypeCollection.ToHashSet( RefType.EqualityComparers.Default );")]
    [InlineData("/* 0004 */  refTypeCollection.ToHashSet( GetRefTypeEqualityComparer() );")]
    [InlineData("/* 0005 */  refTypeCollection.[|<AJ0001>ToHashSet|]( null );")]
    //
    // LINQ method ToHashSet()
    // PartialEquatableRefType => Does Implement IEquatable<T> and does not override GetHashCode()
    //
    [InlineData("/* 0010 */  partialEquatableRefTypeCollection.[|<AJ0001>ToHashSet|]();")]
    [InlineData("/* 0011 */  partialEquatableRefTypeCollection.ToHashSet( partialEquatableRefTypeEqualityComparer );")]
    [InlineData("/* 0012 */  partialEquatableRefTypeCollection.ToHashSet( new PartialEquatableRefTypeEqualityComparer() );")]
    [InlineData("/* 0013 */  partialEquatableRefTypeCollection.ToHashSet( PartialEquatableRefType.EqualityComparers.Default );")]
    [InlineData("/* 0014 */  partialEquatableRefTypeCollection.ToHashSet( GePartialEquatableRefTypeEqualityComparer() );")]
    [InlineData("/* 0015 */  partialEquatableRefTypeCollection.[|<AJ0001>ToHashSet|]( null );")]

    [InlineData("/* 0020 */  fullEquatableRefTypeCollection.ToHashSet();")]
    [InlineData("/* 0021 */  fullEquatableRefTypeCollection.ToHashSet( fullEquatableRefTypeEqualityComparer );")]
    [InlineData("/* 0022 */  fullEquatableRefTypeCollection.ToHashSet( new FullEquatableRefTypeEqualityComparer() );")]
    [InlineData("/* 0023 */  fullEquatableRefTypeCollection.ToHashSet( FullEquatableRefType.EqualityComparers.Default );")]
    [InlineData("/* 0024 */  fullEquatableRefTypeCollection.ToHashSet( GetFullEquatableRefTypeEqualityComparer() );")]
    [InlineData("/* 0025 */  fullEquatableRefTypeCollection.ToHashSet( null );")]

    [InlineData("/* 0030 */  valueTypeCollection.ToHashSet();")]
    public async Task AnalyzeTheory(string insertionCode)
    {
        /*
        var code = CreateCode(insertionCode);

        DiagnosticResult[] expected = startCharIndex < 0 || endCharIndex < 0
        ? []
        : [CreateExpectedDiagnostic(MissingEqualityComparerAnalyzer.DiagnosticRules.MissingEqualityComparer.DiagnosticId, 19, startCharIndex, 19, endCharIndex)];

        await CreateAnalyzerTest(code, expected).RunAsync();
        */
        var code = CreateCode(insertionCode);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    private static string CreateCode(string insertionCode)
    {
        return $$"""
                using System;
                using System.Collections;
                using System.Collections.Generic;
                using System.Linq;
                // placeholder
                // placeholder
                // placeholder
                // placeholder
                // placeholder
                // placeholder
                // placeholder
               
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
                
                //////////////////////////////////
                
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

                //////////////////////////////////
                

                """;
    }
}
