$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-RoslynVersions {

    $path = Join-Path $PSScriptRoot "ContentsGenerator.csv"
    $versions = Get-Content $path | ConvertFrom-Csv 
    $latestRoslynVersion = $versions | Select-Object -ExpandProperty "RoslynVersion" -Last 1

    $versions `
        | ForEach-Object {
            [PSCustomObject]@{
                Include                 = [boolean]::Parse($_.Include)
                IsLast                  = ($_.RoslynVersion -eq $latestRoslynVersion)
                RoslynVersion           = $_.RoslynVersion
                CSharpVersion           = $_.CSharpVersion
                AnalyzersPackageVersion = $_.AnalyzersPackageVersion
                NoWarn                  = $_.NoWarn
            } `
        } `
        | Where-Object { $_.Include }
}

function Get-Node([bool] $IsLast, [string] $RoslynVersion) {
    if ($IsLast) {
        return "<Otherwise>"
    }

    return "<When Condition=`"`$(RoslynVersion) == 'roslyn$RoslynVersion'`">"
}

function Get-NodeName([bool] $IsLast) {
    if ($IsLast) {
        return "Otherwise"
    }

    return "When"
}

function Get-NodeContents(
    [string] $RoslynVersion, 
    [string] $TrimmedRoslynVersion,
    [string] $CSharpVersion, 
    [string] $RoslynPreprocessorDefinitions,
    [string] $CSharpPreprocessorDefinitions, 
    [string] $AnalyzersPackageVersion,
    [string] $NoWarn,
    [bool] $IsLast
) {

    $template = @"
        {{Node}}
            <ItemGroup >
                <PackageReference Update="Microsoft.CodeAnalysis.Analyzers" Version="{{AnalyzersPackageVersion}}" />
                <PackageReference Update="Microsoft.CodeAnalysis.CSharp" Version="{{RoslynVersion}}" />
                <PackageReference Update="Microsoft.CodeAnalysis.Workspaces.Common" Version="{{RoslynVersion}}" />
                <PackageReference Update="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="{{RoslynVersion}}" />
            </ItemGroup>
            <PropertyGroup>
                <DefineConstants>`$(DefineConstants);{{RoslynPreprocessorDefinitions}}</DefineConstants>
                <DefineConstants>`$(DefineConstants);{{CSharpPreprocessorDefinitions}}</DefineConstants>
                <NoWarn>{{NoWarn}}</NoWarn>
            </PropertyGroup>
        </{{NodeName}}>
"@

    $value = $template.Replace("{{Node}}", (Get-Node $IsLast $TrimmedRoslynVersion))
    $value = $value.Replace("{{NodeName}}", (Get-NodeName $IsLast))
    $value = $value.Replace("{{AnalyzersPackageVersion}}", $analyzersPackageVersion)
    $value = $value.Replace("{{RoslynVersion}}", $RoslynVersion)
    $value = $value.Replace("{{RoslynPreprocessorDefinitions}}", $RoslynPreprocessorDefinitions)
    $value = $value.Replace("{{CSharpPreprocessorDefinitions}}", $CSharpPreprocessorDefinitions)
    $value = $value.Replace("{{NoWarn}}", $NoWarn)
      
    $value
}

function Append-SemiColonSeparatedValue([string] $ExistingValue, [string] $ValueToAdd) {
    if (!$ExistingValue) {
        return $ValueToAdd
    }

    if ($ExistingValue.Contains("$ValueToAdd;") -or $ExistingValue.EndsWith("$ValueToAdd") ) {
        return $ExistingValue
    }

    "$ExistingValue;$ValueToAdd"
}

function Remove-TrailingZeroDigitsFromSemVer([string] $Version) {
    while ($true) {
        if ($Version.EndsWith(".0")) {
            $Version = $Version.Substring(0, $Version.Length - 2)
        }
        else {
            return $Version
        }
    }
}

function Reverse-Collection($Collection) {
    for ($i = $Collection.Count - 1 ; $i -ge 0 ; $i-- ) {
        $Collection[$i]
    }
}

function Build-PreprocessorDefinitions([string] $Prefix, [string] $CurrentVersion, [System.Collections.ArrayList] $PreviousVersions ) {
    $value = "$($Prefix)_$CurrentVersion;$($Prefix)_$($CurrentVersion)_OR_GREATER"


    foreach ($previousVersion in Reverse-Collection($PreviousVersions)) {
        if ($previousVersion -eq $CurrentVersion) {
            continue
        }

        $value += ";$($Prefix)_$($previousVersion)_OR_GREATER"
    }
    
    $value
}

