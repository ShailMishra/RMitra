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
