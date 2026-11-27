-- Adds a B2B JobCardScanner reference to DMS dbo.FFIRHeader.
-- Run against the DMS database. This script is provided for manual execution and is not run by the application.
SELECT DB_NAME() AS CurrentDatabase;

IF OBJECT_ID('dbo.FFIRHeader', 'U') IS NULL
    THROW 50001, 'dbo.FFIRHeader was not found. Select the DMS database before running this script.', 1;
GO

IF COL_LENGTH('dbo.FFIRHeader', 'B2BJobCardId') IS NULL
BEGIN
    ALTER TABLE dbo.FFIRHeader
        ADD B2BJobCardId uniqueidentifier NULL;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.FFIRHeader')
      AND name = 'JobCardHeaderId'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.FFIRHeader
        ALTER COLUMN JobCardHeaderId int NULL;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.FFIRHeader')
      AND name = 'JobCardCustomerId'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.FFIRHeader
        ALTER COLUMN JobCardCustomerId int NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.FFIRHeader')
      AND name = 'IX_FFIRHeader_B2BJobCardId'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_FFIRHeader_B2BJobCardId
        ON dbo.FFIRHeader (B2BJobCardId)
        WHERE B2BJobCardId IS NOT NULL;
END;
GO
