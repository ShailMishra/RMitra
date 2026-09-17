<#
.SYNOPSIS
  Creates the local RasoiMitra database and installs tbl/mas/usp objects.
#>
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -i "Install_All.sql"
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
    Write-Host "LocalDB database RasoiMitra is ready."
}
finally {
    Pop-Location
}
