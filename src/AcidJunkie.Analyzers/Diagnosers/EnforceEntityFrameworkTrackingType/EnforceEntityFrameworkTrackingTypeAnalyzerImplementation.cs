using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration.Aj0002;
using AcidJunkie.Analyzers.Extensions;
using AcidJunkie.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using EntityTypesByNamespaceName = System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyList<Microsoft.CodeAnalysis.INamedTypeSymbol>>;

namespace AcidJunkie.Analyzers.Diagnosers.EnforceEntityFrameworkTrackingType;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class EnforceEntityFrameworkTrackingTypeAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<EnforceEntityFrameworkTrackingTypeAnalyzer>
{
    private static readonly ImmutableHashSet<string> IgnoredDbSetMethodNames = new[]
    {
        "Add",
        "AddAsync",
        "AddRange",
        "AddRangeAsync",
        "Attach",
        "AttachRange",
        "Remove",
        "RemoveRange",
        "Update",
        "UpdateRange"
    }.ToImmutableHashSet(StringComparer.Ordinal);

    private static readonly ImmutableHashSet<string> TrackingMethodNames = new[]
    {
        "AsTracking",
        "AsNoTracking"
    }.ToImmutableHashSet(StringComparer.Ordinal);

    private readonly Aj0002Configuration _configuration;

