using Microsoft.CodeAnalysis;

namespace AcidJunkie.Analyzers.Extensions;

public static class NamedTypeSymbolExtensions
{
    public static bool IsTypeOrIsInheritedFrom(this INamedTypeSymbol namedTypeSymbol, Compilation compilation, string fullTypeName)
    {
        var type = compilation.GetTypeByMetadataName(fullTypeName);
        return type is not null && namedTypeSymbol.IsTypeOrIsInheritedFrom(type);
    }

    public static bool IsTypeOrIsInheritedFrom(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol type)
    {
        var current = namedTypeSymbol;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.ConstructedFrom, type))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
