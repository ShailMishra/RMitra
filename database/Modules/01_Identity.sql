USE [HomelyFood];
GO

IF OBJECT_ID(N'mstUsers', N'U') IS NULL
BEGIN
    CREATE TABLE mstUsers
    (
        Id              UNIQUEIDENTIFIER    NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserCode        NVARCHAR(20)        NOT NULL,
        FullName        NVARCHAR(200)       NOT NULL,
        MobileNumber    NVARCHAR(15)        NOT NULL,
        Email           NVARCHAR(200)       NULL,
        Role            NVARCHAR(50)        NOT NULL,
        PasswordHash    NVARCHAR(500)       NULL,
        Status          INT                 NOT NULL CONSTRAINT DF_Users_Status DEFAULT (1),
        CreatedAt       DATETIME2           NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2           NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Users_UserCode UNIQUE (UserCode),
        CONSTRAINT UQ_Users_Mobile_Role UNIQUE (MobileNumber, Role)
    );
END
GO

IF OBJECT_ID(N'mstOtpRequests', N'U') IS NULL
BEGIN
    CREATE TABLE mstOtpRequests
    (
        Id                  UNIQUEIDENTIFIER    NOT NULL CONSTRAINT PK_OtpRequests PRIMARY KEY,
        RequestId           NVARCHAR(40)        NOT NULL,
        MobileNumber        NVARCHAR(15)        NOT NULL,
        Purpose             NVARCHAR(50)        NOT NULL,
        OtpHash             NVARCHAR(128)       NOT NULL,
        AttemptCount        INT                 NOT NULL CONSTRAINT DF_Otp_Attempt DEFAULT (0),
        ExpiresAt           DATETIME2           NOT NULL,
        VerifiedAt          DATETIME2           NULL,
        VerificationToken   NVARCHAR(64)        NULL,
        TokenExpiresAt      DATETIME2           NULL,
        CreatedAt           DATETIME2           NOT NULL CONSTRAINT DF_Otp_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_OtpRequests_RequestId UNIQUE (RequestId)
    );

    CREATE INDEX IX_OtpRequests_Mobile_Purpose ON mstOtpRequests (MobileNumber, Purpose, CreatedAt DESC);
END
GO
