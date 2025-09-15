using System.Collections.Immutable;
using System.Composition;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseIsForNullComparisonFixProvider))]
[Shared]
public class UseIsForNullComparisonFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("AJ0011");

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                .ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root.FindNode(diagnosticSpan) is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        var title = GetCodeFixTitle(binaryExpression.OperatorToken.Text);
        if (title.IsNullOrWhiteSpace())
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => ChangeToReferenceNullCheckAsync(context.Document, binaryExpression, c),
                equivalenceKey: "ChangeToReferenceNullCheck"),
            diagnostic);

        static string? GetCodeFixTitle(string operatorContents)
            => operatorContents switch
            {
                "==" => "Replace '==' by 'is'",
                "!=" => "Replace '!=' by 'is not'",
                _    => null
            };
    }

    private static async Task<Document> ChangeToReferenceNullCheckAsync(Document document, BinaryExpressionSyntax binaryExpression, CancellationToken cancellationToken)
    {
        var left = binaryExpression.Left;
        var right = binaryExpression.Right;
        ExpressionSyntax newExpr;

        // Determine which operand is null
        var (valueExpression, nullExpression) = IsNullLiteral(left) ? (right, left) : (left, right);

        if (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
        {
            // Replace '== null' with 'is null'
            newExpr = SyntaxFactory.IsPatternExpression(
                                        valueExpression,
                                        SyntaxFactory.ConstantPattern(nullExpression))
                                   .WithTriviaFrom(binaryExpression);
        }
        else if (binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
        {
            // Replace '!= null' with 'is not null'
            // Create the 'not' token with proper spacing
            var notToken = SyntaxFactory.Token(
                SyntaxFactory.TriviaList(SyntaxFactory.Space), // Leading space
                SyntaxKind.NotKeyword,
                SyntaxFactory.TriviaList(SyntaxFactory.Space) // Trailing space
            );

            newExpr = SyntaxFactory.IsPatternExpression(
                                        valueExpression,
                                        SyntaxFactory.UnaryPattern(
                                            notToken,
                                            SyntaxFactory.ConstantPattern(nullExpression)))
                                   .WithTriviaFrom(binaryExpression);
        }
        else
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root.ReplaceNode(binaryExpression, newExpr);
        return document.WithSyntaxRoot(newRoot);
    }

    private static bool IsNullLiteral(ExpressionSyntax expression) => expression.IsKind(SyntaxKind.NullLiteralExpression);
}
