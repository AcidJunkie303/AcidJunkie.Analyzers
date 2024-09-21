using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers;

namespace AcidJunkie.Analyzers.Tests.Extensions;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class MethodSymbolExtensionsTests : TestBase<PlaygroundAnalyzer>
{
    [Fact]
    public async Task Bla()
    {
        const string code = """
                                     using System;
                                     using System.Collections.Generic;
                                     
                                     public class MyGenericType<T>
                                     {
                                        public KeyValuePair<T, TValue> GetValue<TValue>(T key, TValue value) => new KeyValuePair.Create(key, value); 
                                     }
                                     
                                     public static void Test()
                                     {
                                        new MyGenericType<string>().GetValue("tb", 303);
                                     }
 
                                     """;

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();

    }
}
