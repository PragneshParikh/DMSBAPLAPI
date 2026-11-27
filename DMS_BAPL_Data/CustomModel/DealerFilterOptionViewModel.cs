namespace DMS_BAPL_Data.CustomModel
{
    /// <summary>26.11.2026 - Dealer Master filter dropdowns (dealer / city / state / status).</summary>
    public class DealerFilterOptionViewModel
    {
        public string DealerCode { get; set; } = string.Empty;
        public string DealerName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public bool IsActive { get; set; }
    }
}
