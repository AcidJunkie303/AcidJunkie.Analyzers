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

namespace AcidJunkie.Analyzers.Diagnosers.ReturnMaterializedCollectionAsEnumerable;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation>
{
    private readonly IAnalyzerConfiguration _configuration;

    public ReturnMaterializedCollectionAsEnumerableAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeArrowExpression()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var arrowExpression = (ArrowExpressionClauseSyntax)Context.Node;

        var methodOrLocalFunction = arrowExpression.Parent switch
        {
            MethodDeclarationSyntax methodDeclaration  => new MethodDeclarationSyntaxOrLocalFunctionDeclaration(methodDeclaration),
            LocalFunctionStatementSyntax localFunction => new MethodDeclarationSyntaxOrLocalFunctionDeclaration(localFunction),
            _                                          => null
        };
        if (methodOrLocalFunction is null)
        {
            return;
        }

        var firstNonCastExpression = arrowExpression.Expression.GetFirstNonCastExpression();
        var actualReturnedType = Context.SemanticModel.GetTypeInfo(firstNonCastExpression).Type;
        if (actualReturnedType is null)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to determine the actual return type of the expression");
            return;
        }

        AnalyzeCore(methodOrLocalFunction, actualReturnedType, arrowExpression.ArrowToken.GetLocation());
    }

    public void AnalyzeReturn()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var returnStatement = (ReturnStatementSyntax)Context.Node;
        if (returnStatement.Expression is null)
        {
            Logger.WriteLine(LogLevel.Full, "return statement has no expression");
            return;
        }

        var firstNonCastExpression = returnStatement.Expression.GetFirstNonCastExpression();
        var actualReturnedType = Context.SemanticModel.GetTypeInfo(firstNonCastExpression).Type;
        if (actualReturnedType is null)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to determine the actual return type of the expression");
            return;
        }

        var containingMethodOrLocalFunction = GetContainingMethodOrLocalFunction(returnStatement);
        if (containingMethodOrLocalFunction is null)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to determine method the return statement belongs to");
            return;
        }

        AnalyzeCore(containingMethodOrLocalFunction, actualReturnedType, returnStatement.ReturnKeyword.GetLocation());
    }

    private static bool IsOverrideOrNewModifier(MethodDeclarationSyntax methodDeclaration)
        => methodDeclaration.Modifiers.Any(static a => a.IsKind(SyntaxKind.OverrideKeyword) || a.IsKind(SyntaxKind.NewKeyword));

    private static bool IsEnumerable(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        var ns = typeSymbol.ContainingNamespace?.ToString() ?? string.Empty;
        if (!typeSymbol.Name.EqualsOrdinal("IEnumerable"))
        {
            return false;
        }

        return ns.EqualsOrdinal("System.Collections")
               || (namedTypeSymbol.Arity == 1 && ns.EqualsOrdinal("System.Collections.Generic"));
    }

    [SuppressMessage("Critical Code Smell", "S131:\"switch/Select\" statements should contain a \"default/Case Else\" clauses")]
    private static MethodDeclarationSyntaxOrLocalFunctionDeclaration? GetContainingMethodOrLocalFunction(ReturnStatementSyntax node)
    {
        foreach (var parent in node.Ancestors())
        {
            switch (parent)
            {
                case ReturnStatementSyntax:
                case LambdaExpressionSyntax:
                case ArrowExpressionClauseSyntax: return null;
                case MethodDeclarationSyntax methodDeclaration:  return new MethodDeclarationSyntaxOrLocalFunctionDeclaration(methodDeclaration);
                case LocalFunctionStatementSyntax localFunction: return new MethodDeclarationSyntaxOrLocalFunctionDeclaration(localFunction);
            }
        }

        return null;
    }

    private void AnalyzeCore(MethodDeclarationSyntaxOrLocalFunctionDeclaration methodOrFunction, ITypeSymbol actualReturnedType, Location location)
    {
        var declaredReturnType = Context.SemanticModel.GetTypeInfo(methodOrFunction.ReturnType, Context.CancellationToken).Type;
        if (declaredReturnType is null)
        {
            Logger.WriteLine(LogLevel.Full, "Unable to determine the return type syntax of the method");
            return;
        }

        if (!IsEnumerable(declaredReturnType))
        {
            Logger.WriteLine(LogLevel.Full, "Method return type is not IEnumerable or IEnumerable<T>");
            return;
        }

        if (methodOrFunction.MethodDeclaration is not null && IsOverrideOrInterfaceImplementation(methodOrFunction.MethodDeclaration))
        {
            return;
        }

        if (!actualReturnedType.DoesImplementWellKnownCollectionInterface())
        {
            Logger.WriteLine(LogLevel.Full, $"Return type {actualReturnedType.GetFullName()} does is or does not implement any well known collection interfaces");
            return;
        }

        Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, location);
        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, location));
    }

    private bool IsOverrideOrInterfaceImplementation(MethodDeclarationSyntax containingMethod)
    {
        if (IsOverrideOrNewModifier(containingMethod))
        {
            Logger.WriteLine(LogLevel.Full, "Method has an override or new modifier");
            return true;
        }

        if (IsInterfaceImplementation(containingMethod))
        {
            Logger.WriteLine(LogLevel.Full, "Method is an interface implementation");
            return true;
        }

        return false;
    }

    private bool IsInterfaceImplementation(MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return false;
        }

        var methodSymbol = Context.SemanticModel.GetDeclaredSymbol(methodDeclaration, Context.CancellationToken);
        if (methodSymbol is null)
        {
            return false;
        }

        var containingType = methodSymbol.ContainingType;
        return containingType.AllInterfaces.Any(IsMethodDefinedInInterface);

        bool IsMethodDefinedInInterface(INamedTypeSymbol interfaceSymbol) =>
            interfaceSymbol
               .GetMembers(methodSymbol.Name)
               .Any(member => member is IMethodSymbol method && IsInterfaceImplementationCore(method, methodSymbol));

        static bool IsInterfaceImplementationCore(IMethodSymbol methodSymbolOfInterface, IMethodSymbol methodSymbolOfImplementation) =>
            methodSymbolOfInterface.Parameters.Length == methodSymbolOfImplementation.Parameters.Length
            && methodSymbolOfInterface.Name.EqualsOrdinal(methodSymbolOfImplementation.Name)
            && SymbolEqualityComparer.Default.Equals(methodSymbolOfInterface.ReturnType, methodSymbolOfImplementation.ReturnType)
            && methodSymbolOfInterface.TypeParameters.Length == methodSymbolOfImplementation.TypeParameters.Length
            && methodSymbolOfInterface.TypeParameters
                                      .Zip(methodSymbolOfImplementation.TypeParameters, (a, b) => SymbolEqualityComparer.Default.Equals(a, b))
                                      .All(a => a);
    }

    private sealed class MethodDeclarationSyntaxOrLocalFunctionDeclaration
    {
        private LocalFunctionStatementSyntax? LocalFunctionStatement { get; }
        public MethodDeclarationSyntax? MethodDeclaration { get; }
        public TypeSyntax ReturnType => MethodDeclaration?.ReturnType ?? LocalFunctionStatement!.ReturnType;

        public MethodDeclarationSyntaxOrLocalFunctionDeclaration(MethodDeclarationSyntax methodDeclaration)
        {
            MethodDeclaration = methodDeclaration;
        }

        public MethodDeclarationSyntaxOrLocalFunctionDeclaration(LocalFunctionStatementSyntax localFunctionStatement)
        {
            LocalFunctionStatement = localFunctionStatement;
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
            private const string Category = "Performance";
            public const string DiagnosticId = "AJ0003";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Do not return materialized collection as enumerable";
            public static readonly LocalizableString MessageFormat = "Do not return materialized collection as enumerable";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
