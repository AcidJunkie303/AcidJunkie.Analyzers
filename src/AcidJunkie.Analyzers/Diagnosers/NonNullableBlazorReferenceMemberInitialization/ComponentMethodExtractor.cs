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

    public IReadOnlyDictionary<MethodKinds, MethodDeclarationSyntax> Extract(ClassDeclarationSyntax classDeclaration)
    {
        var result = new Dictionary<MethodKinds, MethodDeclarationSyntax>(4);

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
                result.Add(MethodKinds.OnInitialized, method);
            }
            else if (method.Identifier.Text.Equals("OnInitializedAsync", StringComparison.Ordinal) && IsTaskType(method.ReturnType))
            {
                result.Add(MethodKinds.OnInitializedAsync, method);
            }
            else if (method.Identifier.Text.Equals("OnParametersSet", StringComparison.Ordinal) && IsVoidType(method.ReturnType))
            {
                result.Add(MethodKinds.OnParametersSet, method);
            }
            else if (method.Identifier.Text.Equals("OnParametersSetAsync", StringComparison.Ordinal) && IsTaskType(method.ReturnType))
            {
                result.Add(MethodKinds.OnParametersSetAsync, method);
            }
        }

        return result;
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
}
