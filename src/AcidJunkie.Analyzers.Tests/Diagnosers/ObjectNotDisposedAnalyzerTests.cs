using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.ObjectNotDisposed;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S4144:Methods should not have identical implementations", Justification = "Splitted up the test into different methods for different categories")]
[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public class ObjectNotDisposedAnalyzerTests : TestBase<ObjectNotDisposedAnalyzer>
{

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
    public async Task Theory_WhenUsingStatement_ThenOk(string insertionCode)
    {
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningDisposableToField_ThenOk()
    {
        const string insertionCode = "_disposable = GetDisposable();";
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_AndDisposedLater_ThenOk()
    {
        const string insertionCode = """
                                     var disposable = GetDisposable();
                                     disposable.Dispose();
                                     """;

        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_AndDisposedLater2_ThenOk()
    {
        const string insertionCode = """
                                     var disposable = GetDisposable();
                                     
                                     if(111 == 222)
                                     {
                                        Console.WriteLine(disposable);
                                     }
                                     else
                                     {
                                         Console.WriteLine(disposable);
                                     }
                                     
                                     disposable.Dispose();
                                     """;

        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_ButNotDisposedInAllPaths_ThenDiagnose()
    {
        const string insertionCode = """
                                     var disposable = {|AJ0002:GetDisposable()|};
                                     if (111 == 222)
                                     {
                                        disposable.Dispose();
                                     }
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_DisposedOrReturnedInAllPaths_ThenO()
    {
        const string insertionCode = """
                                     var disposable = GetDisposable();
                                     if (111 == 222)
                                     {
                                        disposable.Dispose();
                                     }
                                     else
                                     {
                                        return disposable;
                                     }
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenDisposedInAllSwitchArms_ThenOk()
    {
        const string insertionCode = """
                                     var disposable = GetDisposable();

                                     switch(DateTime.UtcNow.Minute)
                                     {
                                        case 0:
                                            disposable.Dispose();
                                            break;
                                     
                                        case 1:
                                            disposable.Dispose();
                                            break;
                                         
                                        default:
                                            disposable.Dispose();
                                            break;
                                     }

                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenNotDisposedInAllSwitchArms_ThenDiagnose()
    {
        const string insertionCode = """
                                     var disposable = {|AJ0002:GetDisposable()|};
                                     
                                     switch(DateTime.UtcNow.Minute)
                                     {
                                        case 0:
                                            break;
                                     
                                        case 1:
                                            disposable.Dispose();
                                            break;
                                         
                                         default:
                                            break;
                                     }
                                     
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenSwitchWithoutDefaultPath_Then()
    {
        const string insertionCode = """
                                     var disposable = {|AJ0002:GetDisposable()|};

                                     switch(DateTime.UtcNow.Minute)
                                     {
                                        case 0:
                                            disposable.Dispose();
                                            break;
                                     }

                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_WhenDisposedInForLoop_ThenDiagnose()
    {
        const string insertionCode = """
                                     var disposable = {|AJ0002:GetDisposable()|};
                                     for (var i = 0 ; i < 10 ; i++)
                                     {
                                        disposable.Dispose(); // no guarantee that this is executed without checking the for-loop conditions
                                     }
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_WhenDoWhileLoop_ThenOk()
    {
        const string insertionCode = """
                                     var disposable = GetDisposable();
                                     do 
                                     {
                                        disposable.Dispose(); // in do-while, it runs at least once
                                     } while(111 == 222);
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_WhenWhileLoop_ThenDiagnose()
    {
        const string insertionCode = """
                                     var disposable = {|AJ0002:GetDisposable()|};
                                     while(111 == 222)
                                     {
                                        disposable.Dispose(); // in while-do, this might never be called
                                     };
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Theory]
    [InlineData("true")]
    [InlineData("1 == 1")]
    [InlineData(@"""a"" == ""a""")]
    public async Task? WhenAssigningToVariable_WhenInWhileLoopWithSimpleAlwaysTrueCondition1_ThenOk(string conditionCode)
    {
        var insertionCode = $$"""
                                     var disposable = GetDisposable();
                                     while({{conditionCode}})
                                     {
                                        disposable.Dispose();
                                     };
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_WhenInWhileLoopWithSimpleAlwaysTrueCondition2_ThenOk()
    {
        const string insertionCode = """
                                     var disposable = GetDisposable();
                                     while(1 == 1)
                                     {
                                        disposable.Dispose();
                                     };
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? WhenAssigningToVariable_WhenForeachLoop_ThenDiagnose()
    {
        const string insertionCode = """
                                     var disposable = {|AJ0002:GetDisposable()|};
                                     foreach(var c in "")
                                     {
                                        disposable.Dispose(); // in foreach-loop, this might never be called
                                     };
                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task? VariousTests()
    {
        // TODO: remove
        const string insertionCode = """
                                     
                                     using(var bb = new OuterDisposableFactory().GetInnerFactory().GetDisposable())
                                     {
                                     }
                                     
                                     using(new OuterDisposableFactory().GetInnerFactory().GetDisposable())
                                     {
                                     }
                                     
                                     IDisposable? instance;
                                     
                                     {
                                        
                                         _disposable = new OuterDisposableFactory().GetInnerFactory().GetDisposable();                            
                                         instance = new OuterDisposableFactory().GetInnerFactory().GetDisposable();
                                         
                                         try
                                         {
                                            Console.WriteLine();
                                         }
                                         finally
                                         {
                                            instance?.Dispose();
                                         }
                                     }
       

                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();

        // TODO: Check all paths.
        // Statements which have an impact on the execution path:
        // - throw
        // When assigned to variable, find variable declaration and check when the variable goes out of scope, if it is disposed then
    }

    [Fact]
    public async Task WhenNewStyleUsingStatement_ThenOk()
    {
        const string insertionCode = """
                                     
                                     using var aaa = new OuterDisposableFactory().GetInnerFactory().GetDisposable();

                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenOldSchoolUsingStatementWithoutVariable_ThenOk()
    {
        const string insertionCode = """

                                     using (new OuterDisposableFactory().GetInnerFactory().GetDisposable())
                                     {
                                     }

                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenOldSchoolUsingStatementWithVariable_ThenOk()
    {
        const string insertionCode = """

                                     using (var aaa = new OuterDisposableFactory().GetInnerFactory().GetDisposable())
                                     {
                                     }

                                     """;
        var code = CreateTestCode(insertionCode, string.Empty);

        await CreateTesterBuilder()
            .WithTestCode(code)
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

          public static class Test
          {
              private static IDisposable? _disposable;
              
              public static object? ComplexTestMethod()
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
              public IDisposable GetDisposableRefType() => null!;
              public RefStructWithDisposeMethod GetRefStructWithDisposeMethod() => new();
          }

          ////////////////////////////////////////////////////////////////
          // Type definitions
          ////////////////////////////////////////////////////////////////
          public sealed class DisposableRefType : IDisposable
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
