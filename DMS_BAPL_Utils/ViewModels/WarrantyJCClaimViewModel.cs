using DMS_BAPL_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DMS_BAPL_Utils.ViewModels
{
    public class WarrantyJCClaimViewModel
    {
        public string? DealerCode { get; set; }
        public string? LocationCode { get; set; }   // <-- add
        public string? LocationName { get; set; }   // <-- add
        public string? ClaimPrefix { get; set; }
        public int? ClaimNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string? ChassisNo { get; set; }
        public int? SupplierId { get; set; }
        public int? JobCardHeaderId { get; set; }
        public int? CustomerLedgerId { get; set; }
        public int? RepairBillHeaderId { get; set; }
        public int? FFIRId { get; set; }
        public string? ClaimAccount { get; set; }
        public string? DealerObservation { get; set; }
        public string? RootCauseAnalysis { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<WarrantyJcclaimDetailViewModel> repairBillDetails { get; set; }
    }
    public class WarrantyJCClaimListViewModel
    {
        public int Id { get; set; }
        public string? ClaimPrefix { get; set; }
        public int? ClaimNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string? ChassisNo { get; set; }
        public string? SupplierName { get; set; }
        public string? JobCardNo { get; set; }
        public decimal TotalAmount { get; set; }
    }
    // All filters optional.
    public class WarrantyJCClaimSearchViewModel
    {
        public string? DealerCode { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ChassisNo { get; set; }
        public int? ClaimNo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
    public class WarrantyJCClaimSearchResultViewModel
    {
        public System.Collections.Generic.List<WarrantyJCClaimListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
    public class WarrantyJcclaimDetailViewModel
    {
        public int RepairBillDetailsId { get; set; }
        public string? ItemType { get; set; }
        public int? MaterialId { get; set; }
        public int? LabourId { get; set; }
        public int? PartWiseLabourId { get; set; }
        public int? PartItemId { get; set; }
        public decimal? PartItemQty { get; set; }
        public decimal? PartItemRate { get; set; }
        public decimal? LabourQty { get; set; }
        public decimal? LabourRate { get; set; }
        public decimal IgstAmount { get; set; }

        public decimal Mrp { get; set; }
        public decimal? TotalWithTax { get; set; }
        public string? DealerObservation { get; set; }
        public string? RootCauseAnalysis { get; set; }
    }
}