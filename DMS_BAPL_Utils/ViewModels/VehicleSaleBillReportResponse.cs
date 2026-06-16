using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillReportResponse
    {
        public List<VehicleSaleBillReportViewModel> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        // Column totals over the full filtered set
        public decimal TotalItemRate { get; set; }
        public decimal TotalTaxable { get; set; }
        public decimal TotalSgst { get; set; }
        public decimal TotalCgst { get; set; }
        public decimal TotalIgst { get; set; }
        public decimal TotalFameII { get; set; }
        public decimal TotalRegistration { get; set; }
        public decimal TotalInsurance { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
