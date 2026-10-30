using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // ═════════════════════════════════════════════════════════════════════
    // Vehicle Dispatch (OEM -> dealer) : Cogxim import + report.
    // Data lives in VehicleInward (one row per dispatched chassis).
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>One sheet of an uploaded workbook (the Cogxim file has one sheet per year).</summary>
    public class DispatchSheetInfo
    {
        public string Name { get; set; } = "";
        public int Rows { get; set; }
        /// <summary>Required columns (Invoice Date, Invoice No, Dealer Code, Chassis No) that this sheet lacks.</summary>
        public List<string> MissingColumns { get; set; } = new();
        public bool CanImport => MissingColumns.Count == 0 && Rows > 0;
    }

    public class DispatchSheetsResponse
    {
        public string FileName { get; set; } = "";
        public List<DispatchSheetInfo> Sheets { get; set; } = new();
    }

    public class VehicleDispatchImportOptions
    {
        public string? SheetName { get; set; }
        /// <summary>true = validate only, nothing is saved.</summary>
        public bool DryRun { get; set; } = true;
        /// <summary>Re-import refreshes chassis that an earlier Cogxim import created. Rows created by DMS / ERP are never changed.</summary>
        public bool UpdateExisting { get; set; } = true;
        /// <summary>Historic dispatches are saved as already received by the dealer (IsAccepted = 1),
        /// so they do not appear in the dealer's "pending inward" list.</summary>
        public bool MarkAsAccepted { get; set; } = true;
    }

    public class DispatchImportIssue
    {
        public int RowNo { get; set; }
        /// <summary>Error | Skipped | Warning | Info</summary>
        public string Level { get; set; } = "";
        public string? InvoiceNo { get; set; }
        public string? ChassisNo { get; set; }
        public string Message { get; set; } = "";
    }

    public class DispatchDealerSummary
    {
        public string DealerCode { get; set; } = "";
        public string? DealerName { get; set; }
        public int Invoices { get; set; }
        public int Vehicles { get; set; }
    }

    public class VehicleDispatchImportResult
    {
        public string Engine { get; set; } = "Vehicle Dispatch import v1.0";
        public bool DryRun { get; set; }
        public string FileName { get; set; } = "";
        public string SheetName { get; set; } = "";
        public int TotalRows { get; set; }
        public int InvoicesInFile { get; set; }

        /// <summary>dry run: new chassis that will be inserted</summary>
        public int VehiclesReady { get; set; }
        /// <summary>dry run: chassis from an earlier Cogxim import that will be refreshed</summary>
        public int VehiclesToUpdate { get; set; }
        public int VehiclesImported { get; set; }
        public int VehiclesUpdated { get; set; }

        public int RowsSkipped { get; set; }
        public int RowsWithErrors { get; set; }
        public int Warnings { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<DispatchDealerSummary> Dealers { get; set; } = new();
        public List<DispatchImportIssue> Issues { get; set; } = new();
        public bool IssuesTruncated { get; set; }
    }

    // ---------------------------------------------------------------- report
    public class VehicleDispatchReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? DealerCode { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ChassisNo { get; set; }
        /// <summary>"" | "cogxim" | "dms"</summary>
        public string? Source { get; set; }
        /// <summary>"" | "accepted" | "pending"</summary>
        public string? Status { get; set; }
        /// <summary>"" | "sold" | "unsold"</summary>
        public string? SaleStatus { get; set; }
        /// <summary>dealer / model / chassis / motor / invoice text search</summary>
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class VehicleDispatchReportRow
    {
        public int SrNo { get; set; }
        public int Id { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public string? InvoiceNo { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerCity { get; set; }
        public string? DealerState { get; set; }
        public string? LocCode { get; set; }
        public string? LocationName { get; set; }
        public string? LocationCity { get; set; }
        public string? ItemCode { get; set; }
        public string? ModelName { get; set; }
        public string? ColrCode { get; set; }
        public string? ColourName { get; set; }
        public int? MfgMonth { get; set; }
        public int? MfgYear { get; set; }
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? BatteryId { get; set; }
        public string? BatteryNo { get; set; }
        public string? BatteryNo2 { get; set; }
        public string? BatteryCapacity { get; set; }
        public string? BatteryChemistry { get; set; }
        public string? BatteryMake { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? Converter { get; set; }
        public string? Vcu { get; set; }
        public string? EcuSerno { get; set; }
        public string? EcuImEi { get; set; }
        public string? ImmoblizerNo { get; set; }
        public string? BikeSimid { get; set; }
        public string? Voltage { get; set; }
        public string? RegNumber { get; set; }
        public string? StartDate { get; set; }
        public string? TyreNo1 { get; set; }
        public string? TyreNo2 { get; set; }
        public string? PoNumber { get; set; }
        public bool? IsAccepted { get; set; }
        public bool? IsD2d { get; set; }
        /// <summary>"Cogxim Import" or "DMS"</summary>
        public string Source { get; set; } = "DMS";
        public string? SaleBillNo { get; set; }
        public DateTime? SaleDate { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class VehicleDispatchReportResponse
    {
        public List<VehicleDispatchReportRow> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalAccepted { get; set; }
        public int TotalPending { get; set; }
        public int TotalSold { get; set; }
        public int TotalUnsold { get; set; }
        public int TotalDealers { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalCogxim { get; set; }
    }
}
