using System.Collections.Immutable;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;

namespace AcidJunkie.Analyzers.Diagnosers.Logging;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WrongLoggerTypeArgumentFixProvider))]
public sealed class WrongLoggerTypeArgumentFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [WrongLoggerTypeArgumentAnalyzer.DiagnosticRules.Default.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var parameter = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<ParameterSyntax>();
        if (parameter is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Change ILogger type argument",
                ct => ChangeLoggerTypeArgument(context.Document, parameter, ct),
                "Change ILogger type argument"),
            diagnostic);
    }

    private static async Task<Document> ChangeLoggerTypeArgument(Document document, ParameterSyntax parameter, CancellationToken cancellationToken)
    {
        var parentTypeDeclaration = parameter
            .GetParents()
            .FirstOrDefault(static a => a is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax);

        if (parentTypeDeclaration is null)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document;
        }

        var parentTypeName = semanticModel.GetDeclaredSymbol(parentTypeDeclaration, cancellationToken)?.Name;
        if (parentTypeName is null)
        {
            return document;
        }

        if (parameter.Type is null)
        {
            return document;
        }

        var newType = SyntaxFactory.ParseTypeName($"ILogger<{parentTypeName}>").WithTriviaFrom(parameter.Type);
        var newParameter = parameter.WithType(newType);
        var newRoot = root.ReplaceNode(parameter, newParameter);

        return document.WithSyntaxRoot(newRoot);
    }
}
