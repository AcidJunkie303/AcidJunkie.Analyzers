using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[SuppressMessage("MicrosoftCodeAnalysisDesign", "RS1032:Define diagnostic message correctly", Justification = "Stupid rule...")]
[SuppressMessage("MicrosoftCodeAnalysisDesign", "RS1033:Define diagnostic description correctly", Justification = "Stupid rule...")]
public sealed class SqlParameterAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";
    public const string DiagnosticId = "BJZZ0003";

    private static readonly LocalizableString Title = "Sql parameter name enforcement";
    private static readonly LocalizableString MessageFormat = "The first argument of SqlParameter must be a literal string and must start with '@' followed by a capital letter. Example: \"@OderId\"";
    private static readonly LocalizableString Description = "For standardization and easy search in the source code, the first argument of SqlParameter must be a literal string and must start with '@' followed by a capital letter. Example: \"@OderId\"";
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Rule);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ObjectCreationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var node = (ObjectCreationExpressionSyntax)context.Node;

        var fullTypeName = node.GetFullTypeName(context, node.Type);
        if (!fullTypeName.EqualsOrdinal("Microsoft.Data.SqlClient.SqlParameter"))
        {
            return;
        }

        if (node.ArgumentList is null || node.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        var firstArgument = node.ArgumentList.Arguments[0];
        var firstArgumentChild = firstArgument.ChildNodes().First();

        if (firstArgumentChild is not LiteralExpressionSyntax literalExpression)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgumentChild.GetLocation()));
            return;
        }

        var argumentData = literalExpression.ToFullString();
        if (!argumentData.StartsWith("\"@", StringComparison.Ordinal))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgumentChild.GetLocation()));
            return;
        }

        if (argumentData.Length <= 3 || !char.IsLetter(argumentData[2]) || !char.IsUpper(argumentData[2]))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgumentChild.GetLocation()));
        }
    }
}
