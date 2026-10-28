$ErrorActionPreference='Stop'
. "$PSScriptRoot/environment.ps1"
$root=Split-Path $PSScriptRoot
$env:ASPNETCORE_ENVIRONMENT='Development'
Write-Host 'Building current source. Close any older website process first (Ctrl+C).'
dotnet restore "$root/Vtb.Hr.Web.csproj" --configfile "$root/NuGet.Config"
if($LASTEXITCODE -ne 0){throw 'Restore failed; website was not started.'}
dotnet build "$root/Vtb.Hr.Web.csproj" --no-restore -c Debug
if($LASTEXITCODE -ne 0){throw 'Build failed; website was not started. Close the old process if DLL files are locked.'}
$assembly=Join-Path $root 'bin/Debug/net10.0/Vtb.Hr.Web.dll'
Write-Host "Starting current build: $assembly"
Write-Host 'Open http://localhost:5082. Use Windows account ACER\admin.'
dotnet $assembly --contentRoot $root --urls 'http://localhost:5082'
if($LASTEXITCODE -ne 0){throw 'Website stopped with an error. Read the message above.'}
