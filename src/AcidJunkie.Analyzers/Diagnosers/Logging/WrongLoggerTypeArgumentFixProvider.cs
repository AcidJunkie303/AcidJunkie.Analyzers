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
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [WrongLoggerTypeArgumentAnalyzerImplementation.DiagnosticRules.Default.DiagnosticId];

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
        if (parameter is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Change ILogger type argument",
                    ct => ChangeLoggerParameterTypeArgument(context.Document, parameter, ct),
                    "Change ILogger type argument"),
                diagnostic);
        }

        var fieldDeclaration = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<FieldDeclarationSyntax>();
        if (fieldDeclaration is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Change ILogger type argument",
                    ct => ChangeLoggerFieldTypeArgument(context.Document, fieldDeclaration, ct),
                    "Change ILogger type argument"),
                diagnostic);
        }

        var propertyDeclaration = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
        if (propertyDeclaration is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Change ILogger type argument",
                    ct => ChangeLoggerPropertyTypeArgument(context.Document, propertyDeclaration, ct),
                    "Change ILogger type argument"),
                diagnostic);
        }
    }

    private static async Task<Document> ChangeLoggerParameterTypeArgument(Document document, ParameterSyntax parameter, CancellationToken cancellationToken)
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

    private static async Task<Document> ChangeLoggerFieldTypeArgument(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        var parentTypeDeclaration = fieldDeclaration
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

        if (fieldDeclaration.Declaration.Type is not GenericNameSyntax fieldType)
        {
            return document;
        }

        var newTypeArgument = SyntaxFactory.ParseTypeName(parentTypeName);
        var newTypeArgumentList = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(newTypeArgument));
        var newGenericName = fieldType.WithTypeArgumentList(newTypeArgumentList);
        var newFieldDeclaration = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithType(newGenericName));
        var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ChangeLoggerPropertyTypeArgument(Document document, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        var parentTypeDeclaration = propertyDeclaration
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

        var newType = SyntaxFactory.ParseTypeName($"ILogger<{parentTypeName}>").WithTriviaFrom(propertyDeclaration.Type);
        var newProperty = propertyDeclaration.WithType(newType);
        var newRoot = root.ReplaceNode(propertyDeclaration, newProperty);

        return document.WithSyntaxRoot(newRoot);
    }
}
