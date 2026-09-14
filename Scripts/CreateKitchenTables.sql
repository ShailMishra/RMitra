-- RasoiMitra Kitchen Registration & Approval Workflow Tables
-- Run this script against your SQL Server database before using Kitchen APIs.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RM_Kitchens')
BEGIN
    CREATE TABLE RM_Kitchens
    (
        KitchenId                   NVARCHAR(20)    NOT NULL PRIMARY KEY,
        KitchenName                 NVARCHAR(200)   NOT NULL,
        OwnerName                   NVARCHAR(200)   NOT NULL,
        MobileNumber                NVARCHAR(15)    NOT NULL,
        Email                       NVARCHAR(200)   NOT NULL,
        AddressLine1                NVARCHAR(500)   NOT NULL,
        AddressLine2                NVARCHAR(500)   NULL,
        City                        NVARCHAR(100)   NOT NULL,
        State                       NVARCHAR(100)   NOT NULL,
        Pincode                     NVARCHAR(10)    NOT NULL,
        KitchenType                 NVARCHAR(100)   NOT NULL,
        CuisineTypes                NVARCHAR(MAX)   NOT NULL,
        OperatingHours              NVARCHAR(MAX)   NOT NULL,
        BankDetails                 NVARCHAR(MAX)   NOT NULL,
        PanCard                     NVARCHAR(10)    NOT NULL,
        KitchenPhoto                NVARCHAR(1000)  NOT NULL,
        Status                      NVARCHAR(50)    NOT NULL DEFAULT 'DRAFT',
        VerificationToken           NVARCHAR(100)   NULL,
        RejectionReason             NVARCHAR(1000)  NULL,
        AdditionalDocumentsRequired NVARCHAR(MAX)   NULL,
        AdminRemarks                NVARCHAR(1000)  NULL,
        ResubmittedDocuments        NVARCHAR(MAX)   NULL,
        CreatedAt                   DATETIME2       NOT NULL,
        UpdatedAt                   DATETIME2       NOT NULL,
        SubmittedAt                 DATETIME2       NULL
    );

    CREATE INDEX IX_RM_Kitchens_Status ON RM_Kitchens (Status);
    CREATE INDEX IX_RM_Kitchens_MobileNumber ON RM_Kitchens (MobileNumber);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RM_KitchenStatusHistory')
BEGIN
    CREATE TABLE RM_KitchenStatusHistory
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        KitchenId   NVARCHAR(20)         NOT NULL,
        Status      NVARCHAR(50)         NOT NULL,
        Remarks     NVARCHAR(1000)       NULL,
        CreatedAt   DATETIME2            NOT NULL,
        CONSTRAINT FK_RM_KitchenStatusHistory_Kitchen
            FOREIGN KEY (KitchenId) REFERENCES RM_Kitchens (KitchenId)
    );

    CREATE INDEX IX_RM_KitchenStatusHistory_KitchenId ON RM_KitchenStatusHistory (KitchenId);
END
GO

IF COL_LENGTH('RM_Kitchens', 'ResubmittedDocuments') IS NULL
BEGIN
    ALTER TABLE RM_Kitchens ADD ResubmittedDocuments NVARCHAR(MAX) NULL;
END
GO
