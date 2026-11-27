-- 27.11.2026  Employee-wise menu rights (Role Menu Rights screen -> Role + Employee)
-- Run once on the DMS database (BAPLDMSvad).
-- When an employee has rows here, ONLY these rows decide his menus, pages and buttons.
-- No rows = he gets the rights of the roles ticked for him in BG Employee Master.
-- Permission bitmask: View=1, Add=2, Edit=4, Delete=8, Print/Download=16 (31 = all)
IF OBJECT_ID('dbo.BgEmployeeMenuRight', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BgEmployeeMenuRight
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BgEmployeeMenuRight PRIMARY KEY,
        BgEmployeeId  INT           NOT NULL,
        MenuId        INT           NOT NULL,
        SubMenuId     INT           NOT NULL,
        Permission    INT           NOT NULL,
        CreatedBy     NVARCHAR(100) NULL,
        CreatedDate   DATETIME      NOT NULL CONSTRAINT DF_BgEmployeeMenuRight_CreatedDate DEFAULT (GETDATE()),
        CONSTRAINT FK_BgEmployeeMenuRight_Employee FOREIGN KEY (BgEmployeeId) REFERENCES dbo.BgEmployeeMaster (Id) ON DELETE CASCADE,
        CONSTRAINT UQ_BgEmployeeMenuRight_Employee_SubMenu UNIQUE (BgEmployeeId, SubMenuId)
    );
END
GO
