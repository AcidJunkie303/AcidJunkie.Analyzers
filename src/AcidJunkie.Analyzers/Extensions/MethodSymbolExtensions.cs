using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;
internal static class MethodSymbolExtensions
{
    private static readonly SymbolDisplayFormat SymbolDisplayFormat = new
    (
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None);

    public static string? GetDocumentationLikeId(this IMethodSymbol methodSymbol)
    {
        var formattedName = methodSymbol.ToDisplayString(SymbolDisplayFormat);
        if (methodSymbol.Arity > 0)
        {
            formattedName += '`';
        }

        return formattedName;
    }
}
