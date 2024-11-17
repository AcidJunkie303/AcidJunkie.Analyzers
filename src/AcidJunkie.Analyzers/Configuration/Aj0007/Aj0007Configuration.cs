using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

internal sealed class Aj0007Configuration : IAnalyzerConfiguration
{
    public static Aj0007Configuration Default { get; } = new(true, Defaults.ParameterOrder, Defaults.ParameterOrderIndexByName);
    public static Aj0007Configuration Disabled { get; } = new(false, [], FrozenDictionary<string, int>.Empty);

    public bool IsEnabled { get; }
    public FrozenDictionary<string, int> ParameterOrderIndexByName { get; }
    public ImmutableArray<string> ParameterOrder { get; }

    public Aj0007Configuration(bool isEnabled, ImmutableArray<string> parameterOrder, FrozenDictionary<string, int> parameterOrderIndexByName)
    {
        IsEnabled = isEnabled;
        ParameterOrderIndexByName = parameterOrderIndexByName;
        ParameterOrder = parameterOrder;
    }

    public bool Validate(SyntaxNodeAnalysisContext context) => context.ReportDiagnostic();

    public static class KeyNames
    {
        public const string IsEnabled = "AJ0007.is_enabled";
        public const string ParameterOrderingFlat = "AJ0007.parameter_ordering";
    }

    public static class Defaults
    {
        public const string ParameterOrderFlat = "{other}|Microsoft.Extensions.Logging|System.Threading.CancellationToken";
        public static ImmutableArray<string> ParameterOrder { get; } = [.. ParameterOrderFlat.Split('|')];

        public static FrozenDictionary<string, int> ParameterOrderIndexByName { get; } = ParameterOrder
            .Select((typeName, index) => (TypeName: typeName, Index: index))
            .ToFrozenDictionary(a => a.TypeName, a => a.Index, StringComparer.Ordinal);
    }
}
