param(
    [string]$version = "1.0.0"
)

$toolsPath = Join-Path $PSScriptRoot "tools"
$specFile = Join-Path $toolsPath "squirrel-spec.json"

dotnet pack --configuration Release --output $toolsPath

.\nuget.exe spec $specFile -o $toolsPath\Release
.\nuget.exe pack $specFile -o $toolsPath\Release -version $version
.\nuget.exe push $toolsPath\Release\*.nupkg -source https://api.nuget.org/v3/index.json

Copy-Item -Path "$toolsPath\Release\*" -Destination ".\GitHubReleases\" -Recurse -Force
