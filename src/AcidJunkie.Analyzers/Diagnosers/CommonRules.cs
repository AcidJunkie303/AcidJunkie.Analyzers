using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Diagnosers;
public static class CommonRules
{
    public static class UnhandledError
    {
        private const string Category = "Warning";
        public const string DiagnosticId = "AJ9999";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
        public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ9999.md";
#pragma warning restore S1075

        public static readonly LocalizableString Title = "The AcidJunkie analyzer package encountered an error";
        public static readonly LocalizableString MessageFormat = "An error occurred in the AcidJunkie.Analyzers package. Check the log file 'AJ.Analyzers.log' in the temp folder.";
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
    }
}
