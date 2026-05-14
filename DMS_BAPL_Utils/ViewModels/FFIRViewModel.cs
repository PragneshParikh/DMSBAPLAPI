using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class FFIRViewModel
    {
        public int Id { get; set; }

        public string FFIRPrefix { get; set; }

        public string? DealerCode { get; set; }

        public int CIRNo { get; set; }

        public DateTime? CIRDate { get; set; }

        public int JobCardCustomerId { get; set; }

        public int JobCardHeaderId { get; set; }

        public string PurposeOfCIR { get; set; }

        public string FFIRChassisNo { get; set; }

        public DateTime? FailureDate { get; set; }

        public string ReportTitle { get; set; }

        public string ReportPreparedBy { get; set; }

        public int? NoOfPassenger { get; set; }

        public string TypeOfRoadSurface { get; set; }

        public bool RepeatFailure { get; set; }

        public bool ChassisModified { get; set; }

        public string FFIRRemarks { get; set; }

        public string CreatedBy { get; set; }

        public List<MainPartAffectedFFIRViewModel> MainParts { get; set; }

        public FFIRDetailObservationViewModel DetailObservation { get; set; }
    }

    public class MainPartAffectedFFIRViewModel
    {
        public int Id { get; set; }

        public string PartAffectedName { get; set; }

        public string PartAffectedDescription { get; set; }
    }
    public class FFIRDetailObservationViewModel
    {
        public string ObservationFailedParts { get; set; }

        public string RootCauseofFailure { get; set; }

        public string CorrectiveAction { get; set; }

        public string ResolutionComplaint { get; set; }

        public string PresentStatusofVehicle { get; set; }

        public string VehicleOffRoadReason { get; set; }
    }

    public class PartDropdownviewmodel
    {
        public int itemId { get; set; }
        public string partAffectedName { get; set; }
    }
    public class FFirCompalintCodeListViewModel
    {
        public int Id { get; set; }
        public string Complaint { get; set; }
    }

    public class FFIRFailurepartDetailsViewModel
    {
        public int Id { get; set; }
        public string FailurePartNameDescription { get; set; }
        public string IssueType { get; set; }
        public int partQty { get; set; }
        public decimal partPrice { get; set; } 
    }
    public class JobCardHistoryViewModel
    {
        public int JobCardId { get; set; }
        public int? JobCardNo { get; set; }
        public DateOnly? JobCardDate { get; set; }
        public int? VehicleJourney { get; set; }
        public string? Observation { get; set; }
        public string ServiceHead { get; set; }
        public string ServiceType { get; set; }
        public string Remarks { get; set; }


    }

    public class FFIRViewModelList
    {
        public int Id { get; set; }

        public string FFIRPrefix { get; set; }

        public string CustomerName { get; set; }

        public string ModelName { get; set; }

        public string? DealerCode { get; set; }

        public int CIRNo { get; set; }

        public DateTime? CIRDate { get; set; }

        public int? JobNo { get; set; }
        public DateOnly? jobDate { get; set; }

        public int? CurrentKms { get; set; }

        public int JobCardCustomerId { get; set; }

        public int JobCardHeaderId { get; set; }

        public string PurposeOfCIR { get; set; }

        public string FFIRChassisNo { get; set; }

        public DateTime? FailureDate { get; set; }

        public string ReportTitle { get; set; }

        public string ReportPreparedBy { get; set; }

        public int? NoOfPassenger { get; set; }

        public string TypeOfRoadSurface { get; set; }

        public bool RepeatFailure { get; set; }

        public bool ChassisModified { get; set; }

        public string FFIRRemarks { get; set; }

    }
}
