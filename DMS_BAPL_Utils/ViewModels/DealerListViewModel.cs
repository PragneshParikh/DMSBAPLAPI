namespace DMS_BAPL_Utils.ViewModels
{
    // Dealer Creation Manager only ever needs these fields. Full dealer
    // detail (address, GST, PAN, credit terms, etc.) lives on the existing
    // Dealer Master screen — not duplicated here.
    public class DealerListViewModel
    {
        public int Id { get; set; }
        public string? Dealercode { get; set; }
        public string? Compname { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string? LinkedUserId { get; set; }
        public string? LinkedUserName { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    public class DealerListFilterModel
    {
        public string? Search { get; set; }
        public string? DealerCode { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class DealerQuickUpdateViewModel
    {
        public string Dealercode { get; set; } = string.Empty;
        public string Compname { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class DealerRoleAssignmentViewModel
    {
        public string RoleId { get; set; } = string.Empty;
    }
}