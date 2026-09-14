USE [HomelyFood];
GO

IF OBJECT_ID(N'delRiders', N'U') IS NULL
BEGIN
    CREATE TABLE delRiders
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Riders PRIMARY KEY,
        RiderId             NVARCHAR(20)     NOT NULL,
        UserId              UNIQUEIDENTIFIER NOT NULL,
        FullName            NVARCHAR(200)    NOT NULL,
        MobileNumber        NVARCHAR(15)     NOT NULL,
        VehicleType         NVARCHAR(20)     NOT NULL,
        Status              NVARCHAR(20)     NOT NULL,
        Available           BIT              NOT NULL CONSTRAINT DF_Riders_Available DEFAULT (0),
        Latitude            DECIMAL(9,6)     NULL,
        Longitude           DECIMAL(9,6)     NULL,
        AccountHolderName   NVARCHAR(200)    NULL,
        AccountNumber       NVARCHAR(50)     NULL,
        IfscCode            NVARCHAR(20)     NULL,
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_Riders_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_Riders_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Riders_RiderId UNIQUE (RiderId),
        CONSTRAINT UQ_Riders_UserId UNIQUE (UserId),
        CONSTRAINT FK_Riders_User FOREIGN KEY (UserId) REFERENCES mstUsers (Id)
    );
END
GO

IF OBJECT_ID(N'delRiderDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE delRiderDocuments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RiderDocuments PRIMARY KEY,
        RiderGuid       UNIQUEIDENTIFIER NOT NULL,
        DocumentType    NVARCHAR(50)     NOT NULL,
        FileUrl         NVARCHAR(1000)   NOT NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_RiderDocs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_RiderDocs_Rider FOREIGN KEY (RiderGuid) REFERENCES delRiders (Id)
    );
END
GO

IF OBJECT_ID(N'delAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE delAssignments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Assignments PRIMARY KEY,
        AssignmentId    NVARCHAR(20)     NOT NULL,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        RiderGuid       UNIQUEIDENTIFIER NOT NULL,
        Status          NVARCHAR(20)     NOT NULL,
        ExpiresAt       DATETIME2        NOT NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Assignments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Assignments_AssignmentId UNIQUE (AssignmentId),
        CONSTRAINT FK_Assignments_Order FOREIGN KEY (OrderGuid) REFERENCES ordOrders (Id),
        CONSTRAINT FK_Assignments_Rider FOREIGN KEY (RiderGuid) REFERENCES delRiders (Id)
    );
END
GO

IF OBJECT_ID(N'delDeliveries', N'U') IS NULL
BEGIN
    CREATE TABLE delDeliveries
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Deliveries PRIMARY KEY,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        RiderGuid       UNIQUEIDENTIFIER NULL,
        DeliveryMode    NVARCHAR(20)     NOT NULL,
        Status          NVARCHAR(30)     NOT NULL,
        Latitude        DECIMAL(9,6)     NULL,
        Longitude       DECIMAL(9,6)     NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Deliveries_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Deliveries_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Deliveries_OrderGuid UNIQUE (OrderGuid),
        CONSTRAINT FK_Deliveries_Order FOREIGN KEY (OrderGuid) REFERENCES ordOrders (Id)
    );
END
GO
