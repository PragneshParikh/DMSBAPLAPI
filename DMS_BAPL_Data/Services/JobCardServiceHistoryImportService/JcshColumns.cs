using System.Collections.Generic;
using System.Linq;

namespace DMS_BAPL_Data.Services.JobCardServiceHistoryImportService
{
    public enum JcshType { Int, Text, Date, DateTime, Decimal, Bit }

    /// <summary>One of the 63 Excel columns and the BAPLDMSvad.dbo.JobCardServiceHistory column it is stored in.</summary>
    public sealed record JcshColumn(string Key, string Db, JcshType Type, int Length, int Precision, int Scale, bool Required, string Group)
    {
        public string SqlType => Type switch
        {
            JcshType.Int => "int",
            JcshType.Text => $"nvarchar({Length})",
            JcshType.Date => "date",
            JcshType.DateTime => "datetime2(3)",
            JcshType.Decimal => $"decimal({Precision},{Scale})",
            JcshType.Bit => "bit",
            _ => "nvarchar(100)"
        };
    }

    /// <summary>
    /// Column layout of "Jobcard servicehistory.xlsx" (= DMS_IOT_DATA.dbo.DMS_ServiceHistory export).
    /// Types / lengths were chosen from a full profile of the 4,81,392-row file (03.11.2026):
    /// max text length found → next round size with head-room; numbers that are always whole → int;
    /// amounts → decimal(18,2). Must stay in sync with docs/jobcard-servicehistory/01_Create_JobCardServiceHistory.sql.
    /// Excel "Id" (source table identity) is kept as SourceId - the new table has its own Id.
    /// </summary>
    public static class JcshColumns
    {
        private static JcshColumn T(string key, int len, string group, bool req = false) => new(key, key, JcshType.Text, len, 0, 0, req, group);
        private static JcshColumn D(string key, string group, bool req = false) => new(key, key, JcshType.Date, 0, 0, 0, req, group);
        private static JcshColumn M(string key, int p, int s, string group) => new(key, key, JcshType.Decimal, 0, p, s, false, group);
        private static JcshColumn I(string key, string group, bool req = false) => new(key, key, JcshType.Int, 0, 0, 0, req, group);

        public static readonly IReadOnlyList<JcshColumn> All = new List<JcshColumn>
        {
            // ---------- Job ----------
            new("Id", "SourceId", JcshType.Int, 0, 0, 0, false, "Job"),
            T("DealerCode", 20, "Job", true),
            I("JobNo", "Job", true),
            D("JobDate", "Job", true),
            T("CompName", 100, "Job"),
            T("Location", 100, "Job"),
            T("InTime", 10, "Job"),
            T("CloseTime", 10, "Job"),
            T("JobCategory", 50, "Job"),
            I("FFRPercentage", "Job"),
            I("DocNo", "Job"),
            T("DocType", 30, "Job"),
            D("DocDate", "Job"),
            T("Supervisor", 100, "Job"),
            T("Technician", 100, "Job"),
            T("ServiceHead", 50, "Job"),
            T("JobType", 100, "Job"),
            T("CouponNo", 100, "Job"),
            D("ExpectedDeliveryDate", "Job"),
            D("ProformaDate", "Job"),
            D("InvoiceDate", "Job"),
            T("Status", 50, "Job"),
            T("RepairType", 50, "Job"),
            D("CompletionDate", "Job"),
            T("JobStatus", 20, "Job"),
            // ---------- Vehicle ----------
            T("Model", 100, "Vehicle"),
            T("BrandName", 100, "Vehicle"),
            T("RegNo", 30, "Vehicle"),
            T("VehicleType", 20, "Vehicle"),
            T("EngineNo", 30, "Vehicle"),
            T("ChassisNo", 30, "Vehicle"),
            M("KMS", 14, 2, "Vehicle"),
            D("SaleDate", "Vehicle"),
            // ---------- Battery ----------
            T("BatterySerialNo1", 30, "Battery"), T("BatterySerialNo2", 30, "Battery"), T("BatterySerialNo3", 30, "Battery"),
            T("BatterySerialNo4", 30, "Battery"), T("BatterySerialNo5", 30, "Battery"), T("BatterySerialNo6", 30, "Battery"),
            T("IndividualAHBattery1", 30, "Battery"), T("IndividualAHBattery2", 30, "Battery"), T("IndividualAHBattery3", 30, "Battery"),
            T("IndividualAHBattery4", 30, "Battery"), T("IndividualAHBattery5", 30, "Battery"), T("IndividualAHBattery6", 30, "Battery"),
            // ---------- Customer ----------
            T("PartyName", 200, "Customer"),
            T("MobileNumber", 20, "Customer"),
            // ---------- Amounts ----------
            M("EstimatedJobExpenses", 18, 2, "Amounts"),
            M("LabourHours", 9, 2, "Amounts"),
            M("Parts", 18, 2, "Amounts"),
            M("Accessory", 18, 2, "Amounts"),
            M("Oil", 18, 2, "Amounts"),
            M("Labour", 18, 2, "Amounts"),
            M("OutsideWork", 18, 2, "Amounts"),
            M("TotalWOTax", 18, 2, "Amounts"),
            M("GSTAmount", 18, 2, "Amounts"),
            M("IGSTAmount", 18, 2, "Amounts"),
            M("NetTotal", 18, 2, "Amounts"),
            new("IsRowTotal", "IsRowTotal", JcshType.Bit, 0, 0, 0, false, "Amounts"),
            // ---------- Source record ----------
            new("CreatedAt", "CreatedAt", JcshType.DateTime, 0, 0, 0, false, "Source Record"),
            new("UpdatedAt", "UpdatedAt", JcshType.DateTime, 0, 0, 0, false, "Source Record"),
            T("RowHash", 100, "Source Record"),
            T("UniqueKey", 100, "Source Record", true),
        };

        public static readonly IReadOnlyDictionary<string, JcshColumn> ByKey = All.ToDictionary(c => c.Key, System.StringComparer.OrdinalIgnoreCase);
    }
}
