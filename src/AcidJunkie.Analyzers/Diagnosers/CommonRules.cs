using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Diagnosers;
public static class CommonRules
{
    public static class UnhandledError
    {
        private const string Category = "Error";
        public const string DiagnosticId = "AJ9999";

        public static readonly LocalizableString Title = "The analyzer package AJ.Analyzers encountered an error";
        public static readonly LocalizableString MessageFormat = Title;
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
    }
}
