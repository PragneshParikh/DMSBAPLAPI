using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillReportFilterModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Matches: BillNo, PartyName, ChasisNo, RegNo, ItemModel, OEMModelName
        /// </summary>
        public string? Search { get; set; }
        public string? SaleType { get; set; }   // SalesType column
        public string? Status { get; set; }
        public string? ItemCode { get; set; }
        public string? Location { get; set; }
        public string? CustomerType { get; set; }
        public int? BillType { get; set; }
        public string? ChassisNo { get; set; }
        public string? SaleBillNo { get; set; }
    }
}
