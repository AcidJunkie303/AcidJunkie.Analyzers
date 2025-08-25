using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

public static class PropertyDeclarationSyntaxExtensions
{
    public static bool IsNonNullableReferenceTypeProperty(this PropertyDeclarationSyntax property, SemanticModel semanticModel)
    {
        if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
        {
            return false;
        }

        var typeSymbol = propertySymbol.Type;
        if (!typeSymbol.IsReferenceType)
        {
            return false;
        }

        return typeSymbol.NullableAnnotation == NullableAnnotation.Annotated // -> #nullable enabled
               || typeSymbol.NullableAnnotation == NullableAnnotation.None; // -> #nullable disabled
    }
}
