using AcidJunkie.Analyzers.Configuration.Aj0007;
using AcidJunkie.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0008;

internal sealed class Aj0008ConfigurationProvider : ConfigurationProviderBase<Aj0008Configuration>
{
    public static Aj0008ConfigurationProvider Instance { get; } = new();

    private Aj0008ConfigurationProvider()
    {
    }

    protected override Aj0008Configuration GetConfigurationCore(in SyntaxNodeAnalysisContext context)
    {
        if (!IsEnabled(context))
        {
            return Aj0008Configuration.Disabled;
        }

        try
        {
            var methodsToCheck = GetMethodsToCheck(context);
            return methodsToCheck switch
            {
                MethodKinds.None => Aj0008Configuration.Disabled,
                null             => Aj0008Configuration.Default,
                _                => new Aj0008Configuration(true, methodsToCheck.Value)
            };
        }
        catch (Exception ex)
        {
            var error =  ex.CreateConfigurationError(Aj0008Configuration.KeyNames.MethodsToCheck);
            return new Aj0008Configuration(error);
        }
    }

    private static MethodKinds? GetMethodsToCheck(in SyntaxNodeAnalysisContext context)
    {
        var value = context.GetOptionsValueOrDefault(Aj0008Configuration.KeyNames.MethodsToCheck);
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        return value
              .Split(['|'], StringSplitOptions.RemoveEmptyEntries)
              .Select(a => a.Trim())
              .Where(a => a.Length > 0)
              .Aggregate(MethodKinds.None, (current, part) => current | ParseMethodKind(part));
    }

    private static MethodKinds ParseMethodKind(string value)
        => Enum.TryParse<MethodKinds>(value, true, out var methodKind)
            ? methodKind
            : throw new InvalidOperationException($"Invalid value: {value}");

    private static bool IsEnabled(in SyntaxNodeAnalysisContext context)
        => context.GetOptionsBooleanValue(Aj0008Configuration.KeyNames.IsEnabled, defaultValue: true);
}
