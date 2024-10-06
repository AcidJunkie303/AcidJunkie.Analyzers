using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Diagnosers;
public static class CommonRules
{
    public static class UnhandledError
    {
        private const string Category = "Error";
        public const string DiagnosticId = "AJ9999";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
        public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ9999.md";
#pragma warning restore S1075

        public static readonly LocalizableString Title = "The analyzer package AJ.Analyzers encountered an error";
        public static readonly LocalizableString MessageFormat = Title;
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
    }
}
