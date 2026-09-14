USE [HomelyFood];
GO

IF OBJECT_ID(N'mstCuisines', N'U') IS NULL
BEGIN
    CREATE TABLE mstCuisines
    (
        Id      INT IDENTITY(1,1) NOT NULL,
        Code    VARCHAR(200)      NOT NULL,
        Name    VARCHAR(200)      NOT NULL
    );

    CREATE INDEX IX_Cuisines ON mstCuisines (Code, Name DESC);
END
GO

IF OBJECT_ID(N'catMenuCategories', N'U') IS NULL
BEGIN
    CREATE TABLE catMenuCategories
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuCategories PRIMARY KEY,
        KitchenGuid     UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(150)    NOT NULL,
        DisplayOrder    INT              NOT NULL CONSTRAINT DF_MenuCategories_Order DEFAULT (0),
        IsActive        BIT              NOT NULL CONSTRAINT DF_MenuCategories_Active DEFAULT (1),
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_MenuCategories_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_MenuCategories_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES kitKitchens (Id)
    );
END
GO

IF OBJECT_ID(N'catMenuItems', N'U') IS NULL
BEGIN
    CREATE TABLE catMenuItems
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY,
        ItemId              NVARCHAR(20)     NOT NULL,
        KitchenGuid         UNIQUEIDENTIFIER NOT NULL,
        CategoryId          UNIQUEIDENTIFIER NOT NULL,
        Name                NVARCHAR(200)    NOT NULL,
        Description         NVARCHAR(1000)   NULL,
        Price               DECIMAL(10,2)    NOT NULL,
        IsVeg               BIT              NOT NULL CONSTRAINT DF_MenuItems_IsVeg DEFAULT (1),
        Status              NVARCHAR(20)     NOT NULL CONSTRAINT DF_MenuItems_Status DEFAULT (N'DRAFT'),
        PhotoUrl            NVARCHAR(1000)   NULL,
        PreparationMinutes  INT              NOT NULL CONSTRAINT DF_MenuItems_Prep DEFAULT (20),
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_MenuItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_MenuItems_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_MenuItems_ItemId UNIQUE (ItemId),
        CONSTRAINT FK_MenuItems_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES kitKitchens (Id),
        CONSTRAINT FK_MenuItems_Category FOREIGN KEY (CategoryId) REFERENCES catMenuCategories (Id)
    );
END
GO
