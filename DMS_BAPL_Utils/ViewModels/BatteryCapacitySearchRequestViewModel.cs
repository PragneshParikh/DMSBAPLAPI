using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class BatteryCapacityPaginatedMasterViewModel
    {
        public string? BatteryCapacity { get; set; }
        public Boolean IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResponseBattery<T>
    {
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; }
    }
}
