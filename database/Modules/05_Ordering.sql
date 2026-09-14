USE [HomelyFood];
GO

IF OBJECT_ID(N'ordCarts', N'U') IS NULL
BEGIN
    CREATE TABLE ordCarts
    (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Carts PRIMARY KEY,
        UserId      UNIQUEIDENTIFIER NOT NULL,
        KitchenGuid UNIQUEIDENTIFIER NULL,
        UpdatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Carts_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Carts_UserId UNIQUE (UserId),
        CONSTRAINT FK_Carts_User FOREIGN KEY (UserId) REFERENCES mstUsers (Id)
    );
END
GO

IF OBJECT_ID(N'ordCartItems', N'U') IS NULL
BEGIN
    CREATE TABLE ordCartItems
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CartItems PRIMARY KEY,
        CartId          UNIQUEIDENTIFIER NOT NULL,
        MenuItemGuid    UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(200)    NOT NULL,
        UnitPrice       DECIMAL(10,2)    NOT NULL,
        Quantity        INT              NOT NULL,
        CONSTRAINT FK_CartItems_Cart FOREIGN KEY (CartId) REFERENCES ordCarts (Id)
    );
END
GO

IF OBJECT_ID(N'ordOrders', N'U') IS NULL
BEGIN
    CREATE TABLE ordOrders
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        OrderId             NVARCHAR(20)     NOT NULL,
        CustomerUserId      UNIQUEIDENTIFIER NOT NULL,
        KitchenGuid         UNIQUEIDENTIFIER NOT NULL,
        KitchenPublicId     NVARCHAR(20)     NOT NULL,
        AddressGuid         UNIQUEIDENTIFIER NOT NULL,
        DeliveryAddress     NVARCHAR(1000)   NOT NULL,
        ItemTotal           DECIMAL(10,2)    NOT NULL,
        DiscountAmount      DECIMAL(10,2)    NOT NULL,
        DeliveryFee         DECIMAL(10,2)    NOT NULL,
        TaxAmount           DECIMAL(10,2)    NOT NULL CONSTRAINT DF_Orders_Tax DEFAULT (0),
        TipAmount           DECIMAL(10,2)    NOT NULL CONSTRAINT DF_Orders_Tip DEFAULT (0),
        GrandTotal          DECIMAL(10,2)    NOT NULL,
        PaymentMethod       NVARCHAR(20)     NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        DeliveryMode        NVARCHAR(20)     NOT NULL CONSTRAINT DF_Orders_Mode DEFAULT (N'RIDER'),
        Instructions        NVARCHAR(500)    NULL,
        CancelReason        NVARCHAR(500)    NULL,
        IdempotencyKey      NVARCHAR(100)    NULL,
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_Orders_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        AcceptedDeadlineAt  DATETIME2        NULL,
        CONSTRAINT UQ_Orders_OrderId UNIQUE (OrderId),
        CONSTRAINT FK_Orders_Customer FOREIGN KEY (CustomerUserId) REFERENCES mstUsers (Id),
        CONSTRAINT FK_Orders_Kitchen FOREIGN KEY (KitchenGuid) REFERENCES kitKitchens (Id)
    );

    CREATE INDEX IX_Orders_Idempotency ON ordOrders (CustomerUserId, IdempotencyKey);
END
GO

IF OBJECT_ID(N'ordOrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE ordOrderItems
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderGuid       UNIQUEIDENTIFIER NOT NULL,
        MenuItemGuid    UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(200)    NOT NULL,
        UnitPrice       DECIMAL(10,2)    NOT NULL,
        Quantity        INT              NOT NULL,
        LineTotal       DECIMAL(10,2)    NOT NULL,
        CONSTRAINT FK_OrderItems_Order FOREIGN KEY (OrderGuid) REFERENCES ordOrders (Id)
    );
END
GO

IF OBJECT_ID(N'ordOrderStatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE ordOrderStatusHistory
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderStatusHistory PRIMARY KEY,
        OrderGuid   UNIQUEIDENTIFIER     NOT NULL,
        Status      NVARCHAR(50)         NOT NULL,
        Remarks     NVARCHAR(500)        NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_OrderHistory_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_OrderHistory_Order FOREIGN KEY (OrderGuid) REFERENCES ordOrders (Id)
    );
END
GO
