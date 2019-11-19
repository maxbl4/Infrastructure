dotnet test
if (-not $?) { exit; }
$versionText = Get-Content publish.version
$version = [System.Version]::Parse($versionText);
$versionText = "$($version.Major).$($version.Minor).$($version.Build + 1)"
"Packing version $versionText"
dotnet pack /p:Version=$versionText .\maxbl4.Infrastructure\maxbl4.Infrastructure.csproj
if (-not $?) { exit; }
nuget push .\maxbl4.Infrastructure\bin\Debug\maxbl4.Infrastructure.$versionText.nupkg
if (-not $?) { exit; }
$versionText > publish.version
"Successfully published package maxbl4.Infrastructure.$versionText"