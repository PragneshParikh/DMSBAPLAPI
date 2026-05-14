using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class CurrentStockFilterModel
    {
        public string? DealerCode { get; set; }

        public string? ModelCode { get; set; }

        public string? ColorCode { get; set; }

        public string? ChassisNo { get; set; }

        public string? StockStatus { get; set; }

        public bool? IsBilled { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
