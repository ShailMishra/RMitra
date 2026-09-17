/* RasoiMitra Azure schema: tbl*, mas*, usp*, seed data.
   Run this in Azure Portal > SQL database RasoiMitra > Query editor.
*/

GO

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


GO

USE [RasoiMitra];
GO

IF OBJECT_ID(N'tblNumberSeries', N'U') IS NULL
BEGIN
    CREATE TABLE tblNumberSeries
    (
        Prefix      NVARCHAR(10) NOT NULL CONSTRAINT PK_tblNumberSeries PRIMARY KEY,
        LastNumber  INT          NOT NULL
    );
END
GO

IF OBJECT_ID(N'tblUsers', N'U') IS NULL
BEGIN
    CREATE TABLE tblUsers
    (
        Id              UNIQUEIDENTIFIER    NOT NULL CONSTRAINT PK_tblUsers PRIMARY KEY,
        UserCode        NVARCHAR(20)        NOT NULL,
        FullName        NVARCHAR(200)       NOT NULL,
        MobileNumber    NVARCHAR(15)        NOT NULL,
        Email           NVARCHAR(200)       NULL,
        Role            NVARCHAR(50)        NOT NULL,
        PasswordHash    NVARCHAR(500)       NULL,
        Status          INT                 NOT NULL CONSTRAINT DF_tblUsers_Status DEFAULT (1),
        CreatedAt       DATETIME2           NOT NULL CONSTRAINT DF_tblUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2           NOT NULL CONSTRAINT DF_tblUsers_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblUsers_UserCode UNIQUE (UserCode),
        CONSTRAINT UQ_tblUsers_Mobile_Role UNIQUE (MobileNumber, Role)
    );
END
GO

IF OBJECT_ID(N'tblOtpRequests', N'U') IS NULL
BEGIN
    CREATE TABLE tblOtpRequests
    (
        Id                  UNIQUEIDENTIFIER    NOT NULL CONSTRAINT PK_tblOtpRequests PRIMARY KEY,
        RequestId           NVARCHAR(40)        NOT NULL,
        MobileNumber        NVARCHAR(15)        NOT NULL,
        Purpose             NVARCHAR(50)        NOT NULL,
        OtpHash             NVARCHAR(128)       NOT NULL,
        AttemptCount        INT                 NOT NULL CONSTRAINT DF_tblOtp_Attempt DEFAULT (0),
        ExpiresAt           DATETIME2           NOT NULL,
        VerifiedAt          DATETIME2           NULL,
        VerificationToken   NVARCHAR(64)        NULL,
        TokenExpiresAt      DATETIME2           NULL,
        CreatedAt           DATETIME2           NOT NULL CONSTRAINT DF_tblOtp_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblOtpRequests_RequestId UNIQUE (RequestId)
    );

    CREATE INDEX IX_tblOtpRequests_Mobile_Purpose ON tblOtpRequests (MobileNumber, Purpose, CreatedAt DESC);
END
GO

IF OBJECT_ID(N'tblKitchens', N'U') IS NULL
BEGIN
    CREATE TABLE tblKitchens
    (
        Id                          UNIQUEIDENTIFIER    NOT NULL CONSTRAINT PK_tblKitchens PRIMARY KEY,
        KitchenId                   NVARCHAR(20)        NOT NULL,
        OwnerUserId                 UNIQUEIDENTIFIER    NOT NULL,
        KitchenName                 NVARCHAR(200)       NOT NULL,
        OwnerName                   NVARCHAR(200)       NOT NULL,
        MobileNumber                NVARCHAR(15)        NOT NULL,
        Email                       NVARCHAR(200)       NOT NULL,
        KitchenType                 NVARCHAR(50)        NOT NULL CONSTRAINT DF_tblKitchens_Type DEFAULT (N'HOMEMADE'),
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
        DeliveryRadiusKm            INT                 NOT NULL CONSTRAINT DF_tblKitchens_Radius DEFAULT (10),
        AccountHolderName           NVARCHAR(200)       NULL,
        AccountNumber               NVARCHAR(50)        NULL,
        IfscCode                    NVARCHAR(20)        NULL,
        AccountType                 NVARCHAR(20)        NULL,
        Status                      NVARCHAR(50)        NOT NULL,
        ListingEnabled              BIT                 NOT NULL CONSTRAINT DF_tblKitchens_Listing DEFAULT (0),
        RejectionReason             NVARCHAR(1000)      NULL,
        AdditionalDocumentsRequired NVARCHAR(MAX)       NULL,
        CreatedAt                   DATETIME2           NOT NULL CONSTRAINT DF_tblKitchens_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt                   DATETIME2           NOT NULL CONSTRAINT DF_tblKitchens_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        SubmittedAt                 DATETIME2           NULL,
        ApprovedAt                  DATETIME2           NULL,
        ActivatedAt                 DATETIME2           NULL,
        CONSTRAINT UQ_tblKitchens_KitchenId UNIQUE (KitchenId),
        CONSTRAINT UQ_tblKitchens_Mobile UNIQUE (MobileNumber),
        CONSTRAINT FK_tblKitchens_Owner FOREIGN KEY (OwnerUserId) REFERENCES tblUsers (Id)
    );

    CREATE INDEX IX_tblKitchens_Status ON tblKitchens (Status);
END
GO

IF OBJECT_ID(N'tblKitchenDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE tblKitchenDocuments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblKitchenDocuments PRIMARY KEY,
        KitchenGuid     UNIQUEIDENTIFIER NOT NULL,
        DocumentType    NVARCHAR(50)     NOT NULL,
        FileUrl         NVARCHAR(1000)   NOT NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblKitchenDocs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_tblKitchenDocs_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES tblKitchens (Id)
    );
END
GO

IF OBJECT_ID(N'tblKitchenStatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE tblKitchenStatusHistory
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblKitchenStatusHistory PRIMARY KEY,
        KitchenGuid UNIQUEIDENTIFIER     NOT NULL,
        Status      NVARCHAR(50)         NOT NULL,
        Remarks     NVARCHAR(1000)       NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_tblKitchenHistory_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_tblKitchenHistory_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES tblKitchens (Id)
    );
END
GO

IF OBJECT_ID(N'tblMenuCategories', N'U') IS NULL
BEGIN
    CREATE TABLE tblMenuCategories
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblMenuCategories PRIMARY KEY,
        KitchenGuid     UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(150)    NOT NULL,
        DisplayOrder    INT              NOT NULL CONSTRAINT DF_tblMenuCategories_Order DEFAULT (0),
        IsActive        BIT              NOT NULL CONSTRAINT DF_tblMenuCategories_Active DEFAULT (1),
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblMenuCategories_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_tblMenuCategories_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES tblKitchens (Id)
    );
END
GO

IF OBJECT_ID(N'tblMenuItems', N'U') IS NULL
BEGIN
    CREATE TABLE tblMenuItems
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblMenuItems PRIMARY KEY,
        ItemId              NVARCHAR(20)     NOT NULL,
        KitchenGuid         UNIQUEIDENTIFIER NOT NULL,
        CategoryId          UNIQUEIDENTIFIER NOT NULL,
        Name                NVARCHAR(200)    NOT NULL,
        Description         NVARCHAR(1000)   NULL,
        Price               DECIMAL(10,2)    NOT NULL,
        IsVeg               BIT              NOT NULL CONSTRAINT DF_tblMenuItems_IsVeg DEFAULT (1),
        Status              NVARCHAR(20)     NOT NULL CONSTRAINT DF_tblMenuItems_Status DEFAULT (N'DRAFT'),
        PhotoUrl            NVARCHAR(1000)   NULL,
        PreparationMinutes  INT              NOT NULL CONSTRAINT DF_tblMenuItems_Prep DEFAULT (20),
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblMenuItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblMenuItems_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblMenuItems_ItemId UNIQUE (ItemId),
        CONSTRAINT FK_tblMenuItems_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES tblKitchens (Id),
        CONSTRAINT FK_tblMenuItems_Category FOREIGN KEY (CategoryId) REFERENCES tblMenuCategories (Id)
    );
END
GO

IF OBJECT_ID(N'tblAddresses', N'U') IS NULL
BEGIN
    CREATE TABLE tblAddresses
    (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblAddresses PRIMARY KEY,
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
        IsDefault   BIT              NOT NULL CONSTRAINT DF_tblAddresses_Default DEFAULT (0),
        CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_tblAddresses_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblAddresses_AddressId UNIQUE (AddressId),
        CONSTRAINT FK_tblAddresses_User FOREIGN KEY (UserId) REFERENCES tblUsers (Id)
    );
END
GO

