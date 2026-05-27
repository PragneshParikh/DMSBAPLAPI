using System;

namespace DMS_BAPL_Data.CustomModel
{
    public class POTrackingFilterModel
    {
        public string? DealerCode { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? POType { get; set; }

        public string? POStatus { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}