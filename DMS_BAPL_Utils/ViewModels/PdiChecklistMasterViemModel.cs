using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PdiChecklistMasterViemModel
    {
        public int Id { get; set; }
        public string? ChecklistName { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; } 

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
