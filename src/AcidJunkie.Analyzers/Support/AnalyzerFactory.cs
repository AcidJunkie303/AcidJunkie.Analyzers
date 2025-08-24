using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Support;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Singleton of the factory")]
public static class AnalyzerFactory<T>
    where T : class
{
    private static readonly Func<SyntaxNodeAnalysisContext, T> Factory = CreateCompiledFactory();

    public static T Create(in SyntaxNodeAnalysisContext context) => Factory(context);

    private static Func<SyntaxNodeAnalysisContext, T> CreateCompiledFactory()
    {
        var ctor = GetConstructor()
                   ?? throw new InvalidOperationException($"No suitable constructor found for type {typeof(T).FullName}");

        var contextParameter = Expression.Parameter(typeof(SyntaxNodeAnalysisContext), "context");
        var lambda = Expression.Lambda<Func<SyntaxNodeAnalysisContext, T>>
        (
            Expression.New(ctor, contextParameter),
            contextParameter
        );

        return lambda.Compile();

        ConstructorInfo? GetConstructor() =>
            typeof(T)
               .GetConstructors()
               .Where(a => a.GetParameters().Length == 1)
               .FirstOrDefault(a =>
                {
                    var parameter = a.GetParameters()[0];
                    return parameter.ParameterType == typeof(SyntaxNodeAnalysisContext)
                           || parameter.ParameterType == typeof(SyntaxNodeAnalysisContext).MakeByRefType();
                });
    }
}
