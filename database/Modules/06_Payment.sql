USE [HomelyFood];
GO

IF OBJECT_ID(N'payPayments', N'U') IS NULL
BEGIN
    CREATE TABLE payPayments
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        PaymentId           NVARCHAR(20)     NOT NULL,
        OrderGuid           UNIQUEIDENTIFIER NOT NULL,
        UserId              UNIQUEIDENTIFIER NOT NULL,
        Amount              DECIMAL(10,2)    NOT NULL,
        Method              NVARCHAR(20)     NOT NULL,
        Status              NVARCHAR(20)     NOT NULL,
        GatewayReference    NVARCHAR(100)    NULL,
        FailureReason       NVARCHAR(500)    NULL,
        AttemptCount        INT              NOT NULL CONSTRAINT DF_Payments_Attempt DEFAULT (1),
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_Payments_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Payments_PaymentId UNIQUE (PaymentId),
        CONSTRAINT FK_Payments_Order FOREIGN KEY (OrderGuid) REFERENCES ordOrders (Id)
    );
END
GO
