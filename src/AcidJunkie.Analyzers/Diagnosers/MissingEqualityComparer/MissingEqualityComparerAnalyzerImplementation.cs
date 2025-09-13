using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Logging;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.MissingEqualityComparer;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class MissingEqualityComparerAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<MissingEqualityComparerAnalyzerImplementation>
{
    private readonly IAnalyzerConfiguration _configuration;

    public MissingEqualityComparerAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeImplicitObjectCreation()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var implicitObjectCreation = (ImplicitObjectCreationExpressionSyntax)Context.Node;

        var typeSymbol = Context.SemanticModel.GetTypeInfo(implicitObjectCreation, Context.CancellationToken).Type;
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        AnalyzeObjectCreationCore(implicitObjectCreation.ArgumentList, implicitObjectCreation.NewKeyword.GetLocation(), namedTypeSymbol);
    }

#if CSHARP_12_OR_GREATER
    public void AnalyzeCollectionExpression()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var collectionExpression = (CollectionExpressionSyntax)Context.Node;

        var typeSymbol = GetAssignmentTargetType(collectionExpression);
        if (typeSymbol is null)
        {
            return;
        }

        var ns = typeSymbol.ContainingNamespace.ToString();
        if (ns.IsNullOrWhiteSpace())
        {
            return;
        }

        var typeParameterName = GenericKeyParameterNameProvider.GetKeyParameterNameForCreation(ns, typeSymbol.Name, typeSymbol.TypeParameters.Length);
        if (typeParameterName is null)
        {
            return;
        }

        AnalyzeObjectCreationCore(null, collectionExpression.GetLocation(), typeSymbol);
    }
#endif

    public void AnalyzeObjectCreation()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var objectCreation = (ObjectCreationExpressionSyntax)Context.Node;

        var typeSymbol = Context.SemanticModel.GetSymbolInfo(objectCreation.Type, Context.CancellationToken).Symbol;
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        var nameNode = objectCreation.ChildNodes().OfType<NameSyntax>().FirstOrDefault();
        var location = nameNode is null
            ? objectCreation.NewKeyword.GetLocation()
            : Context.Node.SyntaxTree.CreateLocationSpan(objectCreation.NewKeyword.GetLocation(), nameNode.GetLocation());

        AnalyzeObjectCreationCore(objectCreation.ArgumentList, location, namedTypeSymbol);
    }

    public void AnalyzeInvocation()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var invocationExpression = (InvocationExpressionSyntax)Context.Node;

        var (owningTypeNameSpace, owningTypeName, methodName, memberAccess) = invocationExpression.GetInvokedMethod(Context.SemanticModel, Context.CancellationToken);
        if (memberAccess is null)
        {
            return;
        }

        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            Logger.WriteLine(LogLevel.Full, $"Unable to determine generic type parameter name for invocation of '{owningTypeNameSpace}.{owningTypeName}.{methodName}'");
            return;
        }

        var keyType = invocationExpression.GetTypeForTypeParameter(Context.SemanticModel, keyTypeParameterName, Context.CancellationToken);
        if (keyType is null)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to determine key type parameter type");
            return;
        }

        if (keyType.IsValueType)
        {
            Logger.WriteLine(LogLevel.Full, $"Key type for '{owningTypeNameSpace}.{owningTypeName}.{methodName}' is {keyType.GetFullName()} which is a struct.");
            return; // Value types use structural comparison
        }

        if (IsCompliantReferenceType(keyType))
        {
            return;
        }

        if (IsAnyParameterEqualityComparer(keyType, invocationExpression.ArgumentList))
        {
            Logger.WriteLine(LogLevel.Full, "Found equality comparer argument");
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, memberAccess.Name.GetLocation());
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, memberAccess.Name.GetLocation()));

        string? GetKeyTypeParameterName() =>
            owningTypeNameSpace is not null && owningTypeName is not null && methodName is not null
                ? GenericKeyParameterNameProvider.GetKeyParameterNameForInvocation(owningTypeNameSpace, owningTypeName, methodName)
                : null;
    }

    private bool IsCompliantReferenceType(ITypeSymbol keyType)
    {
        if (!keyType.IsGetHashCodeOverridden())
        {
            Logger.WriteLine(LogLevel.Full, $"Key type {keyType.GetFullName()} does not override {nameof(GetHashCode)}()");
            return false;
        }

        if (!keyType.IsEqualsOverridden() && !keyType.IsSpecificEqualsOverridden())
        {
            Logger.WriteLine(LogLevel.Full, $"Key type {keyType.GetFullName()} does not override {nameof(Equals)}()");
            return false;
        }

        return true;
    }

    private bool IsAnyParameterEqualityComparer(ITypeSymbol keyType, ArgumentListSyntax? argumentList)
    {
        if (argumentList is null)
        {
            return false;
        }

        foreach (var argument in argumentList.Arguments)
        {
            var argumentType = Context.SemanticModel.GetTypeInfo(argument.Expression, Context.CancellationToken).Type;
            if (argumentType is null)
            {
                continue;
            }

            if (argumentType.ImplementsOrIsInterface("System.Collections.Generic", "IEqualityComparer", keyType))
            {
                return true;
            }
        }

        return false;
    }

    private void AnalyzeObjectCreationCore(ArgumentListSyntax? argumentList, Location locationToReport, INamedTypeSymbol objectTypeBeingCreated)
    {
        var keyTypeParameterName = GetKeyTypeParameterName();
        if (keyTypeParameterName is null)
        {
            Logger.WriteLine(LogLevel.Full, $"Unable to determine generic type parameter name for the creation of type '{objectTypeBeingCreated.GetFullName()}'");
            return;
        }

        var keyParameterIndex = objectTypeBeingCreated.TypeParameters.IndexOf(a => a.Name.EqualsOrdinal(keyTypeParameterName));
        if (keyParameterIndex < 0)
        {
            Logger.WriteLine(LogLevel.Full, $"Unable to determine the index of the key type parameter for type {objectTypeBeingCreated.GetFullName()}");
            return;
        }

        if (objectTypeBeingCreated.TypeArguments.Length != objectTypeBeingCreated.TypeParameters.Length)
        {
            Logger.WriteLine(LogLevel.Full, $"Found mismatch between type argument count and type parameter count for {objectTypeBeingCreated.GetFullName()} which should not be the case!");
            return;
        }

        var keyType = objectTypeBeingCreated.TypeArguments[keyParameterIndex];
        if (keyType.IsValueType)
        {
            Logger.WriteLine(LogLevel.Full, $"Key type for {objectTypeBeingCreated.GetFullName()} is {keyType.GetFullName()} which is a struct.");
            return; // Value types use structural comparison
        }

        if (keyType.ImplementsGenericEquatable() && keyType.IsGetHashCodeOverridden())
        {
            Logger.WriteLine(LogLevel.Full, $"Key type {keyType.GetFullName()} does implement IEquatable<{keyType.GetFullName()}> and does override {nameof(GetHashCode)}() as well");
            return;
        }

        if (IsAnyParameterEqualityComparer(keyType, argumentList))
        {
            Logger.WriteLine(LogLevel.Full, "No parameter is a equality comparer");
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, locationToReport);
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, locationToReport));

        string? GetKeyTypeParameterName()
        {
            var ns = objectTypeBeingCreated.ContainingNamespace?.ToString() ?? string.Empty;

            return GenericKeyParameterNameProvider.GetKeyParameterNameForCreation(ns, objectTypeBeingCreated.Name, objectTypeBeingCreated.TypeParameters.Length);
        }
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Predictability";
            public const string DiagnosticId = "AJ0001";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Provide an equality comparer argument";
            public static readonly LocalizableString MessageFormat = "To prevent unexpected results, use a IEqualityComparer argument because the type used for hash-matching does not fully implement IEquatable<T> together with GetHashCode()";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }

