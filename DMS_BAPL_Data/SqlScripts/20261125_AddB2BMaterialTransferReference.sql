-- Adds B2B source references for Material Transfer and Repair Bill.
SELECT DB_NAME() AS CurrentDatabase;

IF OBJECT_ID('dbo.MaterialTransfer', 'U') IS NULL
    THROW 50001, 'dbo.MaterialTransfer was not found. Select the DMS database before running this script.', 1;

IF OBJECT_ID('dbo.RepairBillHeader', 'U') IS NULL
    THROW 50002, 'dbo.RepairBillHeader was not found. Select the DMS database before running this script.', 1;
GO

IF COL_LENGTH('dbo.MaterialTransfer', 'B2BJobCardId') IS NULL
BEGIN
    ALTER TABLE dbo.MaterialTransfer
        ADD B2BJobCardId uniqueidentifier NULL;
END;
GO

IF COL_LENGTH('dbo.RepairBillHeader', 'B2BJobCardId') IS NULL
BEGIN
    ALTER TABLE dbo.RepairBillHeader
        ADD B2BJobCardId uniqueidentifier NULL;
END;
GO

IF COL_LENGTH('dbo.RepairBillHeader', 'B2BJobCardNumber') IS NULL
BEGIN
    ALTER TABLE dbo.RepairBillHeader
        ADD B2BJobCardNumber nvarchar(100) NULL;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.RepairBillHeader')
      AND name = 'JobId'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.RepairBillHeader
        ALTER COLUMN JobId int NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.RepairBillHeader')
      AND name = 'IX_RepairBillHeader_B2BJobCardId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_RepairBillHeader_B2BJobCardId
        ON dbo.RepairBillHeader (B2BJobCardId)
        WHERE B2BJobCardId IS NOT NULL;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.MaterialTransfer')
      AND name = 'JobId'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.MaterialTransfer
        ALTER COLUMN JobId int NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.MaterialTransfer')
      AND name = 'IX_MaterialTransfer_B2BJobCardId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_MaterialTransfer_B2BJobCardId
        ON dbo.MaterialTransfer (B2BJobCardId)
        WHERE B2BJobCardId IS NOT NULL;
END;
GO
