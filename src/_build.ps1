

Write-Host "Compiling for roslyn3.8" -ForegroundColor Cyan
dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn3.8

Write-Host "Compiling for roslyn4.2" -ForegroundColor Cyan
dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.2

Write-Host "Compiling for roslyn4.4" -ForegroundColor Cyan
dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.4

Write-Host "Compiling for roslyn4.6" -ForegroundColor Cyan
dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.6

Write-Host "Compiling for roslyn4.8" -ForegroundColor Cyan
dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn4.8

Write-Host "Compiling for latest-roslyn" -ForegroundColor Cyan
dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=latest

dotnet restore AcidJunkie.Analyzers.Pack/AcidJunkie.Analyzers.Pack.csproj
dotnet pack AcidJunkie.Analyzers.Pack/AcidJunkie.Analyzers.Pack.csproj --configuration Release --no-build 
#dotnet pack AcidJunkie.Analyzers.Annotations/AcidJunkie.Analyzers.Annotations.csproj --configuration Release
