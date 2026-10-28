$ErrorActionPreference='Stop'
# Uses Windows authentication. Only SELECT statements in the supplied SQL file.
& sqlcmd -S 'ACER\SQLEXPRESS' -E -d VTB2 -b -l 10 -i "$PSScriptRoot/verify-readonly.sql"
if($LASTEXITCODE -ne 0){throw 'Read-only SQL verification failed. No database data was modified.'}