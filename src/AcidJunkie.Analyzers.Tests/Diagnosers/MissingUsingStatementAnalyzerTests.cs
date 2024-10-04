using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Diagnosers.MissingUsingStatement;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S4144:Methods should not have identical implementations", Justification = "Splitted up the test into different methods for different categories")]
[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class MissingUsingStatementAnalyzerTests : TestBase<MissingUsingStatementAnalyzer>
{
    static MissingUsingStatementAnalyzerTests()
    {
        CachedConfigurationProvider.IsCachingEnabled = false;
    }

    [Theory]
    [InlineData("/* 0000 */  using var a = new Factory().GetDisposable();")] // instance method
    [InlineData("/* 0001 */  using var a = new Factory().GetDisposableRefType();")] // instance method
    [InlineData("/* 0002 */  using var a = new Factory().GetRefStructWithDisposeMethod();")] // instance method
    [InlineData("/* 0003 */  using var a = Factory.GetDisposable_StaticMethod();")] // static method
    [InlineData("/* 0004 */  using var a = Factory.GetDisposableRefType_StaticMethod();")] // static method
    [InlineData("/* 0005 */  using var a = Factory.GetRefStructWithDisposeMethod_StaticMethod();")] // static method
    [InlineData("/* 0006 */  using var a = GetDisposableRefType();")] // local static
    [InlineData("/* 0007 */  using var a = GetDisposable();")] // local static
    [InlineData("/* 0008 */  using var a = GetRefStructWithDisposeMethod();")] // local static
    [InlineData("/* 0009 */  using var a = new OuterDisposableFactory().GetInnerFactory().GetDisposable();")] // cascaded
    [InlineData("/* 0100 */  using (var a = new Factory().GetDisposable()){};")] // instance method
    [InlineData("/* 0101 */  using (var a = new Factory().GetDisposableRefType()){};")] // instance method
    [InlineData("/* 0102 */  using (var a = new Factory().GetRefStructWithDisposeMethod()){};")] // instance method
    [InlineData("/* 0103 */  using (var a = Factory.GetDisposable_StaticMethod()){};")] // static method
    [InlineData("/* 0104 */  using (var a = Factory.GetDisposableRefType_StaticMethod()){};")] // static method
    [InlineData("/* 0105 */  using (var a = Factory.GetRefStructWithDisposeMethod_StaticMethod()){};")] // static method
    [InlineData("/* 0106 */  using (var a = GetDisposableRefType()){};")] // local static
    [InlineData("/* 0107 */  using (var a = GetDisposable()){};")] // local static
    [InlineData("/* 0108 */  using (var a = GetRefStructWithDisposeMethod()){};")] // local static
    [InlineData("/* 0109 */  using (var a = new OuterDisposableFactory().GetInnerFactory().GetDisposable()){};")] // cascaded
    [InlineData("/* 9000 */  using var a = new Factory().GetDisposable();")] // instance method
    [InlineData("/* 9001 */  var a = {|AJ0002:new Factory().GetDisposableRefType()|};")] // instance method
    [InlineData("/* 9002 */  var a = {|AJ0002:new Factory().GetRefStructWithDisposeMethod()|};")] // instance method
    [InlineData("/* 9003 */  var a = {|AJ0002:Factory.GetDisposable_StaticMethod()|};")] // static method
    [InlineData("/* 9004 */  var a = {|AJ0002:Factory.GetDisposableRefType_StaticMethod()|};")] // static method
    [InlineData("/* 9005 */  var a = {|AJ0002:Factory.GetRefStructWithDisposeMethod_StaticMethod()|};")] // static method
    [InlineData("/* 9006 */  var a = {|AJ0002:GetDisposableRefType()|};")] // local static
    [InlineData("/* 9007 */  var a = {|AJ0002:GetDisposable()|};")] // local static
    [InlineData("/* 9008 */  var a = {|AJ0002:GetRefStructWithDisposeMethod()|};")] // local static
    [InlineData("/* 9009 */  var a = {|AJ0002:new OuterDisposableFactory().GetInnerFactory().GetDisposable()|};")] // cascaded
    [InlineData("/* 9100 */  var a = {|AJ0002:new Factory().GetDisposable()|};")] // instance method
    [InlineData("/* 9101 */  var a = {|AJ0002:new Factory().GetDisposableRefType()|};")] // instance method
    [InlineData("/* 9102 */  var a = {|AJ0002:new Factory().GetRefStructWithDisposeMethod()|};")] // instance method
    [InlineData("/* 9103 */  var a = {|AJ0002:Factory.GetDisposable_StaticMethod()|};")] // static method
    [InlineData("/* 9104 */  var a = {|AJ0002:Factory.GetDisposableRefType_StaticMethod()|};")] // static method
    [InlineData("/* 9105 */  var a = {|AJ0002:Factory.GetRefStructWithDisposeMethod_StaticMethod()|};")] // static method
    [InlineData("/* 9106 */  var a = {|AJ0002:GetDisposableRefType()|};")] // local static
    [InlineData("/* 9107 */  var a = {|AJ0002:GetDisposable()|};")] // local static
    [InlineData("/* 9108 */  var a = {|AJ0002:GetRefStructWithDisposeMethod()|};")] // local static
    [InlineData("/* 9109 */  var a = {|AJ0002:new OuterDisposableFactory().GetInnerFactory().GetDisposable()|};")] // cascaded

    public async Task Theory_EnsureUsingStatementIsUsed(string insertionCode)
    {
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenStoringDisposableObjectInField_ThenOk()
    {
        const string insertionCode = "_disposable = (IDisposable) GetDisposable();";
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .WithGlobalOptions($"{Aj0002Configuration.KeyNames.IgnoredObjectNames} = bla")
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenStoringDisposableObjectInProperty_ThenOk()
    {
        const string insertionCode = "Disposable = (IDisposable) GetDisposable();";
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .WithGlobalOptions($"{Aj0002Configuration.KeyNames.IgnoredObjectNames} = bla")
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenMethodIsIgnored_ThenOk()
    {
        const string insertionCode = "new Factory<int>().GetDisposable<string>();";
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .WithGlobalOptions($"{Aj0002Configuration.KeyNames.IgnoredObjectNames} = Tests.Factory`1.GetDisposable")
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenReturnedTypeIsIgnored_ThenOk()
    {
        const string insertionCode = "var disposable = (IDisposable) new Factory<int>().GetDisposableRefType();";
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .WithGlobalOptions($"{Aj0002Configuration.KeyNames.IgnoredObjectNames} = Tests.DisposableRefType")
            .Build()
            .RunAsync();
    }

    private static string CreateTestCode(string simpleInsertionCode, string complexInsertionCode) =>
        $$"""
          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Threading.Tasks;

          namespace Tests;

          public class Test
          {
              private IDisposable? _disposable;
              private IDisposable? Disposable {get; set;}
                  
              public object? ComplexTestMethod()
              {
                  {{simpleInsertionCode}}
                  
                  return null; // fallback. Some testing methods might return something
              }
              
              {{complexInsertionCode}}
              
              private static DisposableRefType GetDisposableRefType() => null!;
              private static IDisposable GetDisposable() => null!;
              private static RefStructWithDisposeMethod GetRefStructWithDisposeMethod() => new();
          }
          
          public sealed class Factory
          {
              public static IDisposable GetDisposable_StaticMethod() => null!;
              public static IDisposable GetDisposableRefType_StaticMethod() => null!;
              public static RefStructWithDisposeMethod GetRefStructWithDisposeMethod_StaticMethod() => new();
              public IDisposable GetDisposable() => null!;
              public IDisposable GetDisposable<TT>() => null!;    
              public IDisposable GetDisposableRefType() => null!;
              public RefStructWithDisposeMethod GetRefStructWithDisposeMethod() => new();
          }
          
          public sealed class Factory<T>
          {
              public static DisposableRefType GetDisposable_StaticMethod() => null!;
              public static DisposableRefType GetDisposableRefType_StaticMethod() => null!;
              public static RefStructWithDisposeMethod GetRefStructWithDisposeMethod_StaticMethod() => new();
              public DisposableRefType GetDisposable() => null!;
              public DisposableRefType<TT> GetDisposable<TT>() => null!;
              public DisposableRefType GetDisposableRefType() => null!;
              public RefStructWithDisposeMethod GetRefStructWithDisposeMethod() => new();
          }

          ////////////////////////////////////////////////////////////////
          // Type definitions
          ////////////////////////////////////////////////////////////////
          public sealed class DisposableRefType : IDisposable
          {
              public void Dispose(){}
          }
          
          public sealed class DisposableRefType<T> : IDisposable
          {
              public void Dispose(){}
          }
          
          public ref struct RefStructWithDisposeMethod
          {
              public void Dispose(){}
          }
          
          public sealed class OuterDisposableFactory
          {
              public InnerDisposableFactory GetInnerFactory() => new ();
          }

          public sealed class InnerDisposableFactory
          {
              public IDisposable GetDisposable() => null!; 
          }
          """;
}
