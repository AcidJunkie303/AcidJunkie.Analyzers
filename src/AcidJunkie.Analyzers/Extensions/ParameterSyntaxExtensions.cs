using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

public static class ParameterSyntaxExtensions
{
    public static bool IsParams(this ParameterSyntax param) => param.Modifiers.Any(SyntaxKind.ParamsKeyword);
    public static bool HasThisKeyword(this ParameterSyntax param) => param.Modifiers.Any(SyntaxKind.ThisKeyword);
}
