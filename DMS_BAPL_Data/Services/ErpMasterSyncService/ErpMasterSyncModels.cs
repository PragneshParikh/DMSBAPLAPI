using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.Services.ErpMasterSyncService
{
    // 26.11.2026 - Dealer Master / Location Master: UAT ERP report import + ERP (BaplConnection) sync.
    // Every endpoint first runs as a preview (apply = false) and only writes when apply = true.

    /// <summary>One line of a preview / result list.</summary>
    public class ErpSyncItem
    {
        public string Code { get; set; } = "";
        public string? Name { get; set; }
        public string? DealerCode { get; set; }
        public string Action { get; set; } = "";
        /// <summary>what changes, e.g. "Status Active -> Inactive; Mobile 98.. -> 97.."</summary>
        public string? Detail { get; set; }
        public int? RowNumber { get; set; }
    }

    public class ErpSyncResult
    {
        /// <summary>false = preview only, nothing was written</summary>
        public bool Applied { get; set; }
        public string Source { get; set; } = "";
        public int TotalRows { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Activated { get; set; }
        public int Inactivated { get; set; }
        public int Unchanged { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        /// <summary>DMS rows after the run (or after a real run, for a preview)</summary>
        public int ActiveAfter { get; set; }
        public int InactiveAfter { get; set; }
        public List<ErpSyncItem> Items { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }
}
