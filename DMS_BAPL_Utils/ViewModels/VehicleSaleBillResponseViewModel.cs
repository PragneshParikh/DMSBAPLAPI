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
        public decimal TotalAmount { get; set; }

        public List<VehicleSaleBillDetailVM> Details { get; set; } = new();



    }
}
