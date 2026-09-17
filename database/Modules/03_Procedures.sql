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
