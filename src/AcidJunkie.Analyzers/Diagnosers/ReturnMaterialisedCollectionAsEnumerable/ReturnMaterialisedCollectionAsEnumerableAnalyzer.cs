using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.ReturnMaterialisedCollectionAsEnumerable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnMaterialisedCollectionAsEnumerableAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = [CommonRules.UnhandledError.Rule, DiagnosticRules.Default.Rule];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeAction(AnalyzeReturn, SyntaxKind.ReturnStatement);
    }

    private static void AnalyzeReturn(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;

        if (returnStatement.Expression is null)
        {
            return;
        }

        if (!DoesMethodReturnEnumerable(context, returnStatement))
        {
            return;
        }

        var realReturnExpression = returnStatement.Expression.GetFirstNonCastExpression();
        var returnType = context.SemanticModel.GetTypeInfo(realReturnExpression).Type;
        if (returnType is null)
        {
            return;
        }

        if (!returnType.DoesImplementWellKnownCollectionInterface())
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, returnStatement.ReturnKeyword.GetLocation()));
    }

    private static bool DoesMethodReturnEnumerable(SyntaxNodeAnalysisContext context, ReturnStatementSyntax returnStatement)
    {
        var firstMatchingParent = returnStatement.GetParents().FirstOrDefault(a => a is MethodDeclarationSyntax or LocalFunctionStatementSyntax or LambdaExpressionSyntax or SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax);
        if (firstMatchingParent is null)
        {
            return false;
        }

        if (firstMatchingParent is LambdaExpressionSyntax or SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax)
        {
            return false;
        }

        var returnTypeSyntax = firstMatchingParent switch
        {
            MethodDeclarationSyntax methodDeclaration => methodDeclaration.ReturnType,
            LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.ReturnType,
            _ => null
        };

        if (returnTypeSyntax is null)
        {
            return false;
        }

        var returnType = context.SemanticModel.GetTypeInfo(returnTypeSyntax).Type;
        if (returnType is null)
        {
            return false;
        }

        return IsEnumerable(returnType);
    }

    private static bool IsEnumerable(ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.ToString() ?? string.Empty;

        var namedTypeSymbol = typeSymbol as INamedTypeSymbol;

        if (typeSymbol.Name.EqualsOrdinal("IEnumerable"))
        {
            if (ns.EqualsOrdinal("System.Collections"))
            {
                return true;
            }

            return namedTypeSymbol is not null && namedTypeSymbol.Arity == 1 && ns.EqualsOrdinal("System.Collections.Generic");
        }

        return false;
    }

    internal static class DiagnosticRules
    {
        internal static class Default
        {
            private const string Category = "Performance";
            public const string DiagnosticId = "AJ0003";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0003.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Do not return materialised collection as enumerable";
            public static readonly LocalizableString MessageFormat = "Do not return materialised collection as enumerable";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);
        }
    }
}
