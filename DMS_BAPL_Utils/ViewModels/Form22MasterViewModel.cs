using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class Form22MasterViewModel
    {
        public int Id { get; set; }

        public int OemmodelId { get; set; }
        public string? OemModelName { get; set; }

        public string? SoundLevelHorn { get; set; }

        public string? PassbyNoiseLevel { get; set; }

        public string? ApprovalCertificateNo { get; set; }

        public bool IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

    }
    public class OemModelViewModel
    {
        public int OemmodelId { get; set; }
        public string OemModelName { get; set; }
    }
}
