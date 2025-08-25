using AcidJunkie.Analyzers.Configuration.Aj0008;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;

internal sealed class ComponentMethodExtractor
{
    private readonly SemanticModel _semanticModel;

    public ComponentMethodExtractor(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public IEnumerable<MethodAndMethodKind> Extract(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            if (method.ParameterList.Parameters.Count > 0)
            {
                continue;
            }

            if (method.Body is null && method.ExpressionBody is null)
            {
                continue;
            }

            if (method.Identifier.Text.Equals("OnInitialized", StringComparison.Ordinal) && IsVoidType(method.ReturnType))
            {
                yield return new MethodAndMethodKind(method, MethodKinds.OnInitialized);
            }
            else if (method.Identifier.Text.Equals("OnInitializedAsync", StringComparison.Ordinal) && IsTaskType(method.ReturnType))
            {
                yield return new MethodAndMethodKind(method, MethodKinds.OnInitializedAsync);
            }
            else if (method.Identifier.Text.Equals("OnParametersSet", StringComparison.Ordinal) && IsVoidType(method.ReturnType))
            {
                yield return new MethodAndMethodKind(method, MethodKinds.OnParametersSet);
            }
            else if (method.Identifier.Text.Equals("OnParametersSetAsync", StringComparison.Ordinal) && IsTaskType(method.ReturnType))
            {
                yield return new MethodAndMethodKind(method, MethodKinds.OnParametersSetAsync);
            }
        }
    }

    private bool IsVoidType(TypeSyntax type)
    {
        var symbol = _semanticModel.GetSymbolInfo(type);
        return string.Equals(symbol.Symbol?.Name, "Void", StringComparison.Ordinal);
    }

    private bool IsTaskType(TypeSyntax type)
    {
        var symbol = _semanticModel.GetTypeInfo(type);
        return string.Equals(symbol.Type?.Name, "Task", StringComparison.Ordinal)
               && string.Equals(symbol.Type?.GetFullNamespace(), "System.Threading.Tasks", StringComparison.Ordinal);
    }

    public sealed class MethodAndMethodKind
    {
        public MethodDeclarationSyntax MethodDeclaration { get; }
        public MethodKinds MethodKind { get; }

        public MethodAndMethodKind(MethodDeclarationSyntax methodDeclaration, MethodKinds methodKind)
        {
            MethodDeclaration = methodDeclaration;
            MethodKind = methodKind;
        }
    }
}
