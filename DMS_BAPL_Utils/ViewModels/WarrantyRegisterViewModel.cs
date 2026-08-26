using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class WarrantyRegisterViewModel
    {
        public long Id { get; set; }
        public string SrNo { get; set; }
        public string ClaimType { get; set; }
        public string JobNo { get; set; }
        public DateTime? JobDate { get; set; }
        public string RbillNo { get; set; }
        public DateTime? RbillDate { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string? PartName { get; set; }
        public string? PartDescription { get; set; }
        public string? LabourName { get; set; }
        public string? LabourDescription { get; set; }
        public string? ModelName { get; set; }
        public string? ModelDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Mrp { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalGstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string WarrantyClaimNo { get; set; }
        public DateTime? WarrantyClaimDate { get; set; }
        public string ChasisNo { get; set; }

        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }

        public string PartyName { get; set; }
        public string WarrantyClaimStatus { get; set; }
        public string? ApproverEngineerName { get; set; }
        public string ClaimAcceptRejectReason { get; set; }
        public string PrnNo { get; set; }
        public string WarrantyOrderStatus { get; set; }
        public string WarrantyOrderNo { get; set; }
        public DateTime? WarrantyOrderDate { get; set; }
        public string WarrantyInvoiceStatus { get; set; }
        public string WarrantyInvoiceNo { get; set; }
        public DateTime? WarrantyInvoiceDate { get; set; }
        public string PackingSlipNo { get; set; }
        public DateTime? PackingSlipDate { get; set; }
        public string DispatchNo { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string DispatchReceivedStatus { get; set; }
        public DateTime? DispatchReceivedDate { get; set; }
        public string DispatchReceivedRemarks { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string PackingConcern { get; set; }
        public string PackingConcernType { get; set; }
        public string PackingConcernRemarks { get; set; }
        public string MaterialConcern { get; set; }
        public string MaterialConcernType { get; set; }
        public string MaterialConcernRemarks { get; set; }
    }

    public class WarrantyRegisterFilterModel
    {
        public string? DealerCode { get; set; }
        public string? LocationCode { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? ChassisNo { get; set; }
        public int? ClaimNo { get; set; }

        public string? JobNo { get; set; }
        public string? WarrantyClaimStatus { get; set; }
        public string? WarrantyOrderStatus { get; set; }
        public string? WarrantyInvoiceStatus { get; set; }

        public string? Search { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class WarrantyRegisterPagedResponse
    {
        public List<WarrantyRegisterViewModel> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}