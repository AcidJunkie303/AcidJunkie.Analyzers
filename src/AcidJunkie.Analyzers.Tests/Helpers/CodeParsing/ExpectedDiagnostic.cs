namespace AcidJunkie.Analyzers.Tests.Helpers.CodeParsing;

internal sealed record ExpectedDiagnostic
(
    string DiagnosticId,
    int BeginLineIndex,
    int BeginCharIndex,
    int EndLineIndex,
    int EndCharIndex
);
