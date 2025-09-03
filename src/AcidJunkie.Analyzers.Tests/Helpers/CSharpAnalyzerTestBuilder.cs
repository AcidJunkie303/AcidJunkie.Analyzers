using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace AcidJunkie.Analyzers.Tests.Helpers;

internal static class CSharpAnalyzerTestBuilder
{
    public static CSharpAnalyzerTestBuilder<TAnalyzer> Create<TAnalyzer>(ITestOutputHelper testOutputHelper)
        where TAnalyzer : DiagnosticAnalyzer, new()
        => new(testOutputHelper);
}

internal sealed class CSharpAnalyzerTestBuilder<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private const string EditorConfigHeader = """
                                              root = true

                                              [*.cs]

                                              """;

    private readonly List<string> _additionalEditorConfigLines = [];
    private readonly List<PackageIdentity> _additionalPackages = [];
    private readonly List<Type> _additionalTypes = [];
    private readonly ITestOutputHelper _testOutputHelper;
    private string? _code;
    private string? _enabledOrDisabledDiagnosticId;
    private bool? _isEnabled;

    public CSharpAnalyzerTestBuilder(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithTestCode(string code)
    {
        _code = code;
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithNugetPackage(string packageName, string packageVersion)
    {
        var package = new PackageIdentity(packageName, packageVersion);
        _additionalPackages.Add(package);
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> SetEnabled(bool isEnabled, string diagnosticId)
    {
        _isEnabled = isEnabled;
        _enabledOrDisabledDiagnosticId = diagnosticId;
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithAdditionalReference<T>()
    {
        _additionalTypes.Add(typeof(T));
        return this;
    }

    public CSharpAnalyzerTestBuilder<TAnalyzer> WithEditorConfigLine(string optionsLine)
    {
        _additionalEditorConfigLines.Add(optionsLine);
        return this;
    }

    public CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> Build()
    {
        if (_code.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("No code added!");
        }

        LogSyntaxTree(_code);

        var analyzerTest = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources =
                {
                    _code
                },
#if NET9_0
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages([.._additionalPackages]),
#elif NET8_0
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([.._additionalPackages]),
#else
                .NET framework not handled!
#endif
            }
        };

        foreach (var additionalType in _additionalTypes)
        {
            var reference = MetadataReference.CreateFromFile(additionalType.Assembly.Location);
            analyzerTest.TestState.AdditionalReferences.Add(reference);
        }

        var additionalEditorConfigLines = GetAdditionalConfigurationLines().ToList();
        if (additionalEditorConfigLines.Count > 0)
        {
            var content = EditorConfigHeader + string.Join(Environment.NewLine, additionalEditorConfigLines);
            analyzerTest.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", content));
        }

        return analyzerTest;
    }

    private IEnumerable<string> GetAdditionalConfigurationLines()
    {
        foreach (var line in _additionalEditorConfigLines)
        {
            yield return line;
        }

        if (_isEnabled == null)
        {
            yield break;
        }

        yield return $"{_enabledOrDisabledDiagnosticId}.is_enabled = {(_isEnabled.Value ? "true" : "false")}";
    }

    private void LogSyntaxTree(string code)
    {
        TestFileMarkupParser.GetSpans(code, out var markupFreeCode, out ImmutableArray<TextSpan> _);
        var tree = CSharpSyntaxTree.ParseText(markupFreeCode);
        var root = tree.GetCompilationUnitRoot();
        var hierarchy = SyntaxTreeVisualizer.VisualizeHierarchy(root);
        _testOutputHelper.WriteLine(hierarchy);
    }
}
