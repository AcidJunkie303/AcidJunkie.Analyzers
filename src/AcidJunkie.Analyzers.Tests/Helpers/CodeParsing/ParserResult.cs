namespace AcidJunkie.Analyzers.Tests.Helpers.CodeParsing;

internal sealed record ParserResult(string PureCode, IReadOnlyList<ExpectedDiagnostic> ExpectedDiagnostics);
