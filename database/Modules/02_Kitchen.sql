USE [HomelyFood];
GO

IF OBJECT_ID(N'kitKitchens', N'U') IS NULL
BEGIN
    CREATE TABLE kitKitchens
    (
        Id                          UNIQUEIDENTIFIER    NOT NULL CONSTRAINT PK_Kitchens PRIMARY KEY,
        KitchenId                   NVARCHAR(20)        NOT NULL,
        OwnerUserId                 UNIQUEIDENTIFIER    NOT NULL,
        KitchenName                 NVARCHAR(200)       NOT NULL,
        OwnerName                   NVARCHAR(200)       NOT NULL,
        MobileNumber                NVARCHAR(15)        NOT NULL,
        Email                       NVARCHAR(200)       NOT NULL,
        KitchenType                 NVARCHAR(50)        NOT NULL CONSTRAINT DF_Kitchens_Type DEFAULT (N'HOMEMADE'),
        AddressLine1                NVARCHAR(500)       NULL,
        AddressLine2                NVARCHAR(500)       NULL,
        City                        NVARCHAR(100)       NULL,
        State                       NVARCHAR(100)       NULL,
        Pincode                     NVARCHAR(10)        NULL,
        Latitude                    DECIMAL(9,6)        NULL,
        Longitude                   DECIMAL(9,6)        NULL,
        CuisineTypes                NVARCHAR(MAX)       NULL,
        OpenTime                    NVARCHAR(10)        NULL,
        CloseTime                   NVARCHAR(10)        NULL,
        DeliveryRadiusKm            INT                 NOT NULL CONSTRAINT DF_Kitchens_Radius DEFAULT (10),
        AccountHolderName           NVARCHAR(200)       NULL,
        AccountNumber               NVARCHAR(50)        NULL,
        IfscCode                    NVARCHAR(20)        NULL,
        AccountType                 NVARCHAR(20)        NULL,
        Status                      NVARCHAR(50)        NOT NULL,
        ListingEnabled              BIT                 NOT NULL CONSTRAINT DF_Kitchens_Listing DEFAULT (0),
        RejectionReason             NVARCHAR(1000)      NULL,
        AdditionalDocumentsRequired NVARCHAR(MAX)       NULL,
        CreatedAt                   DATETIME2           NOT NULL CONSTRAINT DF_Kitchens_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt                   DATETIME2           NOT NULL CONSTRAINT DF_Kitchens_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        SubmittedAt                 DATETIME2           NULL,
        ApprovedAt                  DATETIME2           NULL,
        ActivatedAt                 DATETIME2           NULL,
        CONSTRAINT UQ_Kitchens_KitchenId UNIQUE (KitchenId),
        CONSTRAINT UQ_Kitchens_Mobile UNIQUE (MobileNumber),
        CONSTRAINT FK_Kitchens_Owner FOREIGN KEY (OwnerUserId) REFERENCES mstUsers (Id)
    );

    CREATE INDEX IX_Kitchens_Status ON kitKitchens (Status);
END
GO

IF OBJECT_ID(N'kitDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE kitDocuments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_KitchenDocuments PRIMARY KEY,
        KitchenGuid     UNIQUEIDENTIFIER NOT NULL,
        DocumentType    NVARCHAR(50)     NOT NULL,
        FileUrl         NVARCHAR(1000)   NOT NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_KitchenDocs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_KitchenDocs_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES kitKitchens (Id)
    );
END
GO

IF OBJECT_ID(N'kitKitchenStatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE kitKitchenStatusHistory
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KitchenStatusHistory PRIMARY KEY,
        KitchenGuid UNIQUEIDENTIFIER     NOT NULL,
        Status      NVARCHAR(50)         NOT NULL,
        Remarks     NVARCHAR(1000)       NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_KitchenHistory_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_KitchenHistory_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES kitKitchens (Id)
    );
END
GO
