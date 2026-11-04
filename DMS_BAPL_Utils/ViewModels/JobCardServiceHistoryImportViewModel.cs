using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // ─────────────────────────────────────────────────────────────────────────
    // Jobcard Servicehistory Excel (DMS_ServiceHistory export, 63 columns)
    //   → BAPLDMSvad.dbo.JobCardServiceHistory   (NOT JobCardHeader / dbo.JobCards)
    // One Excel row = one job. UniqueKey identifies the job; existing UniqueKeys are skipped,
    // never overwritten. Validate / import run as background jobs (see controller).
    // ─────────────────────────────────────────────────────────────────────────

    public class JcshFieldViewModel
    {
        /// <summary>Excel header (and DB column, except Excel "Id" → DB "SourceId")</summary>
        public string Key { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Group { get; set; } = null!;
        public bool Required { get; set; }
        /// <summary>"DealerCode nvarchar(20)" - shown in the UI</summary>
        public string Target { get; set; } = null!;
    }

    public class JcshSheetInfo
    {
        public string Name { get; set; } = null!;
        public int Rows { get; set; }
    }

    public class JcshUploadResponse
    {
        public Guid UploadId { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string SheetName { get; set; } = null!;
        public List<JcshSheetInfo> Sheets { get; set; } = new();
        public int TotalRows { get; set; }
        public List<string> Headers { get; set; } = new();
        public Dictionary<string, string> SuggestedMapping { get; set; } = new();
        public List<Dictionary<string, string?>> SampleRows { get; set; } = new();
        public List<JcshFieldViewModel> Fields { get; set; } = new();
        /// <summary>expected Excel columns that are not in the file</summary>
        public List<string> MissingColumns { get; set; } = new();
        /// <summary>Excel columns that are not part of the 63-column layout (not imported)</summary>
        public List<string> ExtraColumns { get; set; } = new();
        public bool DestinationConfigured { get; set; }
        public bool DestinationTableExists { get; set; }
        public string? DestinationMessage { get; set; }
    }

    public class JcshRunOptions
    {
        /// <summary>true = validate only, nothing is written</summary>
        public bool DryRun { get; set; } = true;
        /// <summary>field key → Excel header</summary>
        public Dictionary<string, string> Mapping { get; set; } = new();
        /// <summary>rows per bulk copy + transaction (500 - 20,000)</summary>
        public int BatchSize { get; set; } = 5000;
        /// <summary>optional Excel row range (header = row 1)</summary>
        public int? FromRow { get; set; }
        public int? ToRow { get; set; }
    }

    public class JcshIssue
    {
        public int RowNo { get; set; }
        /// <summary>Error (row not imported) | Skipped (duplicate) | Warning (imported) | Info (imported, data note)</summary>
        public string Level { get; set; } = "Error";
        public string? UniqueKey { get; set; }
        public string? JobNo { get; set; }
        public string? ChassisNo { get; set; }
        public string Message { get; set; } = null!;
    }

    public class JcshDealerSummary
    {
        public string DealerCode { get; set; } = null!;
        public int Rows { get; set; }
        public decimal NetTotal { get; set; }
    }

    public class JcshJobStatus
    {
        public Guid JobId { get; set; }
        public Guid UploadId { get; set; }
        public bool DryRun { get; set; }
        /// <summary>Queued | Running | Completed | Failed | Cancelled</summary>
        public string State { get; set; } = "Queued";
        public string Phase { get; set; } = "Queued";
        public string FileName { get; set; } = null!;
        public string SheetName { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public double ElapsedSeconds { get; set; }
        public int TotalRows { get; set; }
        public int RowsRead { get; set; }
        /// <summary>dry run: valid new rows that a real import would insert</summary>
        public int RowsReady { get; set; }
        /// <summary>real import: rows inserted</summary>
        public int RowsInserted { get; set; }
        public int SkippedDuplicateInFile { get; set; }
        public int SkippedExisting { get; set; }
        public int RowsWithErrors { get; set; }
        public int Warnings { get; set; }
        public int BatchesDone { get; set; }
        public int BatchesFailed { get; set; }
        public bool DuplicateCheckedAgainstDb { get; set; }
        public decimal NetTotal { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<JcshDealerSummary> Dealers { get; set; } = new();
        /// <summary>first 1,000 issues per level; IssueCounts are complete</summary>
        public List<JcshIssue> Issues { get; set; } = new();
        public bool IssuesTruncated { get; set; }
        public Dictionary<string, int> IssueCounts { get; set; } = new();
        public string? Message { get; set; }
        public string Engine { get; set; } = "Jobcard servicehistory import v1.0";
    }

    public class JcshHistoryQuery
    {
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? DealerCode { get; set; }
        public int? JobNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class JcshHistoryPage
    {
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<Dictionary<string, object?>> Rows { get; set; } = new();
    }
}
