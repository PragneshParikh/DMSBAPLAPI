namespace DMS_BAPL_Utils.ViewModels
{
    public class PartDispatchKitReportViewModel
    {
        public int SrNo { get; set; }

        public string? PONumber { get; set; }

        public DateTime? PODate { get; set; }

        public DateTime? SubmitToERPDate { get; set; }

        public string? POType { get; set; }

        public string? CompanyName { get; set; }

        public string? MobileNo { get; set; }

        public string? DealerCode { get; set; }

        public string? DealerCity { get; set; }

        public string? DealerState { get; set; }

        public string? LocationCode { get; set; }

        public string? LocationName { get; set; }

        public string? LocationCity { get; set; }
    }
}