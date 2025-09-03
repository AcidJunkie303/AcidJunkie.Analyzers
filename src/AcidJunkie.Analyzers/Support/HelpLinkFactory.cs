namespace AcidJunkie.Analyzers.Support;

public static class HelpLinkFactory
{
    public static string CreateForDiagnosticId(string diagnosticId)
        => $"https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/{diagnosticId}.md";
}
