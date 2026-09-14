/*
    HOMELY dedicated database (local SQL Server).
    Run against the SQL Server instance, not an existing application DB.
*/

IF DB_ID(N'HomelyFood') IS NULL
BEGIN
    CREATE DATABASE [HomelyFood];
END
GO

USE [HomelyFood];
GO

IF OBJECT_ID(N'core.NumberSeries', N'U') IS NULL
BEGIN
    CREATE TABLE core.NumberSeries
    (
        Prefix      NVARCHAR(10) NOT NULL CONSTRAINT PK_NumberSeries PRIMARY KEY,
        LastNumber  INT          NOT NULL
    );
END
GO
