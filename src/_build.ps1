
Set-StrictMode -Version "Latest"
$PSNativeCommandUseErrorActionPreference = $true
$ErrorActionPreference = "Stop"

function CleanUp
{
    Get-ChildItem -Recurse -Filter "obj" -Directory | Remove-Item -Recurse
    Get-ChildItem -Recurse -Filter "bin" -Directory | Remove-Item -Recurse
}

function CompileForSpecificRoslynVersion([string] $RoslynVersion) {
    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn$RoslynVersion" -ForegroundColor Cyan
    dotnet.exe build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn$RoslynVersion
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn$RoslynVersion" -ForegroundColor Cyan
    dotnet.exe build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn$RoslynVersion
}

function CompileAll {

    CompileForSpecificRoslynVersion "3.8"
    CompileForSpecificRoslynVersion "3.9"
    CompileForSpecificRoslynVersion "3.10"
    CompileForSpecificRoslynVersion "3.11"
    CompileForSpecificRoslynVersion "4.01"
    CompileForSpecificRoslynVersion "4.1"
    CompileForSpecificRoslynVersion "4.2"
    CompileForSpecificRoslynVersion "4.3.1"
    CompileForSpecificRoslynVersion "4.4"
    CompileForSpecificRoslynVersion "4.5"
    CompileForSpecificRoslynVersion "4.6"
    CompileForSpecificRoslynVersion "4.7"
    CompileForSpecificRoslynVersion "4.8"
    CompileForSpecificRoslynVersion "4.9.2"
    CompileForSpecificRoslynVersion "4.10"
    CompileForSpecificRoslynVersion "4.11"
    CompileForSpecificRoslynVersion "4.12"
    CompileForSpecificRoslynVersion "4.13"
    CompileForSpecificRoslynVersion "4.14"

    dotnet restore AcidJunkie.Analyzers.Pack/AcidJunkie.Analyzers.Pack.csproj
    dotnet pack AcidJunkie.Analyzers.Pack/AcidJunkie.Analyzers.Pack.csproj --configuration Release --no-build 
}

Push-Location $PSScriptRoot

try {
    CleanUp
    CompileAll
}
finally {
    Pop-Location
}