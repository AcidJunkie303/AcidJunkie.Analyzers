using AcidJunkie.Analyzers.Diagnosers;

namespace AcidJunkie.Analyzers.Tests;

public class SqlParameterAnalyzerTests : TestBase<SqlParameterAnalyzer>
{
    [Fact]
    public async Task Analyze_WhenFirstArgumentIsVariable_ThenDiagnose()
    {
        const string code = """

                            using Microsoft.Data.SqlClient;

                            namespace Playground;

                            public static class SqlParameterNameTest
                            {
                                public static void Test()
                                {
                                    var parameterName = "Id";
                                    var sqlParameter = new SqlParameter(parameterName, System.Data.SqlDbType.Int);
                                }
                            }

                            """;
        var expected = new[]
        {
            CreateExpectedDiagnostic(SqlParameterAnalyzer.DiagnosticId, 11, 45, 11, 58)
        };

        await CreateAnalyzerTest(code, expected).RunAsync();
    }

    [Fact]
    public async Task Analyze_WhenFirstArgumentIsNull_ThenDiagnose()
    {
        const string code = """

                            using Microsoft.Data.SqlClient;

                            namespace Playground;

                            public static class SqlParameterNameTest
                            {
                                public static void Test()
                                {
                                    var sqlParameter = new SqlParameter(null, System.Data.SqlDbType.Int);
                                }
                            }

                            """;

        var expected = new[]
        {
            CreateExpectedDiagnostic(SqlParameterAnalyzer.DiagnosticId, 10, 45, 10, 49)
        };

        await CreateAnalyzerTest(code, expected).RunAsync();
    }

    [Fact]
    public async Task Analyze_WhenFirstArgumentIsStringLiteralNotStartingWithAtSign_ThenDiagnose()
    {
        const string code = """

                            using Microsoft.Data.SqlClient;

                            namespace Playground;

                            public static class SqlParameterNameTest
                            {
                                public static void Test()
                                {
                                    var sqlParameter = new SqlParameter("OrderId", System.Data.SqlDbType.Int);
                                }
                            }

                            """;
        var expected = new[]
        {
            CreateExpectedDiagnostic(SqlParameterAnalyzer.DiagnosticId, 10, 45, 10, 54)
        };

        await CreateAnalyzerTest(code, expected).RunAsync();
    }

    [Fact]
    public async Task Analyze_WhenFirstArgumentIsStringLiteralNotStartingWithAtSignAndCapitalChar_ThenDiagnose()
    {
        const string code = """

                            using Microsoft.Data.SqlClient;

                            namespace Playground;

                            public static class SqlParameterNameTest
                            {
                                public static void Test()
                                {
                                    var sqlParameter = new SqlParameter("@orderId", System.Data.SqlDbType.Int);
                                }
                            }

                            """;
        var expected = new[]
        {
            CreateExpectedDiagnostic(SqlParameterAnalyzer.DiagnosticId, 10, 45, 10, 55)
        };

        await CreateAnalyzerTest(code, expected).RunAsync();
    }

    [Fact]
    public async Task Analyze_WhenFirstArgumentIsStringLiteralStartingWithAtSignAndCapitalLetter_ThenNoDiagnose()
    {
        const string code = """

                            using Microsoft.Data.SqlClient;

                            namespace Playground;

                            public static class SqlParameterNameTest
                            {
                                public static void Test()
                                {
                                    var sqlParameter = new SqlParameter("@OrderId", System.Data.SqlDbType.Int);
                                }
                            }

                            """;
        await CreateAnalyzerTest(code).RunAsync();
    }
}
