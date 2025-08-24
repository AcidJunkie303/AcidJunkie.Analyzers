using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterializedCollectionAsEnumerable;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class TaskCreationWithMaterializedCollectionAsEnumerableAnalyzerTests(ITestOutputHelper testOutputHelper) : TestBase<TaskCreationWithMaterializedCollectionAsEnumerableAnalyzer>(testOutputHelper)
{
    [Fact]
    public Task WhenCreatingEnumerableTaskWithMaterializedCollection_ThenDiagnose()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public void TestMethod()
                                {
                                    var task = {|AJ0004:Task.FromResult|}( (IEnumerable<int>) (List<int>) Enumerable.Range(0, 10).ToList() );
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenCreatingEnumerableValueTaskWithMaterializedCollection_ThenDiagnose()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public void TestMethod()
                                {
                                    var task = {|AJ0004:ValueTask.FromResult|}( (IEnumerable<int>) (List<int>) Enumerable.Range(0, 10).ToList() );
                                }
                            }
                            """;
        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenCreatingTaskOfTypeCollectionWithMaterializedCollection_ThenOk()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public void TestMethod()
                                {
                                    var task = Task.FromResult( (IReadOnlyList<int>) Enumerable.Range(0, 10).ToList() );
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Theory_IsEnabled(bool isEnabled)
    {
        var code = isEnabled
            ? """
              using System;
              using System.Collections.Generic;
              using System.Linq;
              using System.Threading.Tasks;

              namespace Tests;

              public class Test
              {
                  public void TestMethod()
                  {
                      var task = {|AJ0004:ValueTask.FromResult|}( (IEnumerable<int>) (List<int>) Enumerable.Range(0, 10).ToList() ); // returning materialized collection as IEnumerable is not ok
                  }
              }
              """
            : """
              using System;
              using System.Collections.Generic;
              using System.Linq;
              using System.Threading.Tasks;

              namespace Tests;

              public class Test
              {
                  public void TestMethod()
                  {
                      var task = ValueTask.FromResult( (IEnumerable<int>) (List<int>) Enumerable.Range(0, 10).ToList() );
                  }
              }
              """;

        return RunTestAsync(code, isEnabled);
    }

    private static string CreateIsEnabledConfigurationLine(bool isEnabled) => $"AJ0004.is_enabled = {(isEnabled ? "true" : "false")}";

    private Task RunTestAsync(string code, bool isEnabled)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .WithEditorConfigLine(CreateIsEnabledConfigurationLine(isEnabled))
          .Build()
          .RunAsync();
}
