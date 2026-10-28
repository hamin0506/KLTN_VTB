$ErrorActionPreference='Stop'
. "$PSScriptRoot/environment.ps1"
$root=Split-Path $PSScriptRoot
# Build only; never create or modify a database. Isolated TestServer has test identities only.
dotnet restore "$root/Vtb.Hr.Web.csproj" --configfile "$root/NuGet.Config"
if($LASTEXITCODE -ne 0){throw 'Restore failed.'}
dotnet build "$root/Vtb.Hr.Web.csproj" --no-restore --output "$root/.scaffold/final-review"
if($LASTEXITCODE -ne 0){throw 'Website build failed.'}
dotnet restore "$root/tests/Verification/Verification.csproj" --configfile "$root/NuGet.Config"
if($LASTEXITCODE -ne 0){throw 'Verification restore failed.'}
dotnet run --project "$root/tests/Verification/Verification.csproj" --no-restore
if($LASTEXITCODE -ne 0){throw 'Verification failed. Inspect output above.'}