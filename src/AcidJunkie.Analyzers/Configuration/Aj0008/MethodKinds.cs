namespace AcidJunkie.Analyzers.Configuration.Aj0008;

[Flags]
internal enum MethodKinds
{
    None = 0,
    OnInitialized = 1,
    OnInitializedAsync = 2,
    OnParametersSet = 4
}
