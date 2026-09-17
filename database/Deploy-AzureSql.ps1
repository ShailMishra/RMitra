<#
.SYNOPSIS
  Creates Azure SQL resources for RasoiMitra and installs tables, masters, and stored procedures.

.DESCRIPTION
  Sign in to Azure yourself first (Connect-AzAccount or az login).
  This script does not accept or store an Azure Portal Microsoft-account password.
  SQL admin credentials are separate from the portal account.

.EXAMPLE
  Connect-AzAccount
  .\Deploy-AzureSql.ps1 -SqlAdminUser rasoimitraadmin -SqlAdminPassword 'Use-a-strong-password'
#>
[CmdletBinding()]
param(
    [string]$ResourceGroup = "rg-rasoimitra",
    [string]$Location = "centralindia",
    [string]$SqlServerName = "rasoimitra-sql",
    [string]$DatabaseName = "RasoiMitra",
    [string]$SqlAdminUser = "rasoimitraadmin",
    [Parameter(Mandatory = $true)]
    [string]$SqlAdminPassword,
    [string]$ServiceObjective = "Basic"
)

$ErrorActionPreference = "Stop"

function Test-AzCli {
    return [bool](Get-Command az -ErrorAction SilentlyContinue)
}

function Get-PublicIp {
    try {
        return (Invoke-RestMethod -Uri "https://api.ipify.org?format=json").ip
    } catch {
        Write-Warning "Could not detect public IP. Add a firewall rule in Azure Portal > SQL server > Networking."
        return $null
    }
}

function Invoke-SchemaInstall {
    param(
        [string]$ServerFqdn,
        [string]$DatabaseName,
        [string]$SqlAdminUser,
        [string]$SqlAdminPassword
    )

    $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if (-not $sqlcmd) {
        throw "sqlcmd is not installed. Install SQL Server Command Line Tools, then rerun this script."
    }

    $installer = Join-Path $PSScriptRoot "Install_All.sql"
    Write-Host "Installing schema from $installer"
    Push-Location $PSScriptRoot
    try {
        & sqlcmd -S "tcp:$ServerFqdn,1433" -d $DatabaseName -U $SqlAdminUser -P $SqlAdminPassword -I -i "Install_All.sql"
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
    }
    finally {
        Pop-Location
    }
}

function Write-ConnectionHelp {
    param([string]$ConnectionString)
    $project = Join-Path $PSScriptRoot "..\RM-Backend-API\RM-Backend-API.csproj"
    Write-Host ""
    Write-Host "Azure SQL is ready. Set the API connection string (do not commit it):"
    Write-Host "  dotnet user-secrets set `"ConnectionStrings:ConnectionString`" `"$ConnectionString`" --project `"$project`""
    Write-Host ""
    Write-Host "On Render, set ConnectionStrings__ConnectionString to the same value and allow Render outbound IPs on the SQL server firewall."
}

if (Test-AzCli) {
    $account = az account show 2>$null | ConvertFrom-Json
    if (-not $account) {
        throw "Not signed in. Run 'az login' in this terminal, then rerun this script. Do not pass your Azure Portal password here."
    }

    Write-Host "Using Azure CLI subscription: $($account.name)"
    Write-Host "Creating resource group $ResourceGroup in $Location"
    az group create --name $ResourceGroup --location $Location | Out-Null

    $serverExists = az sql server show --name $SqlServerName --resource-group $ResourceGroup 2>$null
    if (-not $serverExists) {
        Write-Host "Creating SQL server $SqlServerName"
        az sql server create --name $SqlServerName --resource-group $ResourceGroup --location $Location --admin-user $SqlAdminUser --admin-password $SqlAdminPassword | Out-Null
    } else {
        Write-Host "SQL server $SqlServerName already exists"
    }

    Write-Host "Allowing Azure services and your current public IP"
    az sql server firewall-rule create --resource-group $ResourceGroup --server $SqlServerName --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 2>$null | Out-Null
    $myIp = Get-PublicIp
    if ($myIp) {
        az sql server firewall-rule create --resource-group $ResourceGroup --server $SqlServerName --name AllowMyIp --start-ip-address $myIp --end-ip-address $myIp 2>$null | Out-Null
    }

    $dbExists = az sql db show --resource-group $ResourceGroup --server $SqlServerName --name $DatabaseName 2>$null
    if (-not $dbExists) {
        Write-Host "Creating database $DatabaseName"
        az sql db create --resource-group $ResourceGroup --server $SqlServerName --name $DatabaseName --service-objective $ServiceObjective | Out-Null
    } else {
        Write-Host "Database $DatabaseName already exists"
    }
} else {
    Import-Module Az.Accounts -ErrorAction Stop
    Import-Module Az.Resources -ErrorAction Stop
    Import-Module Az.Sql -ErrorAction Stop

    $ctx = Get-AzContext
    if (-not $ctx) {
        throw "Not signed in. Run 'Connect-AzAccount' in this terminal, then rerun this script. Do not pass your Azure Portal password here."
    }

    Write-Host "Using Azure PowerShell subscription: $($ctx.Subscription.Name)"
    Write-Host "Creating resource group $ResourceGroup in $Location"
    New-AzResourceGroup -Name $ResourceGroup -Location $Location -Force | Out-Null

    $server = Get-AzSqlServer -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -ErrorAction SilentlyContinue
    if (-not $server) {
        Write-Host "Creating SQL server $SqlServerName"
        $sec = ConvertTo-SecureString $SqlAdminPassword -AsPlainText -Force
        $sqlCred = New-Object System.Management.Automation.PSCredential ($SqlAdminUser, $sec)
        New-AzSqlServer -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -Location $Location -SqlAdministratorCredentials $sqlCred | Out-Null
    } else {
        Write-Host "SQL server $SqlServerName already exists"
    }

    Write-Host "Allowing Azure services and your current public IP"
    if (-not (Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -FirewallRuleName AllowAzureServices -ErrorAction SilentlyContinue)) {
        New-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -FirewallRuleName AllowAzureServices -StartIpAddress "0.0.0.0" -EndIpAddress "0.0.0.0" | Out-Null
    }
    $myIp = Get-PublicIp
    if ($myIp) {
        $existingIp = Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -FirewallRuleName AllowMyIp -ErrorAction SilentlyContinue
        if ($existingIp) {
            Set-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -FirewallRuleName AllowMyIp -StartIpAddress $myIp -EndIpAddress $myIp | Out-Null
        } else {
            New-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -FirewallRuleName AllowMyIp -StartIpAddress $myIp -EndIpAddress $myIp | Out-Null
        }
    }

    $db = Get-AzSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -DatabaseName $DatabaseName -ErrorAction SilentlyContinue
    if (-not $db) {
        Write-Host "Creating database $DatabaseName"
        New-AzSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $SqlServerName -DatabaseName $DatabaseName -RequestedServiceObjectiveName $ServiceObjective | Out-Null
    } else {
        Write-Host "Database $DatabaseName already exists"
    }
}

$serverFqdn = "$SqlServerName.database.windows.net"
Invoke-SchemaInstall -ServerFqdn $serverFqdn -DatabaseName $DatabaseName -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword

$connectionString = "Server=tcp:$serverFqdn,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;"
Write-ConnectionHelp -ConnectionString $connectionString
