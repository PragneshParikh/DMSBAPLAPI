/* BgEmployeeMaster_WidenColumns.sql   27.11.2026
   A National / Zonal Head maps hundreds of dealers; these columns hold comma lists and were too short
   (DealerCode 200, LocationCode / MappedEmployeeIds / MappedZoneIds 500) -> 'String or binary data would be truncated'.
   Safe to run more than once. Run before restarting the API. */

ALTER TABLE dbo.BgEmployeeMaster ALTER COLUMN DealerCode        VARCHAR(MAX) NULL;
ALTER TABLE dbo.BgEmployeeMaster ALTER COLUMN LocationCode      VARCHAR(MAX) NULL;
ALTER TABLE dbo.BgEmployeeMaster ALTER COLUMN MappedEmployeeIds VARCHAR(MAX) NULL;
ALTER TABLE dbo.BgEmployeeMaster ALTER COLUMN MappedZoneIds     VARCHAR(MAX) NULL;

-- check: all four should show max_length = -1 (MAX)
SELECT c.name, t.name AS type_name, c.max_length
FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.BgEmployeeMaster') AND c.name IN ('DealerCode','LocationCode','MappedEmployeeIds','MappedZoneIds');