    public EnforceEntityFrameworkTrackingTypeAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = Aj0002ConfigurationProvider.Instance.GetConfiguration(context);
    }

    [SuppressMessage("Critical Code Smell", "S134:Control flow statements \"if\", \"switch\", \"for\", \"foreach\", \"while\", \"do\"  and \"try\" should not be nested too deeply")]
    [SuppressMessage("Minor Code Smell", "S1227:break statements should not be used except for switch cases")]
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high")]
    public void AnalyzeMemberAccessExpression()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var memberAccessExpression = (MemberAccessExpressionSyntax)Context.Node;
        if (!IsDbSetType(memberAccessExpression, out var dbContextType, out _))
        {
            return;
        }

        Lazy<IReadOnlyDictionary<string, IReadOnlyList<INamedTypeSymbol>>> entitiesOfDbContextByNamespaceNameLazy =
            new(() => GetEntitiesOfDbContextByNamespaceName(dbContextType));

        var currentExpression = memberAccessExpression.Parent;
        while (currentExpression is not null)
        {
            if (currentExpression is MemberAccessExpressionSyntax memberAccess)
            {
                currentExpression = memberAccess.Parent;
                continue;
            }

            if (currentExpression is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax invokedMember)
                {
                    var methodName = invokedMember.Name.Identifier.Text;
                    var isTrackingMethod = TrackingMethodNames.Contains(methodName);
                    if (isTrackingMethod)
                    {
                        return; // we found an AsTracking or AsNoTracking method. So we're good
                    }

                    var isIgnoredMethodName = IgnoredDbSetMethodNames.Contains(methodName);
                    if (isIgnoredMethodName)
                    {
                        return; // For Update, Add, Remove etc. we don't care about tracking
                    }
                }

                if (_configuration.Mode != Mode.Strict)
                {
                    if (IsSelectStatement(invocation, out var resultType))
                    {
                        if (IsEntityTypeOrContainsEntityProperties(resultType, entitiesOfDbContextByNamespaceNameLazy.Value))
                        {
                            break; // the select returns an entity. So we abort and report the diagnostic
                        }

                        return; // if the select statement does not return an entity, we're good
                    }
                }
            }

            currentExpression = currentExpression.Parent;
        }

        // If no AsTracking or AsNoTracking was found in the chain, raise a diagnostic
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, memberAccessExpression.GetLocation()));
    }

    private static bool IsEntityTypeOrContainsEntityProperties(ITypeSymbol type, EntityTypesByNamespaceName entityTypesByNamespaceName)
    {
#pragma warning disable RS1024 // We want to do it by reference in this case
        var visitedTypes = new HashSet<ITypeSymbol>(EqualityComparer<ITypeSymbol>.Default);
#pragma warning restore RS1024

        return IsEntityTypeOrContainsEntityProperties(type, entityTypesByNamespaceName, visitedTypes);
    }

    private static bool IsEntityTypeOrContainsEntityProperties(ITypeSymbol type, EntityTypesByNamespaceName entityTypesByNamespaceName, HashSet<ITypeSymbol> visitedTypes)
    {
        if (visitedTypes.Contains(type))
        {
            return false;
        }

        var isEntityType = entityTypesByNamespaceName.TryGetValue(type.GetFullNamespace(), out var entities)
                           && entities.Contains(type, SymbolEqualityComparer.Default);
        if (isEntityType)
        {
            return true;
        }

        visitedTypes.Add(type);

        if (type.IsEnumerable(out var elementType) && IsEntityTypeOrContainsEntityProperties(elementType, entityTypesByNamespaceName, visitedTypes))
        {
            return true;
        }

        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(a => a is { IsWriteOnly: false, IsStatic: false });

        return properties.Any(property => IsEntityTypeOrContainsEntityProperties(property.Type, entityTypesByNamespaceName, visitedTypes));
    }

    private bool IsSelectStatement(InvocationExpressionSyntax invocationExpression, [NotNullWhen(true)] out ITypeSymbol? resultType)
    {
        resultType = null;

        if (invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return false;
        }

        if (!memberAccessExpression.Name.Identifier.Text.EqualsOrdinal("Select"))
        {
            return false;
        }

        if (Context.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!methodSymbol.IsExtensionMethod
            || !methodSymbol.ContainingType.Name.EqualsOrdinal("Queryable")
            || !methodSymbol.ContainingType.GetFullNamespace().EqualsOrdinal("System.Linq"))
        {
            return false;
        }

        if (Context.SemanticModel.GetTypeInfo(invocationExpression).Type is not INamedTypeSymbol typeInfo)
        {
            return false;
        }

        if (typeInfo.Arity != 1 || !typeInfo.Name.EqualsOrdinal("IQueryable") || !typeInfo.GetFullNamespace().EqualsOrdinal("System.Linq"))
        {
            return false;
        }

        resultType = typeInfo.TypeArguments[0];

        return true;
    }

    private bool IsDbSetType(MemberAccessExpressionSyntax memberAccessExpression, [NotNullWhen(true)] out INamedTypeSymbol? dbContextType, [NotNullWhen(true)] out INamedTypeSymbol? entityType)
    {
        entityType = null;
        dbContextType = null;

        if (Context.SemanticModel.GetTypeInfo(memberAccessExpression).Type is not INamedTypeSymbol memberType)
        {
            return false;
        }

        if (!memberType.IsTypeOrIsInheritedFrom(Context.Compilation, "Microsoft.EntityFrameworkCore.DbSet`1"))
        {
            return false;
        }

        if (memberAccessExpression.Expression is not IdentifierNameSyntax identifierName)
        {
            return false;
        }

        dbContextType = Context.SemanticModel.GetTypeInfo(identifierName).Type as INamedTypeSymbol;
        entityType = memberType.TypeArguments[0] as INamedTypeSymbol;
        return dbContextType is not null && entityType is not null;
    }

    private Dictionary<string, IReadOnlyList<INamedTypeSymbol>> GetEntitiesOfDbContextByNamespaceName(INamedTypeSymbol dbContextType)
    {
        return dbContextType
              .GetMembers()
              .OfType<IPropertySymbol>()
              .Where(a => a is { IsReadOnly: false, IsWriteOnly: false, IsAbstract: false, IsStatic: false, DeclaredAccessibility: Accessibility.Public })
              .Select(a => GetDbSetType(a.Type as INamedTypeSymbol))
              .WhereNotNull()
              .GroupBy(a => a.GetFullNamespace(), StringComparer.Ordinal)
              .ToDictionary(a => a.Key, a => (IReadOnlyList<INamedTypeSymbol>)a.ToList(), StringComparer.Ordinal);

        INamedTypeSymbol? GetDbSetType(INamedTypeSymbol? propertyType)
        {
            if (propertyType is null)
            {
                return null;
            }

            if (!propertyType.IsTypeOrIsInheritedFrom(Context.Compilation, "Microsoft.EntityFrameworkCore.DbSet`1"))
            {
                return null;
            }

            return propertyType.TypeArguments[0] as INamedTypeSymbol;
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
            private const string Category = "Intention";
            public const string DiagnosticId = "AJ0002";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Specify AsTracking or AsNoTracking when querying entity framework";
            public static readonly LocalizableString MessageFormat = "Specify AsTracking or AsNoTracking when querying entity framework";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
