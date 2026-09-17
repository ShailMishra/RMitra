<#
.SYNOPSIS
  Installs RasoiMitra tbl/mas/usp schema into an existing Azure SQL database
  and stores the connection string in .NET user secrets.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ServerFqdn,
    [string]$DatabaseName = "RasoiMitra",
    [string]$SqlAdminUser = "rasoimitraadmin",
    [Parameter(Mandatory = $true)][string]$SqlAdminPassword
)

$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    Write-Host "Installing schema on $ServerFqdn / $DatabaseName"
    & sqlcmd -S "tcp:$ServerFqdn,1433" -d $DatabaseName -U $SqlAdminUser -P $SqlAdminPassword -I -i "Install_All.sql"
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
}
finally {
    Pop-Location
}

$connectionString = "Server=tcp:$ServerFqdn,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;"
$project = Join-Path $PSScriptRoot "..\RM-Backend-API\RM-Backend-API.csproj"
dotnet user-secrets set "ConnectionStrings:ConnectionString" $connectionString --project $project | Out-Null
Write-Host "SCHEMA_OK"
Write-Host "USER_SECRETS_OK"
