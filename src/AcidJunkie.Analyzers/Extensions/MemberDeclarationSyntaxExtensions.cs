using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Extensions;

internal static class MemberDeclarationSyntaxExtensions
{
    public static BaseTypeDeclarationSyntax? GetContainingTypeDeclaration(this MemberDeclarationSyntax memberDeclaration)
    {
        var parent = memberDeclaration.Parent;

        while (parent is not null and not BaseTypeDeclarationSyntax)
        {
            parent = parent.Parent;
        }

        return parent as BaseTypeDeclarationSyntax;
    }

    public static INamedTypeSymbol? GetContainingType(this MemberDeclarationSyntax memberDeclaration, in SyntaxNodeAnalysisContext context) => memberDeclaration.GetContainingType(context.SemanticModel);

    public static INamedTypeSymbol? GetContainingType(this MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel)
    {
        var typeDeclaration = memberDeclaration.GetContainingTypeDeclaration();
        if (typeDeclaration is null)
        {
            return null;
        }

        return ModelExtensions.GetDeclaredSymbol(semanticModel, typeDeclaration) as INamedTypeSymbol;
    }

    public static bool IsStatic(this MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.StaticKeyword);
}
