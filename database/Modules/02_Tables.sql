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
