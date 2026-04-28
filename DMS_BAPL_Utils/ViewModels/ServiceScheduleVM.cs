using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ServiceScheduleVM
    {

        public int Id { get; set; }
        public int OemmodelId { get; set; }
        public int? Noofservice { get; set; }

        public int? Seqno { get; set; }

        public string? SrNo { get; set; }

        public int DaysFrom { get; set; }

        public int DaysTo { get; set; }

        public int JourneyFrom { get; set; }

        public int JourneyTo { get; set; }

        public int? ServiceFrom { get; set; }

        public int ServiceHead { get; set; }

        public int ServiceType { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
    public class ServiceSchedulelistVM
    {

        public int Id { get; set; }

        public int OemModelId { get; set; }
        public string ModelName { get; set; }

        public int? Noofservice { get; set; }

        public int? Seqno { get; set; }

        public string? SrNo { get; set; }

        public int DaysFrom { get; set; }

        public int DaysTo { get; set; }

        public int JourneyFrom { get; set; }

        public int JourneyTo { get; set; }

        public int? ServiceFrom { get; set; }

        public int ServiceHeadId { get; set; }

        public int ServiceTypeId { get; set; }

        public string? ServiceHead { get; set; }

        public string? ServiceType { get; set; }

        public DateTime? EffectiveDate { get; set; }
    }

    public class ModellistVMbasedOnOemMode
    {
        public string ModelName { get; set; }
    }
}
