using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class HSRPListViewModel
    {
        public int? id { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public int? SupplierLedgerId { get; set; }
        public string? SupplierName { get; set; }
        public string ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? HsrpStatus { get; set; }
        public string? HsrpResponse { get; set; }
        public string? InwardStatus { get; set; }
        public string? InwardResponse { get; set; }
        public bool? IsFrontPlate { get; set; }
        public bool? IsRearPlate { get; set; }
        public string? DealerCode { get; set; }

    }

    public class HSRPOrderAddEditViewModel
    {
        public int? id { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public int? SupplierLedgerId { get; set; }
        public string? SupplierName { get; set; }
        public string ChassisNo { get; set; }
        public string? HsrpStatus { get; set; }
        public string? HsrpResponse { get; set; }
        public string? InwardStatus { get; set; }
        public string? InwardResponse { get; set; }
        public string? RegNo { get; set; }
        public DateTime? RegDate { get; set; }
        public string? ModelName { get; set; }
        public string? Colour { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? DealerCode { get; set; }
        public string? InvoiceNo { get; set; }
        public int? CustomerLedgerId { get; set; }
        public int? SaleBillDeailsId { get; set; }
        public string? SaleBillNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool? IsFrontPlate { get; set; }
        public bool? IsRearPlate { get; set; }
    }

    public class HSRPInwardUpdate
    {

        public int? Id { get; set; }
        public string? Status { get; set; }

    }


}
