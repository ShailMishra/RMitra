<#
.SYNOPSIS
  Signs in via device code (ARM REST), then creates RasoiMitra Azure SQL and installs schema.
#>
[CmdletBinding()]
param(
    [string]$ResourceGroup = "rg-rasoimitra",
    [string]$Location = "centralindia",
    [string]$SqlServerName = "rasoimitra-sql",
    [string]$DatabaseName = "RasoiMitra",
    [string]$SqlAdminUser = "rasoimitraadmin",
    [string]$ServiceObjective = "Basic"
)

$ErrorActionPreference = "Stop"
# Azure PowerShell public client — personal Microsoft accounts can use this.
# Azure CLI device login rejects Hotmail/personal accounts.
$clientId = "1950a258-227b-4e31-a9cf-717495945fc2"
$arm = "https://management.azure.com"

function Invoke-Arm {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Url,
        [Parameter(Mandatory = $true)][string]$AccessToken,
        $Body = $null,
        [int]$TimeoutSec = 120
    )
    $headers = @{ Authorization = "Bearer $AccessToken"; "Content-Type" = "application/json" }
    $json = if ($null -ne $Body) { $Body | ConvertTo-Json -Depth 8 } else { $null }
    return Invoke-RestMethod -Method $Method -Uri $Url -Headers $headers -Body $json -TimeoutSec $TimeoutSec
}

function Wait-ArmOperation {
    param(
        [Parameter(Mandatory = $true)][string]$Url,
        [Parameter(Mandatory = $true)][string]$AccessToken,
        [int]$TimeoutSeconds = 600
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        Start-Sleep -Seconds 5
        $headers = @{ Authorization = "Bearer $AccessToken" }
        try {
            $resp = Invoke-WebRequest -Method GET -Uri $Url -Headers $headers -TimeoutSec 60
            if ([int]$resp.StatusCode -eq 200) { return }
        } catch {
            if ((Get-Date) -gt $deadline) { throw }
        }
    } while ((Get-Date) -lt $deadline)
    throw "Timed out waiting for Azure operation: $Url"
}

function Get-DeviceToken {
    param([string]$Tenant = "common")
    $device = Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$Tenant/oauth2/devicecode" -ContentType "application/x-www-form-urlencoded" -Body @{
        client_id = $clientId
        resource  = "https://management.core.windows.net/"
    }
    Write-Host "VERIFY_URL=$($device.verification_url)"
    Write-Host "USER_CODE=$($device.user_code)"
    Write-Host $device.message

    $deadline = (Get-Date).AddSeconds([Math]::Max(60, [int]$device.expires_in))
    $interval = [Math]::Max(5, [int]$device.interval)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds $interval
        try {
            return Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$Tenant/oauth2/token" -ContentType "application/x-www-form-urlencoded" -Body @{
                grant_type = "device_code"
                client_id  = $clientId
                code       = $device.device_code
                resource   = "https://management.core.windows.net/"
            }
        } catch {
            $err = $_.ErrorDetails.Message
            if ($err -notmatch "authorization_pending|slow_down") {
                throw
            }
        }
    }
    throw "Device login timed out."
}

function Get-Subscriptions {
    param([string]$AccessToken)
    $found = @()
    $subs = Invoke-Arm -Method GET -Url "$arm/subscriptions?api-version=2022-12-01" -AccessToken $AccessToken
    foreach ($s in @($subs.value)) {
        Write-Host "SUBSCRIPTION_SEEN name=$($s.displayName) state=$($s.state)"
        $found += [pscustomobject]@{ Sub = $s; Token = $AccessToken }
    }
    return $found
}

Write-Host "REQUESTING_DEVICE_CODE"
$token = Get-DeviceToken -Tenant "common"
Write-Host "ARM_TOKEN_OK"
$accessToken = $token.access_token
$refreshToken = $token.refresh_token