#if CSHARP_12_OR_GREATER
    private INamedTypeSymbol? GetAssignmentTargetType(CollectionExpressionSyntax collectionExpression)
    {
        if (collectionExpression.Parent is EqualsValueClauseSyntax equalsValueClause)
        {
            return GetAssignmentTargetTypeSymbolForEqualsNode(equalsValueClause);
        }

        if (collectionExpression.Parent is AssignmentExpressionSyntax assignmentExpression)
        {
            return GetAssignmentTargetTypeSymbolForAssignmentNode(assignmentExpression);
        }

        return null;
    }

    private INamedTypeSymbol? GetAssignmentTargetTypeSymbolForEqualsNode(EqualsValueClauseSyntax equalsValueClause)
    {
        var target = equalsValueClause.Parent;
        if (target is null)
        {
            return null;
        }

        var symbol = Context.SemanticModel.GetDeclaredSymbol(target);
        if (symbol is null)
        {
            return null;
        }

        return GetTypeSymbol<INamedTypeSymbol>(symbol);
    }

    private INamedTypeSymbol? GetAssignmentTargetTypeSymbolForAssignmentNode(AssignmentExpressionSyntax assignmentExpression)
    {
        var symbolInfo = Context.SemanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol;
        if (symbolInfo is null)
        {
            return null;
        }

        return GetTypeSymbol<INamedTypeSymbol>(symbolInfo);
    }

    private static T? GetTypeSymbol<T>(ISymbol? symbol)
        where T : class, ISymbol
        => symbol switch
        {
            ITypeSymbol typeSymbol         => typeSymbol as T,
            IMethodSymbol methodSymbol     => methodSymbol.ReturnType as T,
            IPropertySymbol propertySymbol => propertySymbol.Type as T,
            IFieldSymbol fieldSymbol       => fieldSymbol.Type as T,
            IEventSymbol eventSymbol       => eventSymbol.Type as T,
            ILocalSymbol localSymbol       => localSymbol.Type as T,
            _                              => null
        };

#endif
}
