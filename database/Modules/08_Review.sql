USE [HomelyFood];
GO

IF OBJECT_ID(N'revReviews', N'U') IS NULL
BEGIN
    CREATE TABLE revReviews
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        KitchenGuid     UNIQUEIDENTIFIER NOT NULL,
        CustomerUserId  UNIQUEIDENTIFIER NOT NULL,
        Rating          INT              NOT NULL,
        RiderRating     INT              NULL,
        Comments        NVARCHAR(1000)   NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Reviews_OrderGuid UNIQUE (OrderGuid),
        CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5),
        CONSTRAINT FK_Reviews_Order FOREIGN KEY (OrderGuid) REFERENCES ordOrders (Id)
    );
END
GO

IF OBJECT_ID(N'mstPlans', N'U') IS NULL
BEGIN
    CREATE TABLE mstPlans
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        PlanCode        VARCHAR(200)      NOT NULL,
        Audience        VARCHAR(200)      NOT NULL,
        Name            VARCHAR(200)      NOT NULL,
        Price           NUMERIC(18,2)     NOT NULL,
        DurationDays    INT               NOT NULL
    );

    CREATE INDEX IX_Plan ON mstPlans (PlanCode, Audience, Price DESC);
END
GO

IF OBJECT_ID(N'subSubscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE subSubscriptions
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Subscriptions PRIMARY KEY,
        SubscriptionId  NVARCHAR(20)     NOT NULL,
        UserId          UNIQUEIDENTIFIER NOT NULL,
        PlanCode        NVARCHAR(50)     NOT NULL,
        Audience        NVARCHAR(20)     NOT NULL,
        ValidUntil      DATETIME2        NOT NULL,
        AutoRenew       BIT              NOT NULL CONSTRAINT DF_Subscriptions_Renew DEFAULT (1),
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Subscriptions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Subscriptions_SubscriptionId UNIQUE (SubscriptionId),
        CONSTRAINT FK_Subscriptions_User FOREIGN KEY (UserId) REFERENCES mstUsers (Id)
    );
END
GO

IF OBJECT_ID(N'setlLedgerLines', N'U') IS NULL
BEGIN
    CREATE TABLE setlLedgerLines
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LedgerLines PRIMARY KEY,
        PartyType   NVARCHAR(20)         NOT NULL,
        PartyGuid   UNIQUEIDENTIFIER     NOT NULL,
        OrderGuid   UNIQUEIDENTIFIER     NOT NULL,
        Amount      DECIMAL(10,2)        NOT NULL,
        Description NVARCHAR(200)        NOT NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_Ledger_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID(N'setlSettlements', N'U') IS NULL
BEGIN
    CREATE TABLE setlSettlements
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Settlements PRIMARY KEY,
        SettlementId    NVARCHAR(20)     NOT NULL,
        PartyType       NVARCHAR(20)     NOT NULL,
        PartyGuid       UNIQUEIDENTIFIER NOT NULL,
        PeriodStart     DATETIME2        NOT NULL,
        PeriodEnd       DATETIME2        NOT NULL,
        Amount          DECIMAL(10,2)    NOT NULL,
        Status          NVARCHAR(20)     NOT NULL,
        InvoiceUrl      NVARCHAR(500)    NULL,
        Utr             NVARCHAR(50)     NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Settlements_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Settlements_SettlementId UNIQUE (SettlementId)
    );
END
GO
