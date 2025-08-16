using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// TODO: remove
#pragma warning disable

namespace AcidJunkie.Analyzers.Diagnosers.EnforceEntityFrameworkTrackingType;

internal sealed class EnforceEntityFrameworkTrackingTypeAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<EnforceEntityFrameworkTrackingTypeAnalyzer>
{
    public EnforceEntityFrameworkTrackingTypeAnalyzerImplementation(SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeMemberAccessExpression()
    {
        var memberAccessExpression = (MemberAccessExpressionSyntax)Context.Node;
        if (!IsDbSetType(memberAccessExpression, out var entityType))
        {
            return;
        }

        return;
    }

    private bool IsDbSetType(MemberAccessExpressionSyntax memberAccessExpression, [NotNullWhen(true)] out INamedTypeSymbol? entityType)
    {
        entityType = null;

        if (Context.SemanticModel.GetTypeInfo(memberAccessExpression).Type is not INamedTypeSymbol memberType)
        {
            return false;
        }

        if (!memberType.IsTypeOrIsInheritedFrom(Context.Compilation, "Microsoft.EntityFrameworkCore.DbSet`1"))
        {
            return false;
        }

        entityType = memberType.TypeArguments[0] as INamedTypeSymbol;
        return entityType is not null;
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> AllRules { get; } = [Default.Rule];

        internal static class Default
        {
            private const string Category = "Intention";
            public const string DiagnosticId = "AJ0002";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0002.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Specify AsTracking or AsNoTracking when querying entity framework";
            public static readonly LocalizableString MessageFormat = "Specify AsTracking or AsNoTracking when querying entity framework";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
