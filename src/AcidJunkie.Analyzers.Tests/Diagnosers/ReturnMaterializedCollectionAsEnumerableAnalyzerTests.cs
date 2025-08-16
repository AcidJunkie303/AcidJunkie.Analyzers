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
    public async Task WhenReturningPureEnumerable_ThenOk()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public async Task WhenUsingYieldReturn_ThenOk()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public async Task WhenReturningCollectionAsEnumerable_ThenOk()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public async Task WhenReturningMaterializedCollection_ThenDiagnose()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public async Task WhenReturningMaterializedCollectionThroughLambda_ThenDiagnose()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public async Task WhenInterfaceImplementation_WhenReturningMaterializedCollection_ThenOk()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }

    [Fact]
    public async Task WhenMethodIsOverridden_WhenReturningMaterializedCollection_ThenOk()
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

        await CreateTesterBuilder()
             .WithTestCode(code)
             .Build()
             .RunAsync();
    }
}
