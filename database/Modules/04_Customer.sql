USE [HomelyFood];
GO

IF OBJECT_ID(N'cusAddresses', N'U') IS NULL
BEGIN
    CREATE TABLE cusAddresses
    (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Addresses PRIMARY KEY,
        AddressId   NVARCHAR(20)     NOT NULL,
        UserId      UNIQUEIDENTIFIER NOT NULL,
        Label       NVARCHAR(50)     NOT NULL,
        Line1       NVARCHAR(500)    NOT NULL,
        Line2       NVARCHAR(500)    NULL,
        City        NVARCHAR(100)    NOT NULL,
        State       NVARCHAR(100)    NOT NULL,
        Pincode     NVARCHAR(10)     NOT NULL,
        Latitude    DECIMAL(9,6)     NOT NULL,
        Longitude   DECIMAL(9,6)     NOT NULL,
        IsDefault   BIT              NOT NULL CONSTRAINT DF_Addresses_Default DEFAULT (0),
        CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Addresses_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Addresses_AddressId UNIQUE (AddressId),
        CONSTRAINT FK_Addresses_User FOREIGN KEY (UserId) REFERENCES mstUsers (Id)
    );
END
GO
