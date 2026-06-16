using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillReportViewModel
    {
        public int SrNo { get; set; }

        // Header
        public int SaleBillId { get; set; }
        public string? SaleBillNo { get; set; }
        public DateTime SaleDate { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? CustomerName { get; set; }
        public string? BillingName { get; set; }
        public string? CustomerType { get; set; }
        public string? SaleType { get; set; }
        public int? BillType { get; set; }
        public string? Financier { get; set; }
        public string? SalesExecutive { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public string? InvoiceNo { get; set; }

        // Vehicle / detail
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? ItemCode { get; set; }
        public string? ModelName { get; set; }
        public string? OemModelName { get; set; }
        public string? Colour { get; set; }
        public string? Hsn { get; set; }
        public int? MfgYear { get; set; }
        public string? RegNo { get; set; }
        public string? InsNo { get; set; }

        // Money
        public decimal ItemRate { get; set; }
        public decimal PreGstDiscount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal SgstPer { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal CgstPer { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal IgstPer { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal FameIIDiscount { get; set; }
        public decimal RegAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal PostGstDiscount { get; set; }
        public decimal FinalAmount { get; set; }

        // Components
        public string? Battery { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? Vcu { get; set; }

    }
}
