using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

public static class FieldDeclarationSyntaxExtensions
{
    public static bool IsNonNullableReferenceTypeField(this FieldDeclarationSyntax field, SemanticModel semanticModel)
    {
        // when declaring multiple variables, all variables are of the same type. We pick the first one.
        var variable = field.Declaration.Variables[0];
        if (semanticModel.GetDeclaredSymbol(variable) is not IFieldSymbol fieldSymbol)
        {
            return false;
        }

        var typeSymbol = fieldSymbol.Type;
        if (!typeSymbol.IsReferenceType)
        {
            return false;
        }

        return typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated;
    }
}
