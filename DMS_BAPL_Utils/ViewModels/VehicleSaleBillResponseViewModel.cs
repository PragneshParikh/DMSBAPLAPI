using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillResponseViewModel
    {
        public int Id { get; set; }
        public string SaleBillNo { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public int? LedgerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Location { get; set; }
        public DateTime SaleDate { get; set; }
        public string SaleType { get; set; }
        public string BillType { get; set; }
        public string BillingName { get; set; }
        public string? Financier { get; set; }
        public string? CashAc { get; set; }
        public string? SalesExecutive { get; set; }
        public string? isTempRegNo { get; set; }
        public string? TempRegNo { get; set; }
        public bool? isD2d { get; set; }
        public string? CustomerType { get; set; }
        public string? erpStatus { get; set; }



        public List<VehicleSaleBillDetailVM> Details { get; set; } = new();



    }
}
