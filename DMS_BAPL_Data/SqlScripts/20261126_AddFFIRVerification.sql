IF OBJECT_ID(N'dbo.FFIRVerification', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FFIRVerification
    (
        FFIRId INT NOT NULL,
        Status VARCHAR(10) NOT NULL
            CONSTRAINT DF_FFIRVerification_Status DEFAULT ('Pending'),
        Remarks NVARCHAR(1000) NULL,
        ReviewedBy VARCHAR(100) NULL,
        ReviewedDate DATETIME NULL,
        CONSTRAINT PK_FFIRVerification PRIMARY KEY (FFIRId),
        CONSTRAINT CK_FFIRVerification_Status
            CHECK (Status IN ('Pending', 'Approved', 'Rejected'))
    );
END;

IF COL_LENGTH(N'dbo.FFIRVerification', N'Remarks') IS NULL
BEGIN
    ALTER TABLE dbo.FFIRVerification ADD Remarks NVARCHAR(1000) NULL;
END;
