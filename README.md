# RasoiMitra Backend API

ASP.NET Core 8 API for RasoiMitra. Public hosting is set up for [Render](https://render.com) from this GitHub repo: [ShailMishra/RMitra](https://github.com/ShailMishra/RMitra).

## Live URLs after deploy

- Swagger: `https://rmitra.onrender.com/swagger`
- Health: `https://rmitra.onrender.com/health`
- Ping: `https://rmitra.onrender.com/api/rasoi-mitra/ping`

Swagger has two docs:

- **HOMELY API** — `/api/homely/*` (auth, kitchen, customer, orders, rider, payments)
- **RasoiMitra Backend API** — `/api/rasoi-mitra/*` (Excel file DB at `App_Data/RasoiMitra.xlsx`)

Homely SQL schema: run `database/Install_All.sql` (local) or `database/Deploy-AzureSql.ps1` (Azure) before using `/api/homely` routes.

The API can start without SQL. Old RasoiMitra kitchen APIs use the Excel file `App_Data/RasoiMitra.xlsx`. Homely APIs need the `RasoiMitra` database.

## Connect Azure SQL

Do **not** use your Azure Portal Microsoft account password as the SQL password, and do not commit connection strings.

1. Sign in to Azure yourself (Azure CLI **or** PowerShell). The portal email/password is not a SQL login.

```powershell
az login
# or, if Azure CLI is not installed:
Connect-AzAccount
```

2. Create the server, `RasoiMitra` database, firewall rules, tables (`tbl*`), masters (`mas*`), and procedures (`usp*`) :

```powershell
cd database
.\Deploy-AzureSql.ps1 -SqlAdminUser rasoimitraadmin -SqlAdminPassword 'Choose-a-strong-SQL-password'
```

`Deploy-AzureSql.ps1` uses Azure CLI when `az` is on PATH, otherwise the `Az` PowerShell modules.

3. Set the connection string locally (user secrets, not source control):

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=RasoiMitra;User ID=YOUR_USER;Password=YOUR_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;" --project RM-Backend-API
```

On Render, set `ConnectionStrings__ConnectionString` and allow the app to reach Azure SQL:

1. Azure Portal → SQL server → **Networking**
2. Enable public access (or add Render outbound IPs)
3. Add a firewall rule, or allow Azure services

Then restart the Render service. `/ready` and `/api/rasoi-mitra/ping` should report the database as connected.

Local Windows SQL Server:

```powershell
cd database
.\Install-Local.ps1
```

`appsettings.Development.json` already points at LocalDB `RasoiMitra`.

## Deploy on Render

1. Push this repo to GitHub (`main` branch).
2. Open [Render Dashboard](https://dashboard.render.com) and sign in with GitHub.
3. Click **New +** → **Blueprint**.
4. Select `ShailMishra/RMitra`.
5. Apply `render.yaml` (web service `rasoimitra-api`, Docker, Singapore).

Or create a **Web Service** manually:

- Runtime: Docker
- Branch: `main`
- Health check path: `/health`
- Region: Singapore

First deploy can take a few minutes. Render's starter instance sleeps on the free/idle plans, so the first request after idle can be slow.

## Local run

```bash
dotnet run --project RM-Backend-API/RM-Backend-API.csproj
```

Swagger: http://localhost:5041/swagger

Set a local SQL connection string in `RM-Backend-API/appsettings.Development.json` or with:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "Server=(localdb)\\MSSQLLocalDB;Database=RasoiMitra;Trusted_Connection=True;TrustServerCertificate=True;" --project RM-Backend-API
```
