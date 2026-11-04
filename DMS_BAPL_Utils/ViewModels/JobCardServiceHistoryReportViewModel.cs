using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // ─────────────────────────────────────────────────────────────────────────
    // Reports on BAPLDMSvad.dbo.JobCardServiceHistory (imported service history):
    //   Service History - one search box: Chassis No or Registration No
    //   Job Data        - dealer (or all) + JobDate range, dealer-wise
    // Server-side filtering + paging (≈4.8 lakh rows). Read-only.
    // ─────────────────────────────────────────────────────────────────────────

    public class JcshServiceHistoryQuery
    {
        /// <summary>Chassis No or Registration No (start of the number is enough; min 3 characters)</summary>
        public string? Search { get; set; }
        /// <summary>true when Search is a value picked from the type-ahead: exact match on ChassisNo / RegNo</summary>
        public bool Exact { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class JcshJobDataQuery
    {
        /// <summary>null / empty = all dealers</summary>
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class JcshDealerOption
    {
        public string DealerCode { get; set; } = null!;
        public string? DealerName { get; set; }
        public int Jobs { get; set; }
    }

    /// <summary>One type-ahead suggestion for the Service History search box</summary>
    public class JcshSuggestion
    {
        /// <summary>the matched value (chassis no or reg no) - put into the search box when picked</summary>
        public string Value { get; set; } = null!;
        /// <summary>"Chassis No" or "Reg No"</summary>
        public string Kind { get; set; } = null!;
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? Model { get; set; }
        public string? DealerCode { get; set; }
        public int Jobs { get; set; }
        public DateTime? LastJobDate { get; set; }
    }

    public class JcshJobDataDealerRow
    {
        public string DealerCode { get; set; } = null!;
        public string? DealerName { get; set; }
        public int Jobs { get; set; }
        public int Vehicles { get; set; }
        public decimal NetTotal { get; set; }
        public DateTime? FirstJobDate { get; set; }
        public DateTime? LastJobDate { get; set; }
    }

    public class JcshReportPage
    {
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public decimal NetTotal { get; set; }
        public List<Dictionary<string, object?>> Rows { get; set; } = new();
        /// <summary>Job Data only: one line per dealer for the whole filter (not just this page)</summary>
        public List<JcshJobDataDealerRow> Dealers { get; set; } = new();
    }
}
