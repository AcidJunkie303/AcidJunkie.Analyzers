using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AcidJunkie.Analyzers.Configuration;
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

    private readonly IAnalyzerConfiguration _configuration;

    public EnforceEntityFrameworkTrackingTypeAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
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
        if (!IsDbSetType(memberAccessExpression, out var dbContextType))
        {
            return;
        }

        if (DoesMethodChainSpecifyTrackingType(memberAccessExpression))
        {
            return;
        }

        var topmostInvocationExpression = GetTopmostInvocationExpression(memberAccessExpression);
        if (topmostInvocationExpression is null)
        {
            return;
        }

        if (topmostInvocationExpression.Expression is MemberAccessExpressionSyntax invocationTarget)
        {
            var isIgnoredMethodName = IgnoredDbSetMethodNames.Contains(invocationTarget.Name.Identifier.Text);
            if (isIgnoredMethodName)
            {
                return; // For Update, Add, Remove etc. we don't care about tracking
            }
        }

        var returnType = Context.SemanticModel.GetTypeInfo(topmostInvocationExpression).Type;
        if (returnType is null || returnType.SpecialType == SpecialType.System_Void)
        {
            return;
        }

        var entitiesOfDbContextByNamespace = GetEntitiesOfDbContextByNamespaceName(dbContextType);
        if (!IsEntityTypeOrContainsEntityProperties(returnType, entitiesOfDbContextByNamespace))
        {
            return;
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

        if (type.IsEnumerable(out var enumerableElementType) && IsEntityTypeOrContainsEntityProperties(enumerableElementType, entityTypesByNamespaceName, visitedTypes))
        {
            return true;
        }

        if (type.IsQueryable(out var queryableElementType) && IsEntityTypeOrContainsEntityProperties(queryableElementType, entityTypesByNamespaceName, visitedTypes))
        {
            return true;
        }

        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(a => a is { IsWriteOnly: false, IsStatic: false });

        return properties.Any(property => IsEntityTypeOrContainsEntityProperties(property.Type, entityTypesByNamespaceName, visitedTypes));
    }

    private static InvocationExpressionSyntax? GetTopmostInvocationExpression(MemberAccessExpressionSyntax memberAccessExpression)
    {
        SyntaxNode current = memberAccessExpression;
        InvocationExpressionSyntax? topmostInvocationExpression = null;

        while (true)
        {
            switch (current)
            {
                case InvocationExpressionSyntax invocation:
                    topmostInvocationExpression = invocation;
                    current = invocation.Parent;
                    continue;

                case MemberAccessExpressionSyntax memberAccess:
                    current = memberAccess.Parent;
                    continue;

                default:
                    return topmostInvocationExpression;
            }
        }
    }

    private static bool DoesMethodChainSpecifyTrackingType(MemberAccessExpressionSyntax memberAccessExpression)
    {
        SyntaxNode current = memberAccessExpression;

        while (true)
        {
            switch (current)
            {
                case InvocationExpressionSyntax invocation:
                    current = invocation.Parent;
                    continue;

                case MemberAccessExpressionSyntax memberAccess:
                    var methodName = memberAccess.Name.Identifier.Text;
                    var isTrackingMethod = TrackingMethodNames.Contains(methodName);
                    if (isTrackingMethod)
                    {
                        return true; // we found an AsTracking or AsNoTracking method. So we're good
                    }

                    current = memberAccess.Parent;
                    continue;

                default:
                    return false;
            }
        }
    }

    private bool IsDbSetType(MemberAccessExpressionSyntax memberAccessExpression, [NotNullWhen(true)] out INamedTypeSymbol? dbContextType)
    {
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
        return dbContextType is not null && memberType.TypeArguments[0] is INamedTypeSymbol;
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
            public static readonly LocalizableString Title = "Specify the Entity Framework tracking type";
            public static readonly LocalizableString MessageFormat = "Specify `AsTracking` or `AsNoTracking` when obtaining entities from entity framework";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