IF OBJECT_ID(N'tblCarts', N'U') IS NULL
BEGIN
    CREATE TABLE tblCarts
    (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblCarts PRIMARY KEY,
        UserId      UNIQUEIDENTIFIER NOT NULL,
        KitchenGuid UNIQUEIDENTIFIER NULL,
        UpdatedAt   DATETIME2        NOT NULL CONSTRAINT DF_tblCarts_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblCarts_UserId UNIQUE (UserId),
        CONSTRAINT FK_tblCarts_User FOREIGN KEY (UserId) REFERENCES tblUsers (Id)
    );
END
GO

IF OBJECT_ID(N'tblCartItems', N'U') IS NULL
BEGIN
    CREATE TABLE tblCartItems
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblCartItems PRIMARY KEY,
        CartId          UNIQUEIDENTIFIER NOT NULL,
        MenuItemGuid    UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(200)    NOT NULL,
        UnitPrice       DECIMAL(10,2)    NOT NULL,
        Quantity        INT              NOT NULL,
        CONSTRAINT FK_tblCartItems_Cart FOREIGN KEY (CartId) REFERENCES tblCarts (Id)
    );
END
GO

IF OBJECT_ID(N'tblOrders', N'U') IS NULL
BEGIN
    CREATE TABLE tblOrders
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblOrders PRIMARY KEY,
        OrderId             NVARCHAR(20)     NOT NULL,
        CustomerUserId      UNIQUEIDENTIFIER NOT NULL,
        KitchenGuid         UNIQUEIDENTIFIER NOT NULL,
        KitchenPublicId     NVARCHAR(20)     NOT NULL,
        AddressGuid         UNIQUEIDENTIFIER NOT NULL,
        DeliveryAddress     NVARCHAR(1000)   NOT NULL,
        ItemTotal           DECIMAL(10,2)    NOT NULL,
        DiscountAmount      DECIMAL(10,2)    NOT NULL,
        DeliveryFee         DECIMAL(10,2)    NOT NULL,
        TaxAmount           DECIMAL(10,2)    NOT NULL CONSTRAINT DF_tblOrders_Tax DEFAULT (0),
        TipAmount           DECIMAL(10,2)    NOT NULL CONSTRAINT DF_tblOrders_Tip DEFAULT (0),
        GrandTotal          DECIMAL(10,2)    NOT NULL,
        PaymentMethod       NVARCHAR(20)     NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        DeliveryMode        NVARCHAR(20)     NOT NULL CONSTRAINT DF_tblOrders_Mode DEFAULT (N'RIDER'),
        Instructions        NVARCHAR(500)    NULL,
        CancelReason        NVARCHAR(500)    NULL,
        IdempotencyKey      NVARCHAR(100)    NULL,
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblOrders_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblOrders_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        AcceptedDeadlineAt  DATETIME2        NULL,
        CONSTRAINT UQ_tblOrders_OrderId UNIQUE (OrderId),
        CONSTRAINT FK_tblOrders_Customer FOREIGN KEY (CustomerUserId) REFERENCES tblUsers (Id),
        CONSTRAINT FK_tblOrders_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES tblKitchens (Id)
    );

    CREATE INDEX IX_tblOrders_Idempotency ON tblOrders (CustomerUserId, IdempotencyKey);
END
GO

IF OBJECT_ID(N'tblOrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE tblOrderItems
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblOrderItems PRIMARY KEY,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        MenuItemGuid    UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(200)    NOT NULL,
        UnitPrice       DECIMAL(10,2)    NOT NULL,
        Quantity        INT              NOT NULL,
        LineTotal       DECIMAL(10,2)    NOT NULL,
        CONSTRAINT FK_tblOrderItems_Order FOREIGN KEY (OrderGuid) REFERENCES tblOrders (Id)
    );
END
GO

IF OBJECT_ID(N'tblOrderStatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE tblOrderStatusHistory
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblOrderStatusHistory PRIMARY KEY,
        OrderGuid   UNIQUEIDENTIFIER     NOT NULL,
        Status      NVARCHAR(50)         NOT NULL,
        Remarks     NVARCHAR(500)        NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_tblOrderHistory_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_tblOrderHistory_Order FOREIGN KEY (OrderGuid) REFERENCES tblOrders (Id)
    );
END
GO

IF OBJECT_ID(N'tblPayments', N'U') IS NULL
BEGIN
    CREATE TABLE tblPayments
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblPayments PRIMARY KEY,
        PaymentId           NVARCHAR(20)     NOT NULL,
        OrderGuid           UNIQUEIDENTIFIER NOT NULL,
        UserId              UNIQUEIDENTIFIER NOT NULL,
        Amount              DECIMAL(10,2)    NOT NULL,
        Method              NVARCHAR(20)     NOT NULL,
        Status              NVARCHAR(20)     NOT NULL,
        GatewayReference    NVARCHAR(100)    NULL,
        FailureReason       NVARCHAR(500)    NULL,
        AttemptCount        INT              NOT NULL CONSTRAINT DF_tblPayments_Attempt DEFAULT (1),
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblPayments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblPayments_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblPayments_PaymentId UNIQUE (PaymentId),
        CONSTRAINT FK_tblPayments_Order FOREIGN KEY (OrderGuid) REFERENCES tblOrders (Id)
    );
END
GO

IF OBJECT_ID(N'tblRiders', N'U') IS NULL
BEGIN
    CREATE TABLE tblRiders
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblRiders PRIMARY KEY,
        RiderId             NVARCHAR(20)     NOT NULL,
        UserId              UNIQUEIDENTIFIER NOT NULL,
        FullName            NVARCHAR(200)    NOT NULL,
        MobileNumber        NVARCHAR(15)     NOT NULL,
        VehicleType         NVARCHAR(20)     NOT NULL,
        Status              NVARCHAR(20)     NOT NULL,
        Available           BIT              NOT NULL CONSTRAINT DF_tblRiders_Available DEFAULT (0),
        Latitude            DECIMAL(9,6)     NULL,
        Longitude           DECIMAL(9,6)     NULL,
        AccountHolderName   NVARCHAR(200)    NULL,
        AccountNumber       NVARCHAR(50)     NULL,
        IfscCode            NVARCHAR(20)     NULL,
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblRiders_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_tblRiders_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblRiders_RiderId UNIQUE (RiderId),
        CONSTRAINT UQ_tblRiders_UserId UNIQUE (UserId),
        CONSTRAINT FK_tblRiders_User FOREIGN KEY (UserId) REFERENCES tblUsers (Id)
    );
END
GO

IF OBJECT_ID(N'tblRiderDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE tblRiderDocuments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblRiderDocuments PRIMARY KEY,
        RiderGuid       UNIQUEIDENTIFIER NOT NULL,
        DocumentType    NVARCHAR(50)     NOT NULL,
        FileUrl         NVARCHAR(1000)   NOT NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblRiderDocs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_tblRiderDocs_Rider FOREIGN KEY (RiderGuid) REFERENCES tblRiders (Id)
    );
END
GO

IF OBJECT_ID(N'tblAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE tblAssignments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblAssignments PRIMARY KEY,
        AssignmentId    NVARCHAR(20)     NOT NULL,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        RiderGuid       UNIQUEIDENTIFIER NOT NULL,
        Status          NVARCHAR(20)     NOT NULL,
        ExpiresAt       DATETIME2        NOT NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblAssignments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblAssignments_AssignmentId UNIQUE (AssignmentId),
        CONSTRAINT FK_tblAssignments_Order FOREIGN KEY (OrderGuid) REFERENCES tblOrders (Id),
        CONSTRAINT FK_tblAssignments_Rider FOREIGN KEY (RiderGuid) REFERENCES tblRiders (Id)
    );
END
GO

IF OBJECT_ID(N'tblDeliveries', N'U') IS NULL
BEGIN
    CREATE TABLE tblDeliveries
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblDeliveries PRIMARY KEY,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        RiderGuid       UNIQUEIDENTIFIER NULL,
        DeliveryMode    NVARCHAR(20)     NOT NULL,
        Status          NVARCHAR(30)     NOT NULL,
        Latitude        DECIMAL(9,6)     NULL,
        Longitude       DECIMAL(9,6)     NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblDeliveries_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblDeliveries_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblDeliveries_OrderGuid UNIQUE (OrderGuid),
        CONSTRAINT FK_tblDeliveries_Order FOREIGN KEY (OrderGuid) REFERENCES tblOrders (Id)
    );
END
GO

