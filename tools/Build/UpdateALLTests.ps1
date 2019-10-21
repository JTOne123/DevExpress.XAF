param(
    $root = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\..\"),
    $branch="lab",
    $source
)
if ($branch -eq "lab" -and !$source){
    $source=Get-PackageFeed -Xpand
}
if ($branch -eq "master") {
    $branch = "Release"
}
$ErrorActionPreference = "Stop"
# Import-XpandPwsh
$excludeFilter = "*client*;*extension*"
$localPackages = Get-ChildItem "$root\tools\nuspec" *ALL.nuspec | ForEach-Object {
    [xml]$nuspec = Get-Content $_.FullName
    $version = [version]$nuspec.package.metadata.Version
    if ($version.revision -eq 0) {
        $version = New-Object System.Version ($version.Major, $version.Minor, $version.build)
    }
    [PSCustomObject]@{
        Id      = $nuspec.package.metadata.id
        Version = $version
    }
}
Write-Host "LocalPackages:" -f blue
$localPackages | Out-String
$remotePackages = Get-XpandPackages -PackageType XAF -Source $branch |Where-Object{$_.Name -like "*All"}
Write-Host "remotePackages:" -f Blue
$remotePackages | Out-String
$latestPackages = (($localPackages + $remotePackages) | Group-Object Id | ForEach-Object {
        $_.group | Sort-Object Version -Descending | Select-Object -first 1
    })
Write-Host "latestPackages:" -f Blue
$latestPackages | Out-String
$packages = $latestPackages | Where-Object {
    $p = $_
    !($excludeFilter.Split(";") | Where-Object { $p.Id -like $_ })
}
Write-Host "finalPackages:" -f Blue
$packages | Out-String

$csprojPath = "$root\src\Tests\All\ALL.Win.Tests\ALL.Win.Tests.csproj"
[xml]$csproj = Get-Content $csprojPath

$csproj.Project.ItemGroup.PackageReference | Where-Object { $_.Include -like "Xpand.*" } | ForEach-Object {
    $pref = $_
    $packages | Where-Object { $_.Id -eq $pref.Include } | ForEach-Object {
        if ($_.Version -ne ([version]$pref.Version)) {
            Write-Host "$($_.Id) version changed from $($pref.Version) to $($_.Version)" -f Green
            $pref.Version = $_.Version.ToString()
        }
        
    }
}
$csproj.Save($csprojPath)
Get-Content $csprojPath -Raw
Write-Host "Building TestApplication" -f Green
$testApplication="$root\src\Tests\ALL\TestApplication\TestApplication.sln"
# "dotnet restore $testApplication --source $source --source `"$root\bin\Nupkg`" --source $(Get-PackageFeed -Nuget) /WarnAsError"
# dotnet restore $testApplication --source $source --source `"$root\bin\Nupkg`" --source $(Get-PackageFeed -Nuget) --packages (Get-NugetInstallationFolder GlobalPackagesFolder)
$localSource="$root\bin\Nupkg"
& (Get-MsBuildPath) $testApplication /WarnAsError /t:restore /p:RestoreAdditionalProjectSources=$source;$localSource; 
# "dotnet msbuild $testApplication /p:configuration=Debug /WarnAsError --no-restore"
# Remove-Item "$root\src\Tests\ALL\TestApplication\TestApplication.Web\obj" -Force -Recurse
# dotnet msbuild $testApplication  --no-restore


