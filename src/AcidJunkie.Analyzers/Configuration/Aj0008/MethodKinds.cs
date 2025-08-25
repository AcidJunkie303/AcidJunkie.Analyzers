namespace AcidJunkie.Analyzers.Configuration.Aj0008;

internal enum MethodKinds
{
    None = 0,
    OnInitialized = 1,
    OnInitializedAsync = 2,
    OnParametersSet = 3,
    OnParametersSetAsync = 4
}
