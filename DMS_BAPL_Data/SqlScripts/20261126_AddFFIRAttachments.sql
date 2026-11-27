SELECT DB_NAME() AS CurrentDatabase;
GO

IF OBJECT_ID('dbo.FFIRHeader', 'U') IS NULL
    THROW 50001, 'dbo.FFIRHeader was not found. Select the DMS database before running this script.', 1;
GO

IF OBJECT_ID('dbo.FFIRAttachment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FFIRAttachment
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FFIRAttachment PRIMARY KEY,
        FFIRId int NOT NULL,
        DealerCode nvarchar(50) NULL,
        FileName nvarchar(260) NOT NULL,
        FileUrl nvarchar(1000) NOT NULL,
        ContentType nvarchar(100) NOT NULL,
        FileSize bigint NOT NULL,
        MediaType nvarchar(10) NOT NULL,
        CreatedBy nvarchar(100) NOT NULL,
        CreatedDate datetime2(0) NOT NULL CONSTRAINT DF_FFIRAttachment_CreatedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted bit NOT NULL CONSTRAINT DF_FFIRAttachment_IsDeleted DEFAULT 0,
        CONSTRAINT FK_FFIRAttachment_FFIRHeader FOREIGN KEY (FFIRId)
            REFERENCES dbo.FFIRHeader(Id),
        CONSTRAINT CK_FFIRAttachment_MediaType CHECK (MediaType IN ('Image', 'Video'))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.FFIRAttachment')
      AND name = 'IX_FFIRAttachment_FFIRId_MediaType_IsDeleted'
)
BEGIN
    CREATE INDEX IX_FFIRAttachment_FFIRId_MediaType_IsDeleted
        ON dbo.FFIRAttachment (FFIRId, MediaType, IsDeleted);
END;
GO
