#1.0.41
$packages = @(
    @{Name = "maxbl4.Infrastructure"; Project = "Infrastructure"}
)

function Main()
{
    $version = GetNextVersion

    #dotnet test
    if (-not $?) { exit $? }

    foreach ($p in $packages){
        Pack $p $version
    }

    UpdateVersion $version
}

function Pack($pkg, $version)
{
    $csproj = ".\$($pkg.Project)\$($pkg.Project).csproj"
    $nupkg = ".\$($pkg.Project)\bin\Release\$($pkg.Name).$version.nupkg"
    dotnet pack -c Release /p:PackageID="$($pkg.Name)" /p:Version=$version $csproj
    if (-not $?) { exit; }
    nuget push -Source NugetLocal $nupkg
    if (-not $?) { exit; }
    nuget push $nupkg
    if (-not $?) { exit; }
}

function GetNextVersion()
{
    $lines = Get-Content $MyInvocation.ScriptName
    $version = [System.Version]::Parse($lines[0].Substring(1))
    return "$($version.Major).$($version.Minor).$($version.Build + 1)"
}

function UpdateVersion($version)
{
    $lines = Get-Content $MyInvocation.ScriptName
    $lines[0] = "#$version"
    Set-Content $MyInvocation.ScriptName $lines -Encoding UTF8     
}

Main
