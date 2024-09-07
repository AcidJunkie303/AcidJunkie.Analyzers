using AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;
using Microsoft.CodeAnalysis.Testing;

namespace AcidJunkie.Analyzers.Tests.Diagnosers.MissingEqualityComparer;

public class MissingEqualityComparerAnalyzerTests : TestBase<MissingEqualityComparerAnalyzer>
{
    [Theory]
    [InlineData(21, 54, "/* 0000 */  new RefType[] { new() }.ToHashSet();")]
    [InlineData(-1, -1, "/* 0000 */  new RefType[] { new() }.ToHashSet(new RefTypeEqualityComparer() );")]
    public async Task AnalyzeTheory(int startCharIndex, int endCharIndex, string insertionCode)
    {
        var code = CreateCode(insertionCode);

        DiagnosticResult[] expected = startCharIndex < 0 || endCharIndex < 0
        ? []
        : [CreateExpectedDiagnostic(MissingEqualityComparerAnalyzer.DiagnosticRules.MissingEqualityComparer.DiagnosticId, 19, startCharIndex, 19, endCharIndex)];

        await CreateAnalyzerTest(code, expected).RunAsync();
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
                        {{insertionCode}}
                    }
                }

                public sealed class RefType
                {
                    public string StringValue { get; set; }
                    public int IntValue { get; set; }
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
                
                """;
    }
}
