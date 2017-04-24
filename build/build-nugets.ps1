# Copy everything to `packages` folder
# Remove old nuget packages
Remove-Item "build\*.nupkg"

Copy-Item "src\IDE\Ammy.VisualStudio.Service\bin\Debug\*.*" "packages\Ammy.1.0.0\build"
Copy-Item "src\Core\Ammy.Host\bin\Debug\*.*" "packages\Ammy.Host.1.0.0\lib\net40"
Copy-Item "lib\Nitra-bin\System.Collections.Immutable.dll" "packages\Ammy.1.0.0\build"
Copy-Item "src\AmmyLibrary" "packages\Ammy.Host.1.0.0\content" -include "*.ammy"
# Build Ammy.Tests.sln
&"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" /m Ammy.Tests.sln

# If failed, quit
if (!$?) {
    Write-Output "Building Ammy.Tests.sln failed"
    exit 
}

# Run test executable
Write-Output "Executing runtime tests..."
& "test\Ammy.Test.Wpf\bin\Debug\Ammy.WpfTest.exe"
if (!$?) {
    Write-Output "Runtime tests failed"
    exit 
}

# Find version file
$versionFile = Get-ChildItem 'build\*.version'

if (!$versionFile) { 
    Write-Output "No version file found"
    exit 
}

Write-Output "Found version file $versionFile"
$m = ([regex]"(\d+)\.(\d+)\.(\d+)").match($versionFile)

$major = $m.Groups[1].Value
$minor = $m.Groups[2].Value
$patch = $m.Groups[3].Value

$incrementedPatch = ($patch -as [int]) + 1

Write-Output "Removing $versionFile"
Remove-Item $versionFile

$newVersion = "$major.$minor.$incrementedPatch"
$newVersionFile = "build\$newVersion.version"

Write-Output "Creating incremented file $newVersionFile"
New-Item $newVersionFile

# Rewrite all nuspec files with new version
Get-ChildItem 'build\*.nuspec' -Recurse | ForEach-Object {
    Write-Output "Replacing version in $_"
    (Get-Content $_ | 
       ForEach-Object  { $_ -replace [regex]'<version>\d+\.\d+\.\d+</version>', "<version>$newVersion</version>" } | 
       ForEach-Object  { $_ -replace [regex]'<dependency id="Ammy" version="\d+\.\d+\.\d+" />', "<dependency id=`"Ammy`" version=`"$newVersion`" />" }) | 
     Set-Content $_
}

# Call nuget.exe
Set-Location "build"

Get-ChildItem '*.nuspec' -Recurse | ForEach-Object {
    Write-Output "Building NuGet package for $_"
    &"..\tools\nuget\nuget.exe" pack "$_"
}