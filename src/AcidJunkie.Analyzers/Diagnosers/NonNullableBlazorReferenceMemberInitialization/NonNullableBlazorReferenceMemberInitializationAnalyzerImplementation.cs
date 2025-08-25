using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0008;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Diagnosers.NonNullableBlazorReferenceMemberInitialization;

// TODO: remove
#pragma warning disable

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation
    : SyntaxNodeAnalyzerImplementationBase<NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation>
{
    private readonly Aj0008Configuration _configuration;

    public NonNullableBlazorReferenceMemberInitializationAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = Aj0008ConfigurationProvider.Instance.GetConfiguration(context);
    }

    public void AnalyzeClassDeclaration()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var classDeclaration = (ClassDeclarationSyntax)Context.Node;

        if (IsBlazorComponent(classDeclaration))
        {
            Logger.WriteLine(() => $"{classDeclaration.Identifier.ValueText} is not a Blazor component. Aborting");
        }

        var methodsToCheck = new ComponentMethodExtractor(Context.SemanticModel)
                            .Extract(classDeclaration)
                            .Where(a => _configuration.MethodsToCheck.Contains(a.MethodKind))
                            .Select(a => a)
                            .ToList();

        Lazy<string> allMethodNamesToCheck = new(() => string.Join(", ", methodsToCheck.Select(a => a.MethodKind)));

        foreach (var (location, memberName) in GetMembersToCheck(classDeclaration))
        {
            var isMemberHandledInAllPaths = methodsToCheck
               .Any(method => AssignmentOrIsNullTestedChecker.IsMemberAssignedOrNullCheckedOnAllExecutionPaths(Context.SemanticModel, method.MethodDeclaration, memberName));
            if (isMemberHandledInAllPaths)
            {
                continue;
            }

            Logger.WriteLine(() => $"Property or field {memberName} of class {classDeclaration.Identifier.ValueText} is not initialized/assigned in all execution paths of any of the following methods: {allMethodNamesToCheck}");
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, location, memberName, allMethodNamesToCheck.Value));
        }
    }

    private static bool IsNullInitialization(ExpressionSyntax expression)
    {
        var current = expression;

        while (true)
        {
            switch (current)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression) || literal.IsKind(SyntaxKind.DefaultLiteralExpression):
                    return true;
                case PostfixUnaryExpressionSyntax postfixUnary:
                    current = postfixUnary.Operand;
                    break;
                default:
                    return false;
            }
        }
    }

    private IEnumerable<LocationAndName> GetMembersToCheck(ClassDeclarationSyntax classDeclaration)
    {
        return [.. GetPropertiesToCheck(), .. GetFieldsToCheck()];

        IEnumerable<LocationAndName> GetPropertiesToCheck()
            => classDeclaration.Members
                               .OfType<PropertyDeclarationSyntax>()
                               .Where(a => !IsInjected(a))
                               .Where(a => a.IsNonNullableReferenceTypeProperty(Context.SemanticModel))
                               .Where(a => a.Initializer is null || HasInitializationWithNullValue(a.Initializer))
                               .Select(a => new LocationAndName(a.Identifier.GetLocation(), a.Identifier.Text));

        IEnumerable<LocationAndName> GetFieldsToCheck()
            => classDeclaration.Members
                               .OfType<FieldDeclarationSyntax>()
                               .Where(a => a.IsNonNullableReferenceTypeField(Context.SemanticModel))
                               .SelectMany(a => a.Declaration.Variables)
                               .Where(a => a.Initializer is null || HasInitializationWithNullValue(a.Initializer))
                               .Select(a => new LocationAndName(a.Identifier.GetLocation(), a.Identifier.Text));
    }

    private bool HasInitializationWithNullValue(EqualsValueClauseSyntax initializer)
    {
        if (IsNullInitialization(initializer.Value))
        {
            return true;
        }

        var typeInfo = ModelExtensions.GetTypeInfo(Context.SemanticModel, initializer.Value);
        if (typeInfo.Type is null)
        {
            return false;
        }

        if (!typeInfo.Type.IsReferenceType)
        {
            return false;
        }

        return typeInfo.Nullability.FlowState == NullableFlowState.MaybeNull;
    }

    private bool IsInjected(PropertyDeclarationSyntax property)
    {
        if (property.AttributeLists.IsNullOrEmpty())
        {
            return false;
        }

        return property.AttributeLists
                       .SelectMany(a => a.Attributes)
                       .Any(IsInjectedAttribute);

        bool IsInjectedAttribute(AttributeSyntax attribute)
        {
            var typeInfo = ModelExtensions.GetTypeInfo(Context.SemanticModel, attribute.Name).Type;
            if (typeInfo is null)
            {
                return false;
            }

            return typeInfo.Name.EqualsOrdinal("InjectAttribute")
                   && typeInfo.GetFullNamespace().EqualsOrdinal("Microsoft.AspNetCore.Components");
        }
    }

    private bool IsBlazorComponent(ClassDeclarationSyntax classDeclaration)
    {
        if (Context.SemanticModel.GetTypeInfo(classDeclaration).Type is not INamedTypeSymbol type)
        {
            return false;
        }

        return type.IsTypeOrIsInheritedFrom(Context.Compilation, "Microsoft.AspNetCore.Components.ComponentBase");
    }

    private sealed class LocationAndName
    {
        public Location Location { get; }
        public string Name { get; }

        public LocationAndName(Location location, string name)
        {
            Location = location;
            Name = name;
        }

        public void Deconstruct(out Location location, out string name)
            => (location, name) = (Location, Name);
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Standardization";
            public const string DiagnosticId = "AJ0008";
#pragma warning disable S1075 // Refactor your code not to use hardcoded absolution paths or URIs
            public const string HelpLinkUri = "https://github.com/AcidJunkie303/AcidJunkie.Analyzers/blob/main/docs/Rules/AJ0008.md";
#pragma warning restore S1075

            public static readonly LocalizableString Title = "Non-nullable reference member not initialized/assigned";
            public static readonly LocalizableString MessageFormat = "The non-nullable reference type member `{0}` is either not checked for null or no value was assigned in all execution paths of the checked methods {1}";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
