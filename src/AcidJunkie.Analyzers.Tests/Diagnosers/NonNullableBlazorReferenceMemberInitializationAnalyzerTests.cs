using System.Collections.Immutable;
using AcidJunkie.Analyzers.Configuration.Aj0008;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

public sealed partial class NonNullableBlazorReferenceMemberInitializationAnalyzerTests
{
    private static ImmutableArray<MethodKinds> AllMethodKinds => [..Enum.GetValues<MethodKinds>()];

    [Theory]
    [InlineData(Nullability.Enabled, "public string? Property1 {get; set;}")]
    [InlineData(Nullability.Enabled, "public int Property1 {get; set;}")]
    public Task WithNullableEnabled_WithNoNonNullableReferenceTypes(Nullability nullability, string insertionCode)
        => RunTestAsync(nullability, insertionCode, AllMethodKinds);

    [Theory]
    [InlineData(Nullability.Enabled, "/* 0001 */ public string {|AJ0008:Property1|} {get; set;}")]
    [InlineData(Nullability.Enabled, "/* 0002 */ public string {|AJ0008:Property1|} {get; set;} = null!;")]
    [InlineData(Nullability.Enabled, "/* 0003 */ public string Property1 {get; set;} = string.Empty;")]
    [InlineData(Nullability.Disabled, "/* 0011 */ public string Property1 {get; set;}")]
    [InlineData(Nullability.Disabled, "/* 0012 */ public string Property1 {get; set;} = null!;")]
    [InlineData(Nullability.Disabled, "/* 0013 */ public string Property1 {get; set;} = string.Empty;")]
    [InlineData(Nullability.None, "/* 0021 */ public string Property1 {get; set;}")]
    [InlineData(Nullability.None, "/* 0022 */ public string Property1 {get; set;} = null!;")]
    [InlineData(Nullability.None, "/* 0023 */ public string Property1 {get; set;} = string.Empty;")]
    public Task WithNullableEnabled_WithProperties_WithNonNullableReferenceTypes(Nullability nullability, string insertionCode)
        => RunTestAsync(nullability, insertionCode, AllMethodKinds);

    [Theory]
    [InlineData(Nullability.Enabled, "/* 0101 */ public string {|AJ0008:_field1|};")]
    [InlineData(Nullability.Enabled, "/* 0102 */ public string {|AJ0008:_field1|} = null!;")]
    [InlineData(Nullability.Enabled, "/* 0103 */ public string _field1 = string.Empty;")]
    [InlineData(Nullability.Disabled, "/* 0111 */ public string _field1;")]
    [InlineData(Nullability.Disabled, "/* 0112 */ public string _field1 = null!;")]
    [InlineData(Nullability.Disabled, "/* 0113 */ public string _field1 = string.Empty;")]
    [InlineData(Nullability.None, "/* 0121 */ public string _field1;")]
    [InlineData(Nullability.None, "/* 0122 */ public string _field1 = null!;")]
    [InlineData(Nullability.None, "/* 0123 */ public string _field1 = string.Empty;")]
    public Task WithNullableEnabled_WithFields_WithNonNullableReferenceTypes(Nullability nullability, string insertionCode)
        => RunTestAsync(nullability, insertionCode, AllMethodKinds);

    [Fact]
    public Task WhenInjectedService_ThenOk()
    {
        const string code = "[global::Microsoft.AspNetCore.Components.InjectAttribute] private string MyString {get;set;} = null;";

        return RunTestAsync(Nullability.Enabled, code, AllMethodKinds);
    }

    // TODO: tests for various configurations (MethodKinds)
}
