using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // ─────────────────────────────────────────────────────────────────────────
    // Cogxim (old DMS) sale-register import  →  VehicleSaleBillHeader / Details
    // One Excel row = one vehicle (chassis). Rows sharing the same Bill No
    // (per dealer) become one VehicleSaleBillHeader with N details.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>One target field the Excel columns can be mapped to.</summary>
    public class ImportFieldViewModel
    {
        public string Key { get; set; } = null!;
        public string Label { get; set; } = null!;
        /// <summary>"Header" (bill level) or "Detail" (vehicle level)</summary>
        public string Group { get; set; } = null!;
        public bool Required { get; set; }
        /// <summary>DB column this value ends up in (shown in the UI)</summary>
        public string Target { get; set; } = null!;
        public List<string> Aliases { get; set; } = new();
    }

    public class ImportSheetInfo
    {
        public string Name { get; set; } = null!;
        public int Rows { get; set; }
    }

    public class VehicleSaleImportPreviewResponse
    {
        public string FileName { get; set; } = null!;
        public List<ImportSheetInfo> Sheets { get; set; } = new();
        public string SheetName { get; set; } = null!;
        public int HeaderRow { get; set; }
        public int TotalRows { get; set; }
        public List<string> Headers { get; set; } = new();
        /// <summary>field key → Excel header (auto-detected, user can change)</summary>
        public Dictionary<string, string> SuggestedMapping { get; set; } = new();
        public List<Dictionary<string, string?>> SampleRows { get; set; } = new();
        public List<ImportFieldViewModel> Fields { get; set; } = new();
    }

    public class VehicleSaleImportOptions
    {
        public string? SheetName { get; set; }
        /// <summary>1-based row number that holds the column titles (default 1)</summary>
        public int HeaderRow { get; set; } = 1;
        /// <summary>field key → Excel header text</summary>
        public Dictionary<string, string> Mapping { get; set; } = new();
        /// <summary>Used when the file has no Dealer Code column (or the cell is blank)</summary>
        public string? DefaultDealerCode { get; set; }
        /// <summary>Loccode used when the location cannot be matched</summary>
        public string? DefaultLocation { get; set; }
        /// <summary>SaleBillNo is unique across ALL dealers. If two dealers used the same
        /// bill number in Cogxim, store it as "DEALERCODE/BILLNO".</summary>
        public bool PrefixDealerCodeOnBillNo { get; set; }
        /// <summary>Create a "Party" ledger when the customer is not found
        /// (the sale-bill edit screen needs a LedgerId).</summary>
        public bool CreateMissingCustomers { get; set; } = true;
        /// <summary>A bill that is already in DMS with Bill From "Cogxim Import" is updated from
        /// the file (header, vehicles matched by chassis, blank customer fields) instead of skipped.
        /// Bills created in DMS itself are never touched.</summary>
        public bool UpdateExisting { get; set; } = true;
        /// <summary>true (default) = Sale Bill No is generated from the dealer's "sale_bill" Prefix Master
        /// format for the bill's financial year; the file's Bill No (Cogxim invoice no) is saved as InvoiceNo.
        /// false = the file's Bill No is used as Sale Bill No.</summary>
        public bool GenerateSaleBillNo { get; set; } = true;
        /// <summary>true (default) = masters the file needs but DMS does not have are created during the import:
        /// OEM Model Master, Item Master (inactive legacy model), Financier ledger (per dealer), City, Occupation.</summary>
        public bool CreateMissingMasters { get; set; } = true;
        /// <summary>true = validate only, nothing is written</summary>
        public bool DryRun { get; set; } = true;
    }

    public class ImportIssue
    {
        public int RowNo { get; set; }
        public string? SaleBillNo { get; set; }
        public string? ChassisNo { get; set; }
        /// <summary>Error (row not imported) | Skipped | Warning (imported) | Info (imported, source-data note)</summary>
        public string Level { get; set; } = "Error";
        public string Message { get; set; } = null!;
    }

    public class ImportDealerSummary
    {
        public string DealerCode { get; set; } = null!;
        public int Bills { get; set; }
        public int Vehicles { get; set; }
        public decimal Amount { get; set; }
    }

    public class VehicleSaleImportResult
    {
        public bool DryRun { get; set; }
        public string FileName { get; set; } = null!;
        public string SheetName { get; set; } = null!;
        public int TotalRows { get; set; }
        public int BillsInFile { get; set; }
        public int BillsReady { get; set; }
        public int VehiclesReady { get; set; }
        public int BillsImported { get; set; }
        public int VehiclesImported { get; set; }
        public int CustomersCreated { get; set; }
        /// <summary>Already-imported Cogxim bills found in the file (UpdateExisting)</summary>
        public int BillsToUpdate { get; set; }
        public int BillsUpdated { get; set; }
        public int VehiclesUpdated { get; set; }
        public int CustomersUpdated { get; set; }
        /// <summary>Masters created by the import (dry run: to be created)</summary>
        public int OemModelsCreated { get; set; }
        public int ItemModelsCreated { get; set; }
        public int FinanciersCreated { get; set; }
        public int CitiesCreated { get; set; }
        public int OccupationsCreated { get; set; }
        /// <summary>Names of the masters created, e.g. "OEM Model: C12 MAX 3.0 Gen 2"</summary>
        public List<string> MastersCreated { get; set; } = new();
        /// <summary>Import engine version - shown on the result so an old API build is easy to spot</summary>
        public string Engine { get; set; } = "Cogxim import v3.1 - auto masters";
        public int RowsSkipped { get; set; }
        public int RowsWithErrors { get; set; }
        public int Warnings { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<ImportDealerSummary> Dealers { get; set; } = new();
        /// <summary>First 1000 issues only; counts above are complete.</summary>
        public List<ImportIssue> Issues { get; set; } = new();
        public bool IssuesTruncated { get; set; }
    }
}