function Build-TargetFileNodes {

    $roslynPreprocessorDefinitions = ""
    $cSharpPreprocessorDefinitions = ""
    $previusRoslynPreprocessorVersions = [System.Collections.ArrayList]::new()
    $previusCSharpPreprocessorVersions = [System.Collections.ArrayList]::new()

    foreach ($roslynVersionInfo in Get-RoslynVersions) {

        $roslynVersion = $roslynVersionInfo.RoslynVersion
        $trimmedRoslynVersion = Remove-TrailingZeroDigitsFromSemVer($roslynVersionInfo.RoslynVersion)
        $cSharpVersion = $roslynVersionInfo.CSharpVersion
        $analyzersPackageVersion = $roslynVersionInfo.AnalyzersPackageVersion
        $noWarn = $roslynVersionInfo.NoWarn
        $preprocessorRoslynVersion = $trimmedRoslynVersion.Replace(".", "_")
        $preprocessorCSharpVersion = $cSharpVersion.Replace(".", "_")

        $roslynPreprocessorDefinitions = Build-PreprocessorDefinitions "ROSLYN" $preprocessorRoslynVersion $previusRoslynPreprocessorVersions
        $cSharpPreprocessorDefinitions = Build-PreprocessorDefinitions "CSHARP" $preprocessorCSharpVersion $previusCSharpPreprocessorVersions

        Get-NodeContents $roslynVersion $trimmedRoslynVersion $cSharpVersion $roslynPreprocessorDefinitions $cSharpPreprocessorDefinitions $analyzersPackageVersion $noWarn $roslynVersionInfo.IsLast

        if (!($previusRoslynPreprocessorVersions.Contains($preprocessorRoslynVersion))) {
            $previusRoslynPreprocessorVersions.Add($preprocessorRoslynVersion) | Out-Null
        }

        if (!($previusCSharpPreprocessorVersions.Contains($preprocessorCSharpVersion))) {
            $previusCSharpPreprocessorVersions.Add($preprocessorCSharpVersion) | Out-Null
        }
    }
}

function Create-BuildCommands {

    foreach ($roslynVersionInfo in Get-RoslynVersions) {

        if ($roslynVersionInfo.IsLast) {
            continue
        }

        $roslynVersion = Remove-TrailingZeroDigitsFromSemVer($roslynVersionInfo.RoslynVersion)

        "Write-Host ""Compiling 'AcidJunkie.Analyzers' for roslyn$roslynVersion"" -ForegroundColor Cyan"
        "dotnet build AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn$roslynVersion"
        "Write-Host ""Compiling 'AcidJunkie.Analyzers.CodeFixers' for roslyn$roslynVersion"" -ForegroundColor Cyan"
        "dotnet build AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn$roslynVersion"
        ""        
    }
}


function Create-PackProjectFileReferences {
    foreach ($roslynVersionInfo in Get-RoslynVersions) {

        if ($roslynVersionInfo.IsLast) {
            continue
        }

        $roslynVersion = Remove-TrailingZeroDigitsFromSemVer($roslynVersionInfo.RoslynVersion)

        "<None Include=`"`$`(MSBuildThisFileDirectory)\..\AcidJunkie.Analyzers\bin\roslyn$roslynVersion\`$(Configuration)\netstandard2.0\AcidJunkie.Analyzers.dll`" Pack=`"true`" PackagePath=`"analyzers/dotnet/roslyn$roslynVersion/cs/`"/>"
        "<None Include=`"`$(MSBuildThisFileDirectory)\..\AcidJunkie.Analyzers.CodeFixers\bin\roslyn$roslynVersion\`$(Configuration)\netstandard2.0\AcidJunkie.Analyzers.CodeFixers.dll`" Pack=`"true`" PackagePath=`"analyzers/dotnet/roslyn$roslynVersion/cs/`" />"
        ""
        
    } 
}

function Create-BuildCommandsForPipeline {
    foreach ($roslynVersionInfo in Get-RoslynVersions) {

        if ($roslynVersionInfo.IsLast) {
            continue
        }

        $roslynVersion = Remove-TrailingZeroDigitsFromSemVer($roslynVersionInfo.RoslynVersion)

        "      - name: Compile Analyzers and Fixers for Roslyn $roslynVersion"
        "        run: |"
        "          dotnet build src/AcidJunkie.Analyzers/AcidJunkie.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn$roslynVersion /p:Version=`$ASSEMBLY_VERSION"
        "          dotnet build src/AcidJunkie.Analyzers.CodeFixers/AcidJunkie.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn$roslynVersion /p:Version=`$ASSEMBLY_VERSION"
        ""
    }
}

"********************************************************************************************************************************"
"* Directory.Build.Target contents"
"********************************************************************************************************************************"
Build-TargetFileNodes
"********************************************************************************************************************************"
"* Local Build Commands"
"********************************************************************************************************************************"
Create-BuildCommands
"********************************************************************************************************************************"
"* Analyzers.Pack project file references"
"********************************************************************************************************************************"
Create-PackProjectFileReferences
"********************************************************************************************************************************"
"* Pipeline contents"
"********************************************************************************************************************************"
Create-BuildCommandsForPipeline
"********************************************************************************************************************************"
"********************************************************************************************************************************"
