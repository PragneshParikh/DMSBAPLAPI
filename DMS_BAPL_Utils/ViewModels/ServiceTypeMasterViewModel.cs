using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ServiceTypeMasterViewModel
    {
        public int Id { get; set; }
        public int? ServiceHeadId { get; set; }
        public string? ServiceTypeName { get; set; }
        public string? ServiceHeadName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
