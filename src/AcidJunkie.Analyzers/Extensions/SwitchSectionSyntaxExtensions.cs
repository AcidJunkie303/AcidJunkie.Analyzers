using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Extensions;

public static class SwitchSectionSyntaxExtensions
{
    public static bool IsDefault(this SwitchSectionSyntax section) => section.Labels.Any(a => a.IsKind(SyntaxKind.DefaultSwitchLabel));
}
