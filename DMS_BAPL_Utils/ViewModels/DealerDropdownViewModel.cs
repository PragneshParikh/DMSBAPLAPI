using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class DealerDropdownViewModel
    {
        public int Id { get; set; }
        public string DealerCode { get; set; }
        public string DealerName { get; set; }
        public string CompName { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }

}