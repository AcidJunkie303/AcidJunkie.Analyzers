using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Diagnosers.TaskCreationWithMaterialisedCollectionAsEnumerable;

namespace AcidJunkie.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzerTests : TestBase<TaskCreationWithMaterialisedCollectionAsEnumerableAnalyzer>
{
    [Fact]
    public async Task WhenCreatingEnumerableTaskWithMaterialisedCollection_ThenDiagnose()
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

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenCreatingEnumerableValueTaskWithMaterialisedCollection_ThenDiagnose()
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
        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }

    [Fact]
    public async Task WhenCreatingTaskOfTypeCollectionWithMaterialisedCollection_ThenOk()
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

        await CreateTesterBuilder()
            .WithTestCode(code)
            .Build()
            .RunAsync();
    }
}