IF OBJECT_ID(N'tblReviews', N'U') IS NULL
BEGIN
    CREATE TABLE tblReviews
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblReviews PRIMARY KEY,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        KitchenGuid     UNIQUEIDENTIFIER NOT NULL,
        CustomerUserId  UNIQUEIDENTIFIER NOT NULL,
        Rating          INT              NOT NULL,
        RiderRating     INT              NULL,
        Comments        NVARCHAR(1000)   NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblReviews_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblReviews_OrderGuid UNIQUE (OrderGuid),
        CONSTRAINT CK_tblReviews_Rating CHECK (Rating BETWEEN 1 AND 5),
        CONSTRAINT FK_tblReviews_Order FOREIGN KEY (OrderGuid) REFERENCES tblOrders (Id)
    );
END
GO

IF OBJECT_ID(N'tblSubscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE tblSubscriptions
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblSubscriptions PRIMARY KEY,
        SubscriptionId  NVARCHAR(20)     NOT NULL,
        UserId          UNIQUEIDENTIFIER NOT NULL,
        PlanCode        NVARCHAR(50)     NOT NULL,
        Audience        NVARCHAR(20)     NOT NULL,
        ValidUntil      DATETIME2        NOT NULL,
        AutoRenew       BIT              NOT NULL CONSTRAINT DF_tblSubscriptions_Renew DEFAULT (1),
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblSubscriptions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblSubscriptions_SubscriptionId UNIQUE (SubscriptionId),
        CONSTRAINT FK_tblSubscriptions_User FOREIGN KEY (UserId) REFERENCES tblUsers (Id)
    );
END
GO

IF OBJECT_ID(N'tblLedgerLines', N'U') IS NULL
BEGIN
    CREATE TABLE tblLedgerLines
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tblLedgerLines PRIMARY KEY,
        PartyType   NVARCHAR(20)         NOT NULL,
        PartyGuid   UNIQUEIDENTIFIER     NOT NULL,
        OrderGuid   UNIQUEIDENTIFIER     NOT NULL,
        Amount      DECIMAL(10,2)        NOT NULL,
        Description NVARCHAR(200)        NOT NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_tblLedger_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID(N'tblSettlements', N'U') IS NULL
BEGIN
    CREATE TABLE tblSettlements
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_tblSettlements PRIMARY KEY,
        SettlementId    NVARCHAR(20)     NOT NULL,
        PartyType       NVARCHAR(20)     NOT NULL,
        PartyGuid       UNIQUEIDENTIFIER NOT NULL,
        PeriodStart     DATETIME2        NOT NULL,
        PeriodEnd       DATETIME2        NOT NULL,
        Amount          DECIMAL(10,2)    NOT NULL,
        Status          NVARCHAR(20)     NOT NULL,
        InvoiceUrl      NVARCHAR(500)    NULL,
        Utr             NVARCHAR(50)     NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_tblSettlements_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tblSettlements_SettlementId UNIQUE (SettlementId)
    );
END
GO


GO

USE [RasoiMitra];
GO

/* Shared helpers */

CREATE OR ALTER PROCEDURE uspNextPublicId
    @Prefix NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM tblNumberSeries WHERE Prefix = @Prefix)
        INSERT INTO tblNumberSeries (Prefix, LastNumber) VALUES (@Prefix, 10000);

    UPDATE tblNumberSeries
    SET LastNumber = LastNumber + 1
    OUTPUT INSERTED.LastNumber
    WHERE Prefix = @Prefix;
END
GO

CREATE OR ALTER PROCEDURE uspFindUser
    @MobileNumber NVARCHAR(15),
    @Purpose NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers WHERE MobileNumber = @MobileNumber AND Role = @Purpose;
END
GO

CREATE OR ALTER PROCEDURE uspInsertUser
    @Id UNIQUEIDENTIFIER,
    @UserCode NVARCHAR(20),
    @FullName NVARCHAR(200),
    @MobileNumber NVARCHAR(15),
    @Email NVARCHAR(200) = NULL,
    @Role NVARCHAR(50),
    @PasswordHash NVARCHAR(500) = NULL,
    @Status INT,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblUsers (Id, UserCode, FullName, MobileNumber, Email, Role, PasswordHash, Status, CreatedAt, UpdatedAt)
    VALUES (@Id, @UserCode, @FullName, @MobileNumber, @Email, @Role, @PasswordHash, @Status, @CreatedAt, @UpdatedAt);
END
GO

/* AuthController */

CREATE OR ALTER PROCEDURE uspSendOtp
    @Id UNIQUEIDENTIFIER,
    @RequestId NVARCHAR(40),
    @MobileNumber NVARCHAR(15),
    @Purpose NVARCHAR(50),
    @OtpHash NVARCHAR(128),
    @ExpiresAt DATETIME2,
    @MaxSendPer15Minutes INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RecentCount INT =
    (
        SELECT COUNT(1)
        FROM tblOtpRequests
        WHERE MobileNumber = @MobileNumber
          AND Purpose = @Purpose
          AND CreatedAt >= DATEADD(MINUTE, -15, SYSUTCDATETIME())
    );

    IF @RecentCount >= @MaxSendPer15Minutes
    BEGIN
        SELECT @RecentCount AS RecentCount, CAST(0 AS BIT) AS Inserted;
        RETURN;
    END

    INSERT INTO tblOtpRequests (Id, RequestId, MobileNumber, Purpose, OtpHash, AttemptCount, ExpiresAt, CreatedAt)
    VALUES (@Id, @RequestId, @MobileNumber, @Purpose, @OtpHash, 0, @ExpiresAt, SYSUTCDATETIME());

    SELECT @RecentCount AS RecentCount, CAST(1 AS BIT) AS Inserted;
END
GO

CREATE OR ALTER PROCEDURE uspVerifyOtp
    @RequestId NVARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblOtpRequests WHERE RequestId = @RequestId;
END
GO

CREATE OR ALTER PROCEDURE uspVerifyOtpIncrementAttempt
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOtpRequests SET AttemptCount = AttemptCount + 1 WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspVerifyOtpConsume
    @Id UNIQUEIDENTIFIER,
    @Token NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOtpRequests
    SET VerifiedAt = SYSUTCDATETIME(),
        VerificationToken = @Token,
        TokenExpiresAt = DATEADD(MINUTE, 30, SYSUTCDATETIME())
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspSetPassword
    @Id UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers
    SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspLogin
    @MobileNumber NVARCHAR(15),
    @Purpose NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers WHERE MobileNumber = @MobileNumber AND Role = @Purpose;
END
GO

CREATE OR ALTER PROCEDURE uspRequireFreshToken
    @VerificationToken NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblOtpRequests WHERE VerificationToken = @VerificationToken;
END
GO

/* CustomerController */

CREATE OR ALTER PROCEDURE uspRegisterCustomer
    @Id UNIQUEIDENTIFIER,
    @UserCode NVARCHAR(20),
    @FullName NVARCHAR(200),
    @MobileNumber NVARCHAR(15),
    @Email NVARCHAR(200) = NULL,
    @Role NVARCHAR(50),
    @PasswordHash NVARCHAR(500) = NULL,
    @Status INT,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspInsertUser @Id, @UserCode, @FullName, @MobileNumber, @Email, @Role, @PasswordHash, @Status, @CreatedAt, @UpdatedAt;
    SELECT * FROM tblUsers WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspGetMe
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspUpdateMe
    @UserId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200) = NULL,
    @Email NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers
    SET FullName = COALESCE(@Name, FullName),
        Email = COALESCE(@Email, Email),
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @UserId;
    SELECT * FROM tblUsers WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspGetAddresses
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAddresses WHERE UserId = @UserId ORDER BY IsDefault DESC, CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspAddAddress
    @Id UNIQUEIDENTIFIER,
    @AddressId NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER,
    @Label NVARCHAR(50),
    @Line1 NVARCHAR(500),
    @Line2 NVARCHAR(500) = NULL,
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Pincode NVARCHAR(10),
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6),
    @IsDefault BIT,
    @CreatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    IF @IsDefault = 1
        UPDATE tblAddresses SET IsDefault = 0 WHERE UserId = @UserId;

    INSERT INTO tblAddresses
        (Id, AddressId, UserId, Label, Line1, Line2, City, State, Pincode, Latitude, Longitude, IsDefault, CreatedAt)
    VALUES
        (@Id, @AddressId, @UserId, @Label, @Line1, @Line2, @City, @State, @Pincode, @Latitude, @Longitude, @IsDefault, @CreatedAt);

    SELECT * FROM tblAddresses WHERE Id = @Id;
END
GO

/* KitchenController / AdminKitchenController */

