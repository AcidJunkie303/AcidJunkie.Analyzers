using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Tests.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Data.SqlClient;

namespace AcidJunkie.Analyzers.Tests.Helpers;

public static class CSharpAnalyzerTestBuilder
{
    public static CSharpAnalyzerTestBuilder<TAnalyzer> Create<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new()
        => new();
}

public sealed class CSharpAnalyzerTestBuilder<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private string? _code;
    private readonly List<Type> _additionalTypes = [];
    private readonly List<string> _additionalGlobalOptionsLines = [];

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithTestCode(string code)
    {
        _code = code;
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithAdditionalReference<T>()
    {
        _additionalTypes.Add(typeof(T));
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithGlobalOptions(string optionsLine)
    {
        _additionalGlobalOptionsLines.Add(optionsLine);
        return this;
    }

    public CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> Build()
    {
        if (_code.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("No code added!");
        }

        var analyzerTest = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources = {_code},
#if NET8_0
                ReferenceAssemblies = Net.Assemblies.Net80,
#elif NET6_0
                ReferenceAssemblies = Net.Assemblies.Net60,
#else
                .NET framework not handled!
#endif

                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(SqlDataReader).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(TestBase<>).Assembly.Location)
                },
            },
        };

        foreach (var additionalType in _additionalTypes)
        {
            var reference = MetadataReference.CreateFromFile(additionalType.Assembly.Location);
            analyzerTest.TestState.AdditionalReferences.Add(reference);
        }

        if (_additionalGlobalOptionsLines.Count > 0)
        {
            var content = string.Join(Environment.NewLine, _additionalGlobalOptionsLines);
            analyzerTest.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", content));
        }

        return analyzerTest;
    }
}
