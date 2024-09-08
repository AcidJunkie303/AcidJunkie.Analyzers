using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Tests.Helpers.CodeParsing;
using AcidJunkie.Analyzers.Tests.Runtime;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Data.SqlClient;

namespace AcidJunkie.Analyzers.Tests.Helpers;

public sealed class CSharpAnalyzerTestBuilder
{
    public static CSharpAnalyzerTestBuilder<TAnalyzer> Create<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new()
        => new();
}

public sealed class CSharpAnalyzerTestBuilder<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private string? _code;
    private string? _message;
    private readonly List<ExpectedDiagnostic> _expectedDiagnostics = [];
    private readonly List<Type> _additionalTypes = [];

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithTestCode(string code)
    {
        var parserResult = TaggedSourceCodeParser.Parse(code);

        _expectedDiagnostics.AddRange(parserResult.ExpectedDiagnostics);
        _code = parserResult.PureCode;

        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithAdditionalReference<T>()
    {
        _additionalTypes.Add(typeof(T));
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
#elif NETCOREAPP3_1_OR_GREATER
                ReferenceAssemblies = Net.Assemblies.NetCore31,
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

        AddAdditionalTypes();
        AddDiagnosticResults();

        return analyzerTest;

        void AddAdditionalTypes()
        {
            foreach (var additionalType in _additionalTypes)
            {
                var reference = MetadataReference.CreateFromFile(additionalType.Assembly.Location);
                analyzerTest.TestState.AdditionalReferences.Add(reference);
            }
        }

        void AddDiagnosticResults()
        {
            var analyzer = new TAnalyzer();
            var diagnosticsById = analyzer.SupportedDiagnostics.ToDictionary(a => a.Id, a => a, StringComparer.OrdinalIgnoreCase);

            foreach (var expectedDiagnostic in _expectedDiagnostics)
            {
                var diagnosticDescriptor = diagnosticsById.GetValueOrDefault(expectedDiagnostic.DiagnosticId)
                    ?? throw new InvalidOperationException($"The diagnostic with ID '{expectedDiagnostic.DiagnosticId}' is not known to the analyzer of type '{typeof(TAnalyzer).Name}'!");
                var diagnostic = new DiagnosticResult(diagnosticDescriptor)
                    .WithSpan(expectedDiagnostic.BeginLineIndex, expectedDiagnostic.BeginCharIndex, expectedDiagnostic.EndLineIndex, expectedDiagnostic.EndCharIndex);

                if (_message is not null)
                {
                    diagnostic = diagnostic.WithMessage(_message);
                }

                analyzerTest.ExpectedDiagnostics.Add(diagnostic);
            }
        }
    }
}
