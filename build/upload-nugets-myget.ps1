Set-Location "build"

Write-Output "Setting API key"

$apiKey = Read-Host -Prompt "Please enter API key"

&"..\tools\nuget\nuget.exe" setApiKey $apiKey -source https://www.myget.org/F/ammy/api/v2/package

Get-ChildItem '*.nupkg' -Recurse | ForEach-Object {
    Write-Output "Uploading $_"
    &"..\tools\nuget\nuget.exe" push $_ -Source https://www.myget.org/F/ammy/api/v2/package
}