CREATE OR ALTER PROCEDURE uspRegisterKitchen
    @Id UNIQUEIDENTIFIER,
    @KitchenId NVARCHAR(20),
    @OwnerUserId UNIQUEIDENTIFIER,
    @KitchenName NVARCHAR(200),
    @OwnerName NVARCHAR(200),
    @MobileNumber NVARCHAR(15),
    @Email NVARCHAR(200),
    @KitchenType NVARCHAR(50),
    @Status NVARCHAR(50),
    @DeliveryRadiusKm INT,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblKitchens
        (Id, KitchenId, OwnerUserId, KitchenName, OwnerName, MobileNumber, Email, KitchenType,
         Status, ListingEnabled, DeliveryRadiusKm, CreatedAt, UpdatedAt)
    VALUES
        (@Id, @KitchenId, @OwnerUserId, @KitchenName, @OwnerName, @MobileNumber, @Email, @KitchenType,
         @Status, 0, @DeliveryRadiusKm, @CreatedAt, @UpdatedAt);

    INSERT INTO tblKitchenStatusHistory (KitchenGuid, Status, Remarks, CreatedAt)
    VALUES (@Id, @Status, N'Draft created', SYSUTCDATETIME());

    SELECT * FROM tblKitchens WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspCountKitchenByMobile
    @MobileNumber NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblKitchens WHERE MobileNumber = @MobileNumber;
END
GO

