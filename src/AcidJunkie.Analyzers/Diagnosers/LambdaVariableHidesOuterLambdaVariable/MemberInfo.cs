namespace AcidJunkie.Analyzers.Diagnosers.LambdaVariableHidesOuterLambdaVariable;

internal sealed class MemberInfo
{
    public string Name { get; }
    public MemberKind Kind { get; }
    public bool IsStatic { get; }

    public MemberInfo(string name, MemberKind kind, bool isStatic)
    {
        Name = name;
        Kind = kind;
        IsStatic = isStatic;
    }
}