$candidates = @(Get-Subscriptions -AccessToken $accessToken)
$tenantIds = @()
try {
    $tenantList = Invoke-Arm -Method GET -Url "$arm/tenants?api-version=2020-01-01" -AccessToken $accessToken
    foreach ($tenant in @($tenantList.value)) {
        $tenantIds += $tenant.tenantId
        Write-Host "TENANT_SEEN=$($tenant.tenantId)"
        if (-not $refreshToken) { continue }
        foreach ($tokenUrl in @(
            "https://login.microsoftonline.com/$($tenant.tenantId)/oauth2/token",
            "https://login.microsoftonline.com/$($tenant.tenantId)/oauth2/v2.0/token"
        )) {
            try {
                $body = if ($tokenUrl -match "v2.0") {
                    @{ grant_type = "refresh_token"; client_id = $clientId; refresh_token = $refreshToken; scope = "https://management.azure.com/.default" }
                } else {
                    @{ grant_type = "refresh_token"; client_id = $clientId; refresh_token = $refreshToken; resource = "https://management.core.windows.net/" }
                }
                $tenantToken = Invoke-RestMethod -Method Post -Uri $tokenUrl -ContentType "application/x-www-form-urlencoded" -Body $body
                $candidates += @(Get-Subscriptions -AccessToken $tenantToken.access_token)
                break
            } catch {
                Write-Host "TENANT_TOKEN_SKIPPED=$($tenant.tenantId)"
            }
        }
    }
} catch {
    Write-Host "TENANT_LIST_SKIPPED"
}

if (-not $candidates -or $candidates.Count -eq 0) {
    foreach ($tenantId in $tenantIds) {
        Write-Host "REQUESTING_TENANT_DEVICE_CODE=$tenantId"
        try {
            $tenantToken = Get-DeviceToken -Tenant $tenantId
            $candidates += @(Get-Subscriptions -AccessToken $tenantToken.access_token)
            if ($candidates.Count -gt 0) { break }
        } catch {
            Write-Host "TENANT_DEVICE_LOGIN_SKIPPED=$tenantId"
        }
    }
}

$chosen = $candidates | Where-Object { $_.Sub.state -eq "Enabled" } | Select-Object -First 1
if (-not $chosen) { $chosen = $candidates | Select-Object -First 1 }
if (-not $chosen) {
    throw "No Azure subscription was found for this Hotmail account. In the portal open Subscriptions and create a Free or Pay-As-You-Go subscription, then rerun."
}

$sub = $chosen.Sub
$accessToken = $chosen.Token
$subId = $sub.subscriptionId
Write-Host "SUBSCRIPTION_OK state=$($sub.state)"

$rgUrl = "$arm/subscriptions/$subId/resourceGroups/${ResourceGroup}?api-version=2021-04-01"
Invoke-Arm -Method PUT -Url $rgUrl -AccessToken $accessToken -Body @{ location = $Location } | Out-Null
Write-Host "RESOURCE_GROUP_OK"

$serverUrl = "$arm/subscriptions/$subId/resourceGroups/$ResourceGroup/providers/Microsoft.Sql/servers/${SqlServerName}?api-version=2021-11-01"
$existingServer = $null
try { $existingServer = Invoke-Arm -Method GET -Url $serverUrl -AccessToken $accessToken } catch { }

$sqlPassword = "Rm#Sql-" + ([guid]::NewGuid().ToString("N").Substring(0, 12)) + "Aa1!"
if ($existingServer) {
    Write-Host "SQL_SERVER_EXISTS"
    if (-not $env:RM_SQL_ADMIN_PASSWORD) {
        throw "SQL server already exists. Re-run with env RM_SQL_ADMIN_PASSWORD set to the existing SQL admin password."
    }
    $sqlPassword = $env:RM_SQL_ADMIN_PASSWORD
} else {
    Write-Host "CREATING_SQL_SERVER"
    try {
        Invoke-Arm -Method PUT -Url $serverUrl -AccessToken $accessToken -Body @{
            location   = $Location
            properties = @{
                administratorLogin         = $SqlAdminUser
                administratorLoginPassword = $sqlPassword
                version                    = "12.0"
                publicNetworkAccess        = "Enabled"
                minimalTlsVersion          = "1.2"
            }
        } | Out-Null
    } catch {
        $SqlServerName = "rasoimitra-sql-" + ([guid]::NewGuid().ToString("N").Substring(0, 8))
        Write-Host "RETRY_SERVER_NAME=$SqlServerName"
        $serverUrl = "$arm/subscriptions/$subId/resourceGroups/$ResourceGroup/providers/Microsoft.Sql/servers/${SqlServerName}?api-version=2021-11-01"
        Invoke-Arm -Method PUT -Url $serverUrl -AccessToken $accessToken -Body @{
            location   = $Location
            properties = @{
                administratorLogin         = $SqlAdminUser
                administratorLoginPassword = $sqlPassword
                version                    = "12.0"
                publicNetworkAccess        = "Enabled"
                minimalTlsVersion          = "1.2"
            }
        } | Out-Null
    }
    Wait-ArmOperation -Url $serverUrl -AccessToken $accessToken
    Write-Host "SQL_SERVER_OK"
}

