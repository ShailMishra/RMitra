# RasoiMitra Backend API

ASP.NET Core 8 API for RasoiMitra. Public hosting is set up for [Render](https://render.com) from this GitHub repo: [ShailMishra/RMitra](https://github.com/ShailMishra/RMitra).

## Live URLs after deploy

- Swagger: `https://rmitra.onrender.com/swagger`
- Health: `https://rmitra.onrender.com/health`
- Ping: `https://rmitra.onrender.com/api/rasoi-mitra/ping`

Swagger has two docs:

- **HOMELY API** — `/api/homely/*` (auth, kitchen, customer, orders, rider, payments)
- **RasoiMitra Backend API** — `/api/rasoi-mitra/*` (Excel file DB at `App_Data/RasoiMitra.xlsx`)

Homely SQL schema: run `database/Install_All.sql` on Azure SQL before using `/api/homely` routes.

The API starts without Azure SQL. Old RasoiMitra kitchen APIs use the Excel file `App_Data/RasoiMitra.xlsx` inside the app. Homely APIs still need Azure SQL later.

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

## Connect Azure SQL later

In the Render service, add:

| Key | Example |
| --- | --- |
| `ConnectionStrings__ConnectionString` | `Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DB;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;` |

Also allow Render to reach Azure SQL:

1. Azure Portal → SQL server → **Networking**
2. Enable public access (or add Render outbound IPs)
3. Add firewall rule, or temporarily allow Azure services

Then restart the Render service. `/ready` and `/api/rasoi-mitra/ping` should report the database as connected.

## Local run

```bash
dotnet run --project RM-Backend-API/RM-Backend-API.csproj
```

Swagger: http://localhost:5041/swagger

Set a local SQL connection string in `RM-Backend-API/appsettings.Development.json` or with:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "Server=localhost;Database=TestDB;..." --project RM-Backend-API
```
