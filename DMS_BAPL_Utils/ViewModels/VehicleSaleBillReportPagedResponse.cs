using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillReportPagedResponse
    {
        public bool Success { get; set; } = true;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        // Aggregates over the full filtered set (not just current page)
        public int TotalVehicles { get; set; }
        public decimal TotalSaleAmount { get; set; }
        public decimal TotalGstAmount { get; set; }
        public decimal TotalSubsidyAmount { get; set; }
        public decimal TotalNetAmount { get; set; }

        public List<VehicleSaleBillReportViewModel> Data { get; set; } = new();
    }
}
