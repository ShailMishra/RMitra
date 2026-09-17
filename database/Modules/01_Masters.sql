USE [RasoiMitra];
GO

IF OBJECT_ID(N'masRoles', N'U') IS NULL
BEGIN
    CREATE TABLE masRoles
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masRoles PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masRoles_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masRoles_Order DEFAULT (0),
        CONSTRAINT UQ_masRoles_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masAuthPurposes', N'U') IS NULL
BEGIN
    CREATE TABLE masAuthPurposes
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masAuthPurposes PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masAuthPurposes_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masAuthPurposes_Order DEFAULT (0),
        CONSTRAINT UQ_masAuthPurposes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masUserStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masUserStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masUserStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        StatusValue     INT               NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masUserStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masUserStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masUserStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masKitchenTypes', N'U') IS NULL
BEGIN
    CREATE TABLE masKitchenTypes
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masKitchenTypes PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masKitchenTypes_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masKitchenTypes_Order DEFAULT (0),
        CONSTRAINT UQ_masKitchenTypes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masKitchenStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masKitchenStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masKitchenStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masKitchenStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masKitchenStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masKitchenStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masDocumentTypes', N'U') IS NULL
BEGIN
    CREATE TABLE masDocumentTypes
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masDocumentTypes PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        Audience        NVARCHAR(50)      NOT NULL,
        IsRequired      BIT               NOT NULL CONSTRAINT DF_masDocumentTypes_Required DEFAULT (1),
        IsActive        BIT               NOT NULL CONSTRAINT DF_masDocumentTypes_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masDocumentTypes_Order DEFAULT (0),
        CONSTRAINT UQ_masDocumentTypes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masCuisines', N'U') IS NULL
BEGIN
    CREATE TABLE masCuisines
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masCuisines PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masCuisines_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masCuisines_Order DEFAULT (0),
        CONSTRAINT UQ_masCuisines_Code UNIQUE (Code)
    );

    CREATE INDEX IX_masCuisines_Name ON masCuisines (Name);
END
GO

IF OBJECT_ID(N'masMenuItemStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masMenuItemStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masMenuItemStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masMenuItemStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masMenuItemStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masMenuItemStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masAddressLabels', N'U') IS NULL
BEGIN
    CREATE TABLE masAddressLabels
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masAddressLabels PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masAddressLabels_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masAddressLabels_Order DEFAULT (0),
        CONSTRAINT UQ_masAddressLabels_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masOrderStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masOrderStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masOrderStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masOrderStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masOrderStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masOrderStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masPaymentMethods', N'U') IS NULL
BEGIN
    CREATE TABLE masPaymentMethods
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masPaymentMethods PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masPaymentMethods_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masPaymentMethods_Order DEFAULT (0),
        CONSTRAINT UQ_masPaymentMethods_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masPaymentStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masPaymentStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masPaymentStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masPaymentStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masPaymentStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masPaymentStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masDeliveryModes', N'U') IS NULL
BEGIN
    CREATE TABLE masDeliveryModes
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masDeliveryModes PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masDeliveryModes_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masDeliveryModes_Order DEFAULT (0),
        CONSTRAINT UQ_masDeliveryModes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masVehicleTypes', N'U') IS NULL
BEGIN
    CREATE TABLE masVehicleTypes
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masVehicleTypes PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masVehicleTypes_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masVehicleTypes_Order DEFAULT (0),
        CONSTRAINT UQ_masVehicleTypes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masRiderStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masRiderStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masRiderStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masRiderStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masRiderStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masRiderStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masAssignmentStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masAssignmentStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masAssignmentStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masAssignmentStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masAssignmentStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masAssignmentStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masTripStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masTripStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masTripStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masTripStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masTripStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masTripStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masSettlementStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE masSettlementStatuses
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masSettlementStatuses PRIMARY KEY,
        Code            NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(100)     NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masSettlementStatuses_Active DEFAULT (1),
        DisplayOrder    INT               NOT NULL CONSTRAINT DF_masSettlementStatuses_Order DEFAULT (0),
        CONSTRAINT UQ_masSettlementStatuses_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'masPlans', N'U') IS NULL
BEGIN
    CREATE TABLE masPlans
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_masPlans PRIMARY KEY,
        PlanCode        NVARCHAR(50)      NOT NULL,
        Audience        NVARCHAR(50)      NOT NULL,
        Name            NVARCHAR(200)     NOT NULL,
        Price           DECIMAL(18,2)     NOT NULL,
        DurationDays    INT               NOT NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_masPlans_Active DEFAULT (1),
        CONSTRAINT UQ_masPlans_PlanCode UNIQUE (PlanCode)
    );

    CREATE INDEX IX_masPlans_Audience ON masPlans (Audience, Price);
END
GO
