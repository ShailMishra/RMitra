/*
    RasoiMitra dedicated database.
    Local:  sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -i Install_All.sql
    Azure:  create the database with Deploy-AzureSql.ps1 (Azure SQL cannot CREATE DATABASE from T-SQL),
            then install schema:
            sqlcmd -S tcp:SERVER.database.windows.net,1433 -d RasoiMitra -U USER -P PASSWORD -I -i Install_All.sql

    EngineEdition 5 = Azure SQL Database.
*/

IF SERVERPROPERTY('EngineEdition') <> 5 AND DB_ID(N'RasoiMitra') IS NULL
BEGIN
    CREATE DATABASE [RasoiMitra];
END
GO

IF SERVERPROPERTY('EngineEdition') <> 5
BEGIN
    USE [RasoiMitra];
END
GO
