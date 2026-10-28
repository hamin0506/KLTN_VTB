. "$PSScriptRoot/environment.ps1"
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet tool install dotnet-ef --version 10.0.13 --tool-path .tools --configfile NuGet.Config
if (!(Test-Path '.tools/dotnet-ef.exe')) { throw 'Không cài đặt được dotnet-ef.' }
# Read-only reverse engineering. No migrations or database update.
& ./.tools/dotnet-ef.exe dbcontext scaffold 'Name=ConnectionStrings:VTB2' Microsoft.EntityFrameworkCore.SqlServer --context VtbScaffoldContext --context-dir .scaffold/Data --output-dir .scaffold/Entities --namespace Vtb.Hr.Web.Scaffold.Entities --context-namespace Vtb.Hr.Web.Scaffold.Data --no-onconfiguring --force
if ($LASTEXITCODE -ne 0) { throw 'Scaffold thất bại. Kiểm tra kết nối và quyền đọc metadata.' }

