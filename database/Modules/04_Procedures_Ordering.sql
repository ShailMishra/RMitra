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