CREATE OR ALTER PROCEDURE uspGetKitchen
    @KitchenId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspGetKitchenById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblKitchens WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspUpdate
    @KitchenId NVARCHAR(20),
    @KitchenName NVARCHAR(200),
    @OwnerName NVARCHAR(200),
    @Email NVARCHAR(200),
    @AddressLine1 NVARCHAR(500) = NULL,
    @AddressLine2 NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Pincode NVARCHAR(10) = NULL,
    @Latitude DECIMAL(9,6) = NULL,
    @Longitude DECIMAL(9,6) = NULL,
    @CuisineTypes NVARCHAR(MAX) = NULL,
    @OpenTime NVARCHAR(10) = NULL,
    @CloseTime NVARCHAR(10) = NULL,
    @DeliveryRadiusKm INT,
    @AccountHolderName NVARCHAR(200) = NULL,
    @AccountNumber NVARCHAR(50) = NULL,
    @IfscCode NVARCHAR(20) = NULL,
    @AccountType NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblKitchens SET
        KitchenName = @KitchenName,
        OwnerName = @OwnerName,
        Email = @Email,
        AddressLine1 = @AddressLine1,
        AddressLine2 = @AddressLine2,
        City = @City,
        State = @State,
        Pincode = @Pincode,
        Latitude = @Latitude,
        Longitude = @Longitude,
        CuisineTypes = @CuisineTypes,
        OpenTime = @OpenTime,
        CloseTime = @CloseTime,
        DeliveryRadiusKm = @DeliveryRadiusKm,
        AccountHolderName = @AccountHolderName,
        AccountNumber = @AccountNumber,
        IfscCode = @IfscCode,
        AccountType = @AccountType,
        UpdatedAt = SYSUTCDATETIME()
    WHERE KitchenId = @KitchenId;

    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspUploadDocument
    @Id UNIQUEIDENTIFIER,
    @KitchenGuid UNIQUEIDENTIFIER,
    @DocumentType NVARCHAR(50),
    @FileUrl NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblKitchenDocuments (Id, KitchenGuid, DocumentType, FileUrl, CreatedAt)
    VALUES (@Id, @KitchenGuid, @DocumentType, @FileUrl, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetDocuments
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblKitchenDocuments WHERE KitchenGuid = @KitchenGuid;
END
GO

CREATE OR ALTER PROCEDURE uspSubmit
    @KitchenId NVARCHAR(20),
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblKitchens
    SET Status = @Status,
        SubmittedAt = SYSUTCDATETIME(),
        UpdatedAt = SYSUTCDATETIME(),
        RejectionReason = NULL,
        AdditionalDocumentsRequired = NULL
    WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspSetKitchenStatus
    @KitchenId NVARCHAR(20),
    @Status NVARCHAR(50),
    @ListingEnabled BIT = NULL,
    @RejectionReason NVARCHAR(1000) = NULL,
    @AdditionalDocumentsRequired NVARCHAR(MAX) = NULL,
    @SetApprovedAt BIT = 0,
    @SetActivatedAt BIT = 0,
    @Remarks NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblKitchens
    SET Status = @Status,
        ListingEnabled = COALESCE(@ListingEnabled, ListingEnabled),
        RejectionReason = COALESCE(@RejectionReason, RejectionReason),
        AdditionalDocumentsRequired = COALESCE(@AdditionalDocumentsRequired, AdditionalDocumentsRequired),
        ApprovedAt = CASE WHEN @SetApprovedAt = 1 THEN SYSUTCDATETIME() ELSE ApprovedAt END,
        ActivatedAt = CASE WHEN @SetActivatedAt = 1 THEN SYSUTCDATETIME() ELSE ActivatedAt END,
        UpdatedAt = SYSUTCDATETIME()
    WHERE KitchenId = @KitchenId;

    DECLARE @KitchenGuid UNIQUEIDENTIFIER = (SELECT Id FROM tblKitchens WHERE KitchenId = @KitchenId);
    IF @KitchenGuid IS NOT NULL AND @Remarks IS NOT NULL
        INSERT INTO tblKitchenStatusHistory (KitchenGuid, Status, Remarks, CreatedAt)
        VALUES (@KitchenGuid, @Status, @Remarks, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetProgress
    @KitchenId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspActivate
    @KitchenId NVARCHAR(20),
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspSetKitchenStatus @KitchenId = @KitchenId, @Status = @Status, @SetActivatedAt = 1, @Remarks = N'Kitchen activated';
    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspCountPublishedMenuItems
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblMenuItems WHERE KitchenGuid = @KitchenGuid AND Status = N'PUBLISHED';
END
GO

CREATE OR ALTER PROCEDURE uspGetDashboard
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(1) AS TodayOrders,
        ISNULL(SUM(CASE WHEN Status = N'DELIVERED' THEN ItemTotal ELSE 0 END), 0) AS TodayEarnings
    FROM tblOrders
    WHERE KitchenGuid = @KitchenGuid
      AND CAST(CreatedAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE);
END
GO

CREATE OR ALTER PROCEDURE uspGetPendingKitchens
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM tblKitchens
    WHERE Status IN (N'SUBMITTED', N'DOCUMENT_VERIFICATION_PENDING', N'UNDER_REVIEW', N'ADDITIONAL_DOCUMENTS_REQUIRED', N'RESUBMITTED')
    ORDER BY SubmittedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspApproveKitchen
    @KitchenId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspSetKitchenStatus @KitchenId = @KitchenId, @Status = N'APPROVED', @ListingEnabled = 1, @SetApprovedAt = 1, @Remarks = N'Approved';
    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspRejectKitchen
    @KitchenId NVARCHAR(20),
    @Reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblKitchens
    SET Status = N'REJECTED',
        RejectionReason = @Reason,
        ListingEnabled = 0,
        UpdatedAt = SYSUTCDATETIME()
    WHERE KitchenId = @KitchenId;

    DECLARE @KitchenGuid UNIQUEIDENTIFIER = (SELECT Id FROM tblKitchens WHERE KitchenId = @KitchenId);
    INSERT INTO tblKitchenStatusHistory (KitchenGuid, Status, Remarks, CreatedAt)
    VALUES (@KitchenGuid, N'REJECTED', @Reason, SYSUTCDATETIME());

    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspRequestDocuments
    @KitchenId NVARCHAR(20),
    @DocumentsRequired NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblKitchens
    SET Status = N'ADDITIONAL_DOCUMENTS_REQUIRED',
        AdditionalDocumentsRequired = @DocumentsRequired,
        UpdatedAt = SYSUTCDATETIME()
    WHERE KitchenId = @KitchenId;

    DECLARE @KitchenGuid UNIQUEIDENTIFIER = (SELECT Id FROM tblKitchens WHERE KitchenId = @KitchenId);
    INSERT INTO tblKitchenStatusHistory (KitchenGuid, Status, Remarks, CreatedAt)
    VALUES (@KitchenGuid, N'ADDITIONAL_DOCUMENTS_REQUIRED', @DocumentsRequired, SYSUTCDATETIME());

    SELECT * FROM tblKitchens WHERE KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspGetNearby
AS
BEGIN
    SET NOCOUNT ON;
    SELECT k.*
    FROM tblKitchens k
    WHERE k.Status = N'ACTIVE'
      AND k.Latitude IS NOT NULL
      AND k.Longitude IS NOT NULL
      AND EXISTS (SELECT 1 FROM tblMenuItems i WHERE i.KitchenGuid = k.Id AND i.Status = N'PUBLISHED');
END
GO

CREATE OR ALTER PROCEDURE uspGetListingEnabled
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ListingEnabled FROM tblKitchens WHERE Id = @KitchenGuid;
END
GO

CREATE OR ALTER PROCEDURE uspInsertKitchenHistory
    @KitchenGuid UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Remarks NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblKitchenStatusHistory (KitchenGuid, Status, Remarks, CreatedAt)
    VALUES (@KitchenGuid, @Status, @Remarks, SYSUTCDATETIME());
END
GO


GO

USE [RasoiMitra];
GO

/* Menu / catalog */

CREATE OR ALTER PROCEDURE uspGetCuisines
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Code, Name, CAST(1 AS BIT) AS IsActive
    FROM masCuisines
    WHERE IsActive = 1
    ORDER BY DisplayOrder, Name;
END
GO

CREATE OR ALTER PROCEDURE uspGetMenuCategories
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMenuCategories WHERE KitchenGuid = @KitchenGuid AND IsActive = 1 ORDER BY DisplayOrder, Name;
END
GO

CREATE OR ALTER PROCEDURE uspAddMenuCategory
    @Id UNIQUEIDENTIFIER,
    @KitchenGuid UNIQUEIDENTIFIER,
    @Name NVARCHAR(150),
    @DisplayOrder INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblMenuCategories (Id, KitchenGuid, Name, DisplayOrder, IsActive, CreatedAt)
    VALUES (@Id, @KitchenGuid, @Name, @DisplayOrder, 1, SYSUTCDATETIME());

    UPDATE tblKitchens
    SET Status = N'MENU_SETUP', UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KitchenGuid AND Status = N'APPROVED';
END
GO

CREATE OR ALTER PROCEDURE uspAddMenuItem
    @Id UNIQUEIDENTIFIER,
    @ItemId NVARCHAR(20),
    @KitchenGuid UNIQUEIDENTIFIER,
    @CategoryId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Description NVARCHAR(1000) = NULL,
    @Price DECIMAL(10,2),
    @IsVeg BIT,
    @Status NVARCHAR(20),
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblMenuItems
        (Id, ItemId, KitchenGuid, CategoryId, Name, Description, Price, IsVeg, Status, PreparationMinutes, CreatedAt, UpdatedAt)
    VALUES
        (@Id, @ItemId, @KitchenGuid, @CategoryId, @Name, @Description, @Price, @IsVeg, @Status, 20, @CreatedAt, @UpdatedAt);

    UPDATE tblKitchens
    SET Status = N'MENU_SETUP', UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KitchenGuid AND Status = N'APPROVED';
END
GO

CREATE OR ALTER PROCEDURE uspGetMenuItems
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMenuItems WHERE KitchenGuid = @KitchenGuid AND Status <> N'DELETED' ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE uspGetMenuItem
    @ItemId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMenuItems WHERE ItemId = @ItemId;
END
GO

CREATE OR ALTER PROCEDURE uspUpdateItem
    @ItemId NVARCHAR(20),
    @Name NVARCHAR(200),
    @Description NVARCHAR(1000) = NULL,
    @Price DECIMAL(10,2),
    @IsVeg BIT,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblMenuItems
    SET Name = @Name,
        Description = @Description,
        Price = @Price,
        IsVeg = @IsVeg,
        Status = @Status,
        UpdatedAt = SYSUTCDATETIME()
    WHERE ItemId = @ItemId;

    SELECT * FROM tblMenuItems WHERE ItemId = @ItemId;
END
GO

CREATE OR ALTER PROCEDURE uspUploadItemPhoto
    @ItemId NVARCHAR(20),
    @FileUrl NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblMenuItems SET PhotoUrl = @FileUrl, UpdatedAt = SYSUTCDATETIME() WHERE ItemId = @ItemId;
END
GO

CREATE OR ALTER PROCEDURE uspDeleteItem
    @ItemId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblMenuItems SET Status = N'DELETED', UpdatedAt = SYSUTCDATETIME() WHERE ItemId = @ItemId;
END
GO

CREATE OR ALTER PROCEDURE uspGetStorefront
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMenuItems WHERE KitchenGuid = @KitchenGuid AND Status = N'PUBLISHED';
END
GO

CREATE OR ALTER PROCEDURE uspGetPublishedMenuItem
    @ItemId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMenuItems WHERE ItemId = @ItemId AND Status = N'PUBLISHED';
END
GO

CREATE OR ALTER PROCEDURE uspGetMenuItemIds
    @Ids NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, ItemId
    FROM tblMenuItems
    WHERE Id IN (SELECT TRY_CONVERT(UNIQUEIDENTIFIER, value) FROM STRING_SPLIT(@Ids, ',') WHERE TRY_CONVERT(UNIQUEIDENTIFIER, value) IS NOT NULL);
END
GO

/* OrderingController */

CREATE OR ALTER PROCEDURE uspGetCart
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCarts WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspCreateCart
    @Id UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblCarts (Id, UserId, UpdatedAt) VALUES (@Id, @UserId, @UpdatedAt);
    SELECT * FROM tblCarts WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspGetCartItems
    @CartId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCartItems WHERE CartId = @CartId;
END
GO

CREATE OR ALTER PROCEDURE uspUpdateCart
    @CartId UNIQUEIDENTIFIER,
    @KitchenGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM tblCartItems WHERE CartId = @CartId;
    UPDATE tblCarts SET KitchenGuid = @KitchenGuid, UpdatedAt = SYSUTCDATETIME() WHERE Id = @CartId;
END
GO

CREATE OR ALTER PROCEDURE uspAddCartItem
    @Id UNIQUEIDENTIFIER,
    @CartId UNIQUEIDENTIFIER,
    @MenuItemGuid UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @UnitPrice DECIMAL(10,2),
    @Quantity INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblCartItems (Id, CartId, MenuItemGuid, Name, UnitPrice, Quantity)
    VALUES (@Id, @CartId, @MenuItemGuid, @Name, @UnitPrice, @Quantity);
END
GO

CREATE OR ALTER PROCEDURE uspClearCart
    @CartId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM tblCartItems WHERE CartId = @CartId;
    UPDATE tblCarts SET KitchenGuid = NULL WHERE Id = @CartId;
END
GO

CREATE OR ALTER PROCEDURE uspPlace
    @Id UNIQUEIDENTIFIER,
    @OrderId NVARCHAR(20),
    @CustomerUserId UNIQUEIDENTIFIER,
    @KitchenGuid UNIQUEIDENTIFIER,
    @KitchenPublicId NVARCHAR(20),
    @AddressGuid UNIQUEIDENTIFIER,
    @DeliveryAddress NVARCHAR(1000),
    @ItemTotal DECIMAL(10,2),
    @DiscountAmount DECIMAL(10,2),
    @DeliveryFee DECIMAL(10,2),
    @TipAmount DECIMAL(10,2),
    @GrandTotal DECIMAL(10,2),
    @PaymentMethod NVARCHAR(20),
    @Status NVARCHAR(50),
    @DeliveryMode NVARCHAR(20),
    @Instructions NVARCHAR(500) = NULL,
    @IdempotencyKey NVARCHAR(100) = NULL,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2,
    @AcceptedDeadlineAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblOrders
        (Id, OrderId, CustomerUserId, KitchenGuid, KitchenPublicId, AddressGuid, DeliveryAddress,
         ItemTotal, DiscountAmount, DeliveryFee, TaxAmount, TipAmount, GrandTotal, PaymentMethod,
         Status, DeliveryMode, Instructions, IdempotencyKey, CreatedAt, UpdatedAt, AcceptedDeadlineAt)
    VALUES
        (@Id, @OrderId, @CustomerUserId, @KitchenGuid, @KitchenPublicId, @AddressGuid, @DeliveryAddress,
         @ItemTotal, @DiscountAmount, @DeliveryFee, 0, @TipAmount, @GrandTotal, @PaymentMethod,
         @Status, @DeliveryMode, @Instructions, @IdempotencyKey, @CreatedAt, @UpdatedAt, @AcceptedDeadlineAt);

    INSERT INTO tblOrderStatusHistory (OrderGuid, Status, Remarks, CreatedAt)
    VALUES (@Id, @Status, N'Order created', SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspAddOrderItem
    @Id UNIQUEIDENTIFIER,
    @OrderGuid UNIQUEIDENTIFIER,
    @MenuItemGuid UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @UnitPrice DECIMAL(10,2),
    @Quantity INT,
    @LineTotal DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblOrderItems (Id, OrderGuid, MenuItemGuid, Name, UnitPrice, Quantity, LineTotal)
    VALUES (@Id, @OrderGuid, @MenuItemGuid, @Name, @UnitPrice, @Quantity, @LineTotal);
END
GO

CREATE OR ALTER PROCEDURE uspGetByIdempotencyKey
    @IdempotencyKey NVARCHAR(100),
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrderId FROM tblOrders WHERE IdempotencyKey = @IdempotencyKey AND CustomerUserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspGetAddress
    @AddressId NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAddresses WHERE AddressId = @AddressId AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspGetList
    @UserId UNIQUEIDENTIFIER,
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF @Role = N'KITCHEN_OWNER'
        SELECT o.OrderId
        FROM tblOrders o
        INNER JOIN tblKitchens k ON k.Id = o.KitchenGuid
        WHERE k.OwnerUserId = @UserId
        ORDER BY o.CreatedAt DESC;
    ELSE
        SELECT OrderId FROM tblOrders WHERE CustomerUserId = @UserId ORDER BY CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspGet
    @OrderId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblOrders WHERE OrderId = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderItems
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblOrderItems WHERE OrderGuid = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderTimeline
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Status FROM tblOrderStatusHistory WHERE OrderGuid = @OrderGuid ORDER BY CreatedAt;
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderTracking
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Latitude, Longitude FROM tblDeliveries WHERE OrderGuid = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspCancel
    @OrderGuid UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Remarks NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOrders SET Status = @Status, UpdatedAt = SYSUTCDATETIME() WHERE Id = @OrderGuid;
    INSERT INTO tblOrderStatusHistory (OrderGuid, Status, Remarks, CreatedAt)
    VALUES (@OrderGuid, @Status, @Remarks, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetOrders
    @KitchenId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderId
    FROM tblOrders o
    INNER JOIN tblKitchens k ON k.Id = o.KitchenGuid
    WHERE k.KitchenId = @KitchenId
    ORDER BY o.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspAcceptOrder
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspCancel @OrderGuid = @OrderGuid, @Status = N'ACCEPTED', @Remarks = N'Kitchen accepted';
END
GO

CREATE OR ALTER PROCEDURE uspRejectOrder
    @OrderGuid UNIQUEIDENTIFIER,
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspCancel @OrderGuid = @OrderGuid, @Status = @Status, @Remarks = N'Kitchen rejected';
    IF @Status = N'REFUNDED'
        UPDATE tblPayments SET Status = N'REFUNDED', UpdatedAt = SYSUTCDATETIME() WHERE OrderGuid = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspUpdateOrderStatus
    @OrderGuid UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Remarks NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspCancel @OrderGuid = @OrderGuid, @Status = @Status, @Remarks = @Remarks;
END
GO

CREATE OR ALTER PROCEDURE uspMarkOrderPlaced
    @OrderGuid UNIQUEIDENTIFIER,
    @AcceptMinutes INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOrders
    SET Status = N'PLACED',
        AcceptedDeadlineAt = DATEADD(MINUTE, @AcceptMinutes, SYSUTCDATETIME()),
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @OrderGuid;

    INSERT INTO tblOrderStatusHistory (OrderGuid, Status, Remarks, CreatedAt)
    VALUES (@OrderGuid, N'PLACED', N'Payment successful', SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspIsCustomerOrder
    @OrderId NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblOrders WHERE OrderId = @OrderId AND CustomerUserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspOwnerKitchen
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT KitchenId FROM tblKitchens WHERE OwnerUserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspKitchenOwnsOrder
    @KitchenGuid UNIQUEIDENTIFIER,
    @KitchenId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblKitchens WHERE Id = @KitchenGuid AND KitchenId = @KitchenId;
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderOwner
    @OrderId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT k.OwnerUserId
    FROM tblOrders o
    INNER JOIN tblKitchens k ON k.Id = o.KitchenGuid
    WHERE o.OrderId = @OrderId;
END
GO

CREATE OR ALTER PROCEDURE uspRate
    @Id UNIQUEIDENTIFIER,
    @OrderGuid UNIQUEIDENTIFIER,
    @KitchenGuid UNIQUEIDENTIFIER,
    @CustomerUserId UNIQUEIDENTIFIER,
    @Rating INT,
    @RiderRating INT = NULL,
    @Comments NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblReviews (Id, OrderGuid, KitchenGuid, CustomerUserId, Rating, RiderRating, Comments, CreatedAt)
    VALUES (@Id, @OrderGuid, @KitchenGuid, @CustomerUserId, @Rating, @RiderRating, @Comments, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspCountReview
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblReviews WHERE OrderGuid = @OrderGuid;
END
GO


GO

USE [RasoiMitra];
GO

/* PaymentController */

CREATE OR ALTER PROCEDURE uspCreatePayment
    @Id UNIQUEIDENTIFIER,
    @PaymentId NVARCHAR(20),
    @OrderGuid UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @Amount DECIMAL(10,2),
    @Method NVARCHAR(20),
    @Status NVARCHAR(20),
    @AttemptCount INT,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblPayments (Id, PaymentId, OrderGuid, UserId, Amount, Method, Status, AttemptCount, CreatedAt, UpdatedAt)
    VALUES (@Id, @PaymentId, @OrderGuid, @UserId, @Amount, @Method, @Status, @AttemptCount, @CreatedAt, @UpdatedAt);
END
GO

CREATE OR ALTER PROCEDURE uspCountPayments
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblPayments WHERE OrderGuid = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspConfirm
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @GatewayReference NVARCHAR(100) = NULL,
    @FailureReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblPayments
    SET Status = @Status,
        GatewayReference = COALESCE(@GatewayReference, GatewayReference),
        FailureReason = COALESCE(@FailureReason, FailureReason),
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspWebhook
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @GatewayReference NVARCHAR(100) = NULL,
    @FailureReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    EXEC uspConfirm @Id = @Id, @Status = @Status, @GatewayReference = @GatewayReference, @FailureReason = @FailureReason;
END
GO

CREATE OR ALTER PROCEDURE uspGetPayment
    @PaymentId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblPayments WHERE PaymentId = @PaymentId;
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderIdByGuid
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrderId FROM tblOrders WHERE Id = @OrderGuid;
END
GO

/* DeliveryController */

CREATE OR ALTER PROCEDURE uspRegisterRider
    @Id UNIQUEIDENTIFIER,
    @RiderId NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(200),
    @MobileNumber NVARCHAR(15),
    @VehicleType NVARCHAR(20),
    @Status NVARCHAR(20),
    @AccountHolderName NVARCHAR(200) = NULL,
    @AccountNumber NVARCHAR(50) = NULL,
    @IfscCode NVARCHAR(20) = NULL,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblRiders
        (Id, RiderId, UserId, FullName, MobileNumber, VehicleType, Status, AccountHolderName, AccountNumber, IfscCode, CreatedAt, UpdatedAt)
    VALUES
        (@Id, @RiderId, @UserId, @FullName, @MobileNumber, @VehicleType, @Status, @AccountHolderName, @AccountNumber, @IfscCode, @CreatedAt, @UpdatedAt);
END
GO

CREATE OR ALTER PROCEDURE uspGetRiderByUser
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblRiders WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE uspUploadRiderDocument
    @Id UNIQUEIDENTIFIER,
    @RiderGuid UNIQUEIDENTIFIER,
    @DocumentType NVARCHAR(50),
    @FileUrl NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblRiderDocuments (Id, RiderGuid, DocumentType, FileUrl, CreatedAt)
    VALUES (@Id, @RiderGuid, @DocumentType, @FileUrl, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetRiderDocuments
    @RiderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DocumentType FROM tblRiderDocuments WHERE RiderGuid = @RiderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspSubmitRider
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblRiders SET Status = @Status, UpdatedAt = SYSUTCDATETIME() WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspSetPresence
    @Id UNIQUEIDENTIFIER,
    @Available BIT,
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblRiders
    SET Available = @Available, Latitude = @Latitude, Longitude = @Longitude, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspGetCurrentAssignment
    @RiderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 *
    FROM tblAssignments
    WHERE RiderGuid = @RiderGuid AND Status = N'OFFERED' AND ExpiresAt > SYSUTCDATETIME()
    ORDER BY CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspRespondAssignment
    @AssignmentId NVARCHAR(20),
    @RiderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAssignments WHERE AssignmentId = @AssignmentId AND RiderGuid = @RiderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspSetAssignmentStatus
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblAssignments SET Status = @Status WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspCountBusyDeliveries
    @RiderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM tblDeliveries
    WHERE RiderGuid = @RiderGuid AND Status IN (N'ASSIGNED', N'ARRIVED_KITCHEN', N'PICKED_UP');
END
GO

CREATE OR ALTER PROCEDURE uspInsertDelivery
    @Id UNIQUEIDENTIFIER,
    @OrderGuid UNIQUEIDENTIFIER,
    @RiderGuid UNIQUEIDENTIFIER = NULL,
    @DeliveryMode NVARCHAR(20),
    @Status NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblDeliveries (Id, OrderGuid, RiderGuid, DeliveryMode, Status, CreatedAt, UpdatedAt)
    VALUES (@Id, @OrderGuid, @RiderGuid, @DeliveryMode, @Status, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspCountDeclinedAssignments
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblAssignments WHERE OrderGuid = @OrderGuid AND Status = N'DECLINED';
END
GO

CREATE OR ALTER PROCEDURE uspUpdateTripStatus
    @Id UNIQUEIDENTIFIER,
    @Status NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblDeliveries SET Status = @Status, UpdatedAt = SYSUTCDATETIME() WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspGetTrip
    @OrderGuid UNIQUEIDENTIFIER,
    @RiderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblDeliveries WHERE OrderGuid = @OrderGuid AND RiderGuid = @RiderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspUpdateTracking
    @OrderGuid UNIQUEIDENTIFIER,
    @RiderGuid UNIQUEIDENTIFIER,
    @Latitude DECIMAL(9,6),
    @Longitude DECIMAL(9,6)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblDeliveries
    SET Latitude = @Latitude, Longitude = @Longitude, UpdatedAt = SYSUTCDATETIME()
    WHERE OrderGuid = @OrderGuid AND RiderGuid = @RiderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspReviewRider
    @RiderId NVARCHAR(20),
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblRiders SET Status = @Status, UpdatedAt = SYSUTCDATETIME() WHERE RiderId = @RiderId;
    SELECT * FROM tblRiders WHERE RiderId = @RiderId;
END
GO

CREATE OR ALTER PROCEDURE uspGetAvailableRiders
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblRiders WHERE Status = N'APPROVED' AND Available = 1 AND Latitude IS NOT NULL;
END
GO

CREATE OR ALTER PROCEDURE uspGetKitchenLocationForOrder
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT k.Latitude, k.Longitude
    FROM tblOrders o
    INNER JOIN tblKitchens k ON k.Id = o.KitchenGuid
    WHERE o.Id = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspInsertAssignment
    @Id UNIQUEIDENTIFIER,
    @AssignmentId NVARCHAR(20),
    @OrderGuid UNIQUEIDENTIFIER,
    @RiderGuid UNIQUEIDENTIFIER,
    @TtlSeconds INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblAssignments (Id, AssignmentId, OrderGuid, RiderGuid, Status, ExpiresAt, CreatedAt)
    VALUES (@Id, @AssignmentId, @OrderGuid, @RiderGuid, N'OFFERED', DATEADD(SECOND, @TtlSeconds, SYSUTCDATETIME()), SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspFallbackKitchenSelf
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOrders SET DeliveryMode = N'KITCHEN_SELF', UpdatedAt = SYSUTCDATETIME() WHERE Id = @OrderGuid;
    IF NOT EXISTS (SELECT 1 FROM tblDeliveries WHERE OrderGuid = @OrderGuid)
        INSERT INTO tblDeliveries (Id, OrderGuid, DeliveryMode, Status, CreatedAt, UpdatedAt)
        VALUES (NEWID(), @OrderGuid, N'KITCHEN_SELF', N'ASSIGNED', SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderAddress
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrderId, DeliveryAddress FROM tblOrders WHERE Id = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspGetRider
    @RiderId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblRiders WHERE RiderId = @RiderId;
END
GO

/* Subscription / ReviewController */

CREATE OR ALTER PROCEDURE uspGetPlans
    @Audience NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM masPlans WHERE @Audience IS NULL OR Audience = @Audience ORDER BY Price;
END
GO

CREATE OR ALTER PROCEDURE uspGetPlan
    @PlanCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM masPlans WHERE PlanCode = @PlanCode;
END
GO

CREATE OR ALTER PROCEDURE uspPurchase
    @Id UNIQUEIDENTIFIER,
    @SubscriptionId NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER,
    @PlanCode NVARCHAR(50),
    @Audience NVARCHAR(20),
    @ValidUntil DATETIME2,
    @AutoRenew BIT,
    @CreatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblSubscriptions (Id, SubscriptionId, UserId, PlanCode, Audience, ValidUntil, AutoRenew, CreatedAt)
    VALUES (@Id, @SubscriptionId, @UserId, @PlanCode, @Audience, @ValidUntil, @AutoRenew, @CreatedAt);
    SELECT * FROM tblSubscriptions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE uspGetSubscriptionMe
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblSubscriptions WHERE UserId = @UserId AND ValidUntil > SYSUTCDATETIME() ORDER BY ValidUntil DESC;
END
GO

CREATE OR ALTER PROCEDURE uspCancelSubscription
    @SubscriptionId NVARCHAR(20),
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblSubscriptions SET AutoRenew = 0 WHERE SubscriptionId = @SubscriptionId AND UserId = @UserId;
    SELECT @@ROWCOUNT AS Updated;
END
GO

CREATE OR ALTER PROCEDURE uspHasActiveHomelyPlus
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM tblSubscriptions
    WHERE UserId = @UserId AND Audience = N'CUSTOMER' AND ValidUntil > SYSUTCDATETIME();
END
GO

CREATE OR ALTER PROCEDURE uspIsFeaturedKitchen
    @KitchenOwnerUserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1)
    FROM tblSubscriptions
    WHERE UserId = @KitchenOwnerUserId AND Audience = N'KITCHEN' AND ValidUntil > SYSUTCDATETIME();
END
GO

/* Settlements */

CREATE OR ALTER PROCEDURE uspCountLedgerLines
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) FROM tblLedgerLines WHERE OrderGuid = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspGetOrderSettlementFacts
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT KitchenGuid, ItemTotal, DeliveryFee, DeliveryMode FROM tblOrders WHERE Id = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspGetDeliveryRider
    @OrderGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RiderGuid FROM tblDeliveries WHERE OrderGuid = @OrderGuid;
END
GO

CREATE OR ALTER PROCEDURE uspInsertLedgerLine
    @PartyType NVARCHAR(20),
    @PartyGuid UNIQUEIDENTIFIER,
    @OrderGuid UNIQUEIDENTIFIER,
    @Amount DECIMAL(10,2),
    @Description NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblLedgerLines (PartyType, PartyGuid, OrderGuid, Amount, Description, CreatedAt)
    VALUES (@PartyType, @PartyGuid, @OrderGuid, @Amount, @Description, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetLedger
    @PartyType NVARCHAR(20),
    @PartyGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderId, l.Amount, l.Description, l.CreatedAt
    FROM tblLedgerLines l
    INNER JOIN tblOrders o ON o.Id = l.OrderGuid
    WHERE l.PartyType = @PartyType AND l.PartyGuid = @PartyGuid
    ORDER BY l.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspRunSettlements
    @Start DATETIME2,
    @End DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PartyType, PartyGuid, SUM(Amount) AS Amount
    FROM tblLedgerLines
    WHERE CreatedAt >= @Start AND CreatedAt <= @End
    GROUP BY PartyType, PartyGuid;
END
GO

CREATE OR ALTER PROCEDURE uspInsertSettlement
    @Id UNIQUEIDENTIFIER,
    @SettlementId NVARCHAR(20),
    @PartyType NVARCHAR(20),
    @PartyGuid UNIQUEIDENTIFIER,
    @PeriodStart DATETIME2,
    @PeriodEnd DATETIME2,
    @Amount DECIMAL(10,2),
    @Status NVARCHAR(20),
    @InvoiceUrl NVARCHAR(500) = NULL,
    @Utr NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblSettlements
        (Id, SettlementId, PartyType, PartyGuid, PeriodStart, PeriodEnd, Amount, Status, InvoiceUrl, Utr, CreatedAt)
    VALUES
        (@Id, @SettlementId, @PartyType, @PartyGuid, @PeriodStart, @PeriodEnd, @Amount, @Status, @InvoiceUrl, @Utr, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE uspGetSettlements
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettlementId, PartyType, Amount, Status, PeriodStart, PeriodEnd, InvoiceUrl, Utr
    FROM tblSettlements
    ORDER BY CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE uspGetSettlement
    @SettlementId NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettlementId, PartyType, Amount, Status, PeriodStart, PeriodEnd, InvoiceUrl, Utr
    FROM tblSettlements
    WHERE SettlementId = @SettlementId;
END
GO

CREATE OR ALTER PROCEDURE uspSeedAdminPassword
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers
    SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME()
    WHERE MobileNumber = N'9999999999' AND Role = N'ADMIN' AND PasswordHash IS NULL;
END
GO


GO

USE [RasoiMitra];
GO

/* Lookup / master data required by the Homely APIs */

IF NOT EXISTS (SELECT 1 FROM masRoles)
BEGIN
    INSERT INTO masRoles (Code, Name, DisplayOrder) VALUES
        (N'CUSTOMER', N'Customer', 1),
        (N'KITCHEN_OWNER', N'Kitchen Owner', 2),
        (N'RIDER', N'Rider', 3),
        (N'ADMIN', N'Admin', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masAuthPurposes)
BEGIN
    INSERT INTO masAuthPurposes (Code, Name, DisplayOrder) VALUES
        (N'CUSTOMER', N'Customer registration / login', 1),
        (N'KITCHEN_OWNER', N'Kitchen owner registration / login', 2),
        (N'RIDER', N'Rider registration / login', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masUserStatuses)
BEGIN
    INSERT INTO masUserStatuses (Code, Name, StatusValue, DisplayOrder) VALUES
        (N'ACTIVE', N'Active', 1, 1),
        (N'INACTIVE', N'Inactive', 2, 2),
        (N'BLOCKED', N'Blocked', 3, 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masKitchenTypes)
BEGIN
    INSERT INTO masKitchenTypes (Code, Name, DisplayOrder) VALUES
        (N'HOMEMADE', N'Homemade kitchen', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM masKitchenStatuses)
BEGIN
    INSERT INTO masKitchenStatuses (Code, Name, DisplayOrder) VALUES
        (N'DRAFT', N'Draft', 1),
        (N'MOBILE_VERIFIED', N'Mobile verified', 2),
        (N'SUBMITTED', N'Submitted', 3),
        (N'DOCUMENT_VERIFICATION_PENDING', N'Document verification pending', 4),
        (N'UNDER_REVIEW', N'Under review', 5),
        (N'ADDITIONAL_DOCUMENTS_REQUIRED', N'Additional documents required', 6),
        (N'RESUBMITTED', N'Resubmitted', 7),
        (N'REJECTED', N'Rejected', 8),
        (N'APPROVED', N'Approved', 9),
        (N'MENU_SETUP', N'Menu setup', 10),
        (N'ACTIVE', N'Active', 11);
END
GO

IF NOT EXISTS (SELECT 1 FROM masDocumentTypes)
BEGIN
    INSERT INTO masDocumentTypes (Code, Name, Audience, IsRequired, DisplayOrder) VALUES
        (N'KITCHEN_PHOTO', N'Kitchen photo', N'KITCHEN', 1, 1),
        (N'PAN_CARD', N'PAN card', N'KITCHEN', 1, 2),
        (N'FSSAI_LICENSE', N'FSSAI license', N'KITCHEN', 0, 3),
        (N'AADHAAR', N'Aadhaar', N'RIDER', 1, 4),
        (N'DRIVING_LICENCE', N'Driving licence', N'RIDER', 1, 5),
        (N'VEHICLE_RC', N'Vehicle RC', N'RIDER', 1, 6),
        (N'SELFIE', N'Selfie', N'RIDER', 1, 7);
END
GO

IF NOT EXISTS (SELECT 1 FROM masCuisines)
BEGIN
    INSERT INTO masCuisines (Code, Name, DisplayOrder) VALUES
        (N'NORTH_INDIAN', N'North Indian', 1),
        (N'SOUTH_INDIAN', N'South Indian', 2),
        (N'GUJARATI', N'Gujarati', 3),
        (N'RAJASTHANI', N'Rajasthani', 4),
        (N'BENGALI', N'Bengali', 5),
        (N'PUNJABI', N'Punjabi', 6),
        (N'MAHARASHTRIAN', N'Maharashtrian', 7),
        (N'CHINESE', N'Chinese', 8),
        (N'CONTINENTAL', N'Continental', 9),
        (N'STREET_FOOD', N'Street Food', 10),
        (N'SWEETS', N'Sweets', 11),
        (N'OTHER', N'Other', 12);
END
GO

IF NOT EXISTS (SELECT 1 FROM masMenuItemStatuses)
BEGIN
    INSERT INTO masMenuItemStatuses (Code, Name, DisplayOrder) VALUES
        (N'DRAFT', N'Draft', 1),
        (N'PUBLISHED', N'Published', 2),
        (N'DELETED', N'Deleted', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masAddressLabels)
BEGIN
    INSERT INTO masAddressLabels (Code, Name, DisplayOrder) VALUES
        (N'HOME', N'Home', 1),
        (N'WORK', N'Work', 2),
        (N'OTHER', N'Other', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masOrderStatuses)
BEGIN
    INSERT INTO masOrderStatuses (Code, Name, DisplayOrder) VALUES
        (N'PAYMENT_PENDING', N'Payment pending', 1),
        (N'PAYMENT_FAILED', N'Payment failed', 2),
        (N'PLACED', N'Placed', 3),
        (N'ACCEPTED', N'Accepted', 4),
        (N'PREPARING', N'Preparing', 5),
        (N'READY', N'Ready', 6),
        (N'OUT_FOR_DELIVERY', N'Out for delivery', 7),
        (N'DELIVERED', N'Delivered', 8),
        (N'CANCELLED', N'Cancelled', 9),
        (N'REJECTED', N'Rejected', 10),
        (N'REFUNDED', N'Refunded', 11);
END
GO

IF NOT EXISTS (SELECT 1 FROM masPaymentMethods)
BEGIN
    INSERT INTO masPaymentMethods (Code, Name, DisplayOrder) VALUES
        (N'UPI', N'UPI', 1),
        (N'CARD', N'Card', 2),
        (N'NETBANKING', N'Net banking', 3),
        (N'WALLET', N'Wallet', 4),
        (N'COD', N'Cash on delivery', 5);
END
GO

IF NOT EXISTS (SELECT 1 FROM masPaymentStatuses)
BEGIN
    INSERT INTO masPaymentStatuses (Code, Name, DisplayOrder) VALUES
        (N'PENDING', N'Pending', 1),
        (N'SUCCESS', N'Success', 2),
        (N'FAILED', N'Failed', 3),
        (N'REFUNDED', N'Refunded', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masDeliveryModes)
BEGIN
    INSERT INTO masDeliveryModes (Code, Name, DisplayOrder) VALUES
        (N'RIDER', N'Rider delivery', 1),
        (N'KITCHEN_SELF', N'Kitchen self delivery', 2);
END
GO

IF NOT EXISTS (SELECT 1 FROM masVehicleTypes)
BEGIN
    INSERT INTO masVehicleTypes (Code, Name, DisplayOrder) VALUES
        (N'BIKE', N'Bike', 1),
        (N'SCOOTER', N'Scooter', 2),
        (N'CYCLE', N'Cycle', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masRiderStatuses)
BEGIN
    INSERT INTO masRiderStatuses (Code, Name, DisplayOrder) VALUES
        (N'DRAFT', N'Draft', 1),
        (N'SUBMITTED', N'Submitted', 2),
        (N'APPROVED', N'Approved', 3),
        (N'REJECTED', N'Rejected', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masAssignmentStatuses)
BEGIN
    INSERT INTO masAssignmentStatuses (Code, Name, DisplayOrder) VALUES
        (N'OFFERED', N'Offered', 1),
        (N'ACCEPTED', N'Accepted', 2),
        (N'DECLINED', N'Declined', 3),
        (N'EXPIRED', N'Expired', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masTripStatuses)
BEGIN
    INSERT INTO masTripStatuses (Code, Name, DisplayOrder) VALUES
        (N'ASSIGNED', N'Assigned', 1),
        (N'ARRIVED_KITCHEN', N'Arrived at kitchen', 2),
        (N'PICKED_UP', N'Picked up', 3),
        (N'DELIVERED', N'Delivered', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masSettlementStatuses)
BEGIN
    INSERT INTO masSettlementStatuses (Code, Name, DisplayOrder) VALUES
        (N'PENDING', N'Pending', 1),
        (N'PAID', N'Paid', 2),
        (N'FAILED', N'Failed', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masPlans)
BEGIN
    INSERT INTO masPlans (PlanCode, Audience, Name, Price, DurationDays) VALUES
        (N'HOMELY_PLUS_MONTHLY', N'CUSTOMER', N'HomelyPlus Monthly', 149, 30),
        (N'HOMELY_PLUS_QUARTERLY', N'CUSTOMER', N'HomelyPlus Quarterly', 399, 90),
        (N'HOMELY_PLUS_ANNUAL', N'CUSTOMER', N'HomelyPlus Annual', 999, 365),
        (N'FEATURED_MONTHLY', N'KITCHEN', N'Featured Kitchen Monthly', 499, 30);
END
GO

IF NOT EXISTS (SELECT 1 FROM tblUsers WHERE MobileNumber = N'9999999999' AND Role = N'ADMIN')
BEGIN
    INSERT INTO tblUsers (Id, UserCode, FullName, MobileNumber, Email, Role, Status, CreatedAt, UpdatedAt)
    VALUES (
        '11111111-1111-1111-1111-111111111111',
        N'ADM10001',
        N'HOMELY Admin',
        N'9999999999',
        N'admin@homely.local',
        N'ADMIN',
        1,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END
GO