$fwAzure = "$arm/subscriptions/$subId/resourceGroups/$ResourceGroup/providers/Microsoft.Sql/servers/$SqlServerName/firewallRules/AllowAzureServices?api-version=2021-11-01"
Invoke-Arm -Method PUT -Url $fwAzure -AccessToken $accessToken -Body @{
    properties = @{ startIpAddress = "0.0.0.0"; endIpAddress = "0.0.0.0" }
} | Out-Null
try {
    $myIp = (Invoke-RestMethod -Uri "https://api.ipify.org?format=json").ip
    $fwMe = "$arm/subscriptions/$subId/resourceGroups/$ResourceGroup/providers/Microsoft.Sql/servers/$SqlServerName/firewallRules/AllowMyIp?api-version=2021-11-01"
    Invoke-Arm -Method PUT -Url $fwMe -AccessToken $accessToken -Body @{
        properties = @{ startIpAddress = $myIp; endIpAddress = $myIp }
    } | Out-Null
    Write-Host "FIREWALL_OK"
} catch {
    Write-Host "FIREWALL_IP_SKIPPED"
}

$dbUrl = "$arm/subscriptions/$subId/resourceGroups/$ResourceGroup/providers/Microsoft.Sql/servers/$SqlServerName/databases/${DatabaseName}?api-version=2021-11-01"
$existingDb = $null
try { $existingDb = Invoke-Arm -Method GET -Url $dbUrl -AccessToken $accessToken } catch { }
if (-not $existingDb) {
    Write-Host "CREATING_DATABASE"
    Invoke-Arm -Method PUT -Url $dbUrl -AccessToken $accessToken -Body @{
        location = $Location
        sku      = @{ name = $ServiceObjective; tier = "Basic" }
    } | Out-Null
    Wait-ArmOperation -Url $dbUrl -AccessToken $accessToken
}
Write-Host "DATABASE_OK"

$serverFqdn = "$SqlServerName.database.windows.net"
Push-Location $PSScriptRoot
try {
    Write-Host "INSTALLING_SCHEMA"
    $env:SQLCMDPASSWORD = $sqlPassword
    & sqlcmd -S "tcp:$serverFqdn,1433" -d $DatabaseName -U $SqlAdminUser -P $sqlPassword -I -i "Install_All.sql"
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
}
finally {
    Remove-Item Env:SQLCMDPASSWORD -ErrorAction SilentlyContinue
    Pop-Location
}
Write-Host "SCHEMA_OK"

$connectionString = "Server=tcp:$serverFqdn,1433;Initial Catalog=$DatabaseName;User ID=$SqlAdminUser;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;"
$project = Join-Path $PSScriptRoot "..\RM-Backend-API\RM-Backend-API.csproj"
dotnet user-secrets set "ConnectionStrings:ConnectionString" $connectionString --project $project | Out-Null
Write-Host "USER_SECRETS_OK"
Write-Host "SERVER_FQDN=$serverFqdn"
Write-Host "DATABASE=$DatabaseName"
Write-Host "SQL_ADMIN_USER=$SqlAdminUser"
Write-Host "DEPLOY_COMPLETE"
