using System.Text.RegularExpressions;

namespace AcidJunkie.Analyzers.Configuration.Aj0007;

public sealed class ParameterDescription
{
    private readonly Regex _matcher;
    public int Index { get; }
    public bool IsOther { get; }

    public ParameterDescription(Regex matcher, int index, bool isOther)
    {
        _matcher = matcher;
        Index = index;
        IsOther = isOther;
    }

    public bool Matcher(string typeName) => _matcher.IsMatch(typeName);
}
