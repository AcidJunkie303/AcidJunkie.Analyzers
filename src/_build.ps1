
Set-StrictMode -Version "Latest"
$PSNativeCommandUseErrorActionPreference = $true
$ErrorActionPreference = "Stop"

function Compile 
{
    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn3.8" -ForegroundColor Cyan
    dotnet.exe build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn3.8
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn3.8" -ForegroundColor Cyan
    dotnet.exe build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn3.8

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn3.9" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn3.9
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn3.9" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn3.9

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn3.10" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn3.10
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn3.10" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn3.10

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn3.11" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn3.11
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn3.11" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn3.11

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.0.1" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.0.1
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.0.1" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.0.1

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.1" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.1
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.1" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.1

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.2" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.2
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.2" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.2

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.3.1" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.3.1
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.3.1" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.3.1

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.4" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.4
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.4" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.4

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.5" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.5
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.5" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.5

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.6" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.6
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.6" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.6

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.7" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.7
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.7" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.7

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.8" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.8
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.8" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.8

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.9.2" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.9.2
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.9.2" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.9.2

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.10" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.10
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.10" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.10

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.11" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.11
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.11" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.11

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.12" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.12
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.12" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.12

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.13" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.13
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.13" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.13

    Write-Host "Compiling 'AcidJunkie.Analyzers' for roslyn4.14" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.14
    Write-Host "Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn4.14" -ForegroundColor Cyan
    dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn4.14

    dotnet restore AcidJunkie.Analyzers.Pack/AcidJunkie.Analyzers.Pack.csproj
    dotnet pack AcidJunkie.Analyzers.Pack/AcidJunkie.Analyzers.Pack.csproj --configuration Release --no-build 
}

Push-Location $PSScriptRoot

try {
    Compile
}
finally {
    Pop-Location
}