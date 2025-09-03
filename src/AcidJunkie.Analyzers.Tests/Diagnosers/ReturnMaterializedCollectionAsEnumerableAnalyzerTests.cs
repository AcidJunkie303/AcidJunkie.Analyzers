using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.ReturnMaterializedCollectionAsEnumerable;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S4144:Methods should not have identical implementations", Justification = "Splitted up the test into different methods for different categories")]
[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class ReturnMaterializedCollectionAsEnumerableAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<ReturnMaterializedCollectionAsEnumerableAnalyzer>(testOutputHelper)
{
    [Fact]
    public Task WhenReturningPureEnumerable_ThenOk()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public IEnumerable<int> TestMethod()
                                {
                                    return Enumerable.Range(0, 10);
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenUsingYieldReturn_ThenOk()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public IEnumerable<int> TestMethod()
                                {
                                    yield return 1;
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenReturningCollectionAsEnumerable_ThenOk()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public IEnumerable<int> TestMethod()
                                {
                                    var items = Enumerable.Range(0, 10).ToList();
                                    return items.AsEnumerable();
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenReturningMaterializedCollection_ThenDiagnose()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public IEnumerable<int> TestMethod()
                                {
                                    {|AJ0003:return|} (IEnumerable<int>) Enumerable.Range(0, 10).ToList();
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenReturningMaterializedCollectionThroughLambda_ThenDiagnose()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class Test
                            {
                                public IEnumerable<int> TestMethod()
                                    {|AJ0003:=>|} Enumerable.Range(0, 10).ToList();
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenInterfaceImplementation_WhenReturningMaterializedCollection_ThenOk()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public interface ITest
                            {
                                IEnumerable<int> TestMethod();
                            }

                            public class Test : ITest
                            {
                                public IEnumerable<int> TestMethod()
                                {
                                    var list = Enumerable.Range(0, 10).ToList();
                                    return list; ;
                                }
                            }
                            """;

        return RunTestAsync(code, true);
    }

    [Fact]
    public Task WhenMethodIsOverridden_WhenReturningMaterializedCollection_ThenOk()
    {
        const string code = """
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;

                            namespace Tests;

                            public class TestBase
                            {
                                public virtual IEnumerable<int> TestMethod()
                                {{
                                    return [];
                                }}
                            }

                            public class Test : TestBase
                            {
                                public override IEnumerable<int> TestMethod()
                                {
                                    var list = Enumerable.Range(0, 10).ToList();
                                    return list; ;
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
                  public IEnumerable<int> TestMethod() {|AJ0003:=>|} Enumerable.Range(0, 10).ToList(); // returning materialized collection as IEnumerable is not ok
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
                  public IEnumerable<int> TestMethod() => Enumerable.Range(0, 10); // returning IEnumerable directly is ok
              }
              """;

        return RunTestAsync(code, isEnabled);
    }

    private Task RunTestAsync(string code, bool isEnabled)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .SetEnabled(isEnabled, "AJ0003")
          .Build()
          .RunAsync();
}
