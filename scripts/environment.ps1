param()
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.cli'
$env:NUGET_PACKAGES = Join-Path (Get-Location) '.nuget/packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path (Get-Location) '.nuget/http-cache'
$env:NUGET_SCRATCH = Join-Path (Get-Location) '.nuget/scratch'
$env:TEMP = $env:NUGET_SCRATCH
$env:TMP = $env:TEMP
New-Item $env:TEMP -ItemType Directory -Force | Out-Null
