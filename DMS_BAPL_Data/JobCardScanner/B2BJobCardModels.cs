using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.JobCardScanner
{
    // B2B Job Card (24.11.2026) - request / response DTOs of api/b2b-jobcard. Entities are never returned directly.
    // Angular mirror: src/app/components/b2b-jobcard/models/b2b-jobcard.model.ts

    /// <summary>JobCardScanner dbo.Dealers row the caller works for (or the dealer an admin picked).</summary>
    public class B2BDealerDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? DmsDealerCode { get; set; }
        public string? City { get; set; }
    }

    /// <summary>One dbo.ServiceMenuMasters row: Job Type -> Service Head -> Priority cascade.</summary>
    public class B2BServiceMenuRowDto
    {
        public Guid Id { get; set; }
        public int JobTypeId { get; set; }
        public string JobTypeName { get; set; } = "";
        public int ServiceHeadId { get; set; }
        public string ServiceHeadName { get; set; } = "";
        /// <summary>short code shown in the dropdown (e.g. "1")</summary>
        public string PriorityValue { get; set; } = "";
        /// <summary>posted value - JobCardScanner JobCardPriority name (Normal / High / Urgent)</summary>
        public string PriorityLabel { get; set; } = "";
        public int SortOrder { get; set; }
    }

    public class B2BIdNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class B2BPriorityDto
    {
        public string Value { get; set; } = "";   // PriorityLabel (posted)
        public string Label { get; set; } = "";   // PriorityValue (shown)
    }

    public class B2BComplaintDto
    {
        public Guid Id { get; set; }
        public string ComplaintText { get; set; } = "";
        public int SortOrder { get; set; }
    }

    public class B2BWorkshopDto
    {
        public string LocCode { get; set; } = "";
        public string LocName { get; set; } = "";
    }

    public class B2BMastersDto
    {
        public List<B2BServiceMenuRowDto> ServiceMenu { get; set; } = new();
        public List<B2BComplaintDto> Complaints { get; set; } = new();
        /// <summary>JobCardScanner dbo.WorkshopLocations of the dealer (empty when the dealer has none synced yet)</summary>
        public List<B2BWorkshopDto> Workshops { get; set; } = new();
        public B2BDealerDto? Dealer { get; set; }
        public string? DealerMessage { get; set; }
    }

    public class B2BVehicleSuggestionDto
    {
        /// <summary>JobCardScanner vehicle id; null for DMS sale / service history hits</summary>
        public Guid? VehicleId { get; set; }
        /// <summary>JobCardScanner / DMS sale / Service history</summary>
        public string Source { get; set; } = "";
        public DateTime? SaleDate { get; set; }
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? Model { get; set; }
        public string? CustomerName { get; set; }
        public string? DealerCode { get; set; }
        public int Jobs { get; set; }
    }

    /// <summary>vehicle-suggest result: the caller's dealer first; when nothing matches there, every dealer (Scope = "all")</summary>
    public class B2BVehicleSuggestResultDto
    {
        public string Scope { get; set; } = "dealer";
        public List<B2BVehicleSuggestionDto> Items { get; set; } = new();
    }

    public class B2BStaffDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        /// <summary>works at the selected workshop (technician location / supervisor work area)</summary>
        public bool AtLocation { get; set; }
        /// <summary>technician's workshop name (or code)</summary>
        public string? LocationName { get; set; }
    }

    /// <summary>Supervisor/Advisor (JobCardScanner Users, Role = Supervisor) and Technician (JobCardScanner Technicians) of a workshop</summary>
    public class B2BStaffListsDto
    {
        public List<B2BStaffDto> Supervisors { get; set; } = new();
        public List<B2BStaffDto> Technicians { get; set; } = new();
    }

    public class B2BPhotoDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = "";
        public string? Caption { get; set; }
        public string Stage { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class B2BPhotoInput
    {
        public string? FileName { get; set; }
        public string? BlobUrl { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public string? Caption { get; set; }
    }

    public class B2BSavePhotosRequest
    {
        public List<B2BPhotoInput> Photos { get; set; } = new();
    }

    public class B2BCustomerDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class B2BVehicleDto
    {
        public Guid? Id { get; set; }
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? Model { get; set; }
        public string? Variant { get; set; }
        public string? Color { get; set; }
        public string? BatteryNo { get; set; }
        public string? MotorNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? ConverterNo { get; set; }
        public string? ChargerNo { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public DateOnly? NextServiceDueDate { get; set; }
        public DateOnly? InsuranceExpiry { get; set; }
        public double Odometer { get; set; }
        public string? DealerCode { get; set; }
    }

    /// <summary>One past JobCardScanner job card of the vehicle (Service Card History).</summary>
    public class B2BHistoryRowDto
    {
        public Guid Id { get; set; }
        public string JobCardNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string? RegNo { get; set; }
        public double OdometerAtCheckIn { get; set; }
        public string? JobType { get; set; }
        public string? ServiceHead { get; set; }
        public string? Priority { get; set; }
        public string? ServiceLocation { get; set; }
        public string? Status { get; set; }
        public string? Stage { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? Complaints { get; set; }
    }

    public class B2BVehicleCardDto
    {
        public bool Found { get; set; }
        public B2BVehicleDto? Vehicle { get; set; }
        public B2BCustomerDto? Customer { get; set; }
        /// <summary>odometer of the vehicle's latest job card (any dealer), else Vehicles.Odometer; null = never serviced</summary>
        public double? PreviousOdometer { get; set; }
        /// <summary>number of an open (not Closed / Cancelled) job card of this chassis at the caller's dealer</summary>
        public string? OpenJobCardNumber { get; set; }
        public List<B2BHistoryRowDto> History { get; set; } = new();
    }

    public class B2BJobCardFilter
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? JobNo { get; set; }
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? CustomerName { get; set; }
        public string? ServiceLocationCode { get; set; }
        public string? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class B2BJobCardListRowDto
    {
        public Guid Id { get; set; }
        public string JobCardNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = "";
        public string? Stage { get; set; }
        public string? ManualJobNo { get; set; }
        public string? ServiceLocation { get; set; }
        public string? JobType { get; set; }
        public string? JobSource { get; set; }
        public string? ServiceHead { get; set; }
        public string Priority { get; set; } = "";
        public string? Supervisor { get; set; }
        public string? Technician { get; set; }
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? Model { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public double OdometerAtCheckIn { get; set; }
        public string? DealerCode { get; set; }
        public string? Complaints { get; set; }
    }

    public class B2BPagedDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class B2BJobCardDetailDto
    {
        public Guid Id { get; set; }
        public string JobCardNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Stage { get; set; }
        public bool CanEdit { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public int? JobTypeId { get; set; }
        public string? JobTypeName { get; set; }
        public int? ServiceHeadId { get; set; }
        public string? ServiceHeadName { get; set; }
        public string Priority { get; set; } = "";
        public string? ServiceLocationCode { get; set; }
        public string? ServiceLocationName { get; set; }
        public int? JobSourceId { get; set; }
        public string? JobSourceName { get; set; }
        public string? ManualJobNo { get; set; }
        public string? CouponNo { get; set; }
        public string? SupervisorName { get; set; }
        public string? TechnicianName { get; set; }
        public string? AssignedTechnicianName { get; set; }
        public double OdometerAtCheckIn { get; set; }
        public int? BatteryLevelAtCheckIn { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        public string? CustomerVoice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Complaints { get; set; } = new();
        public List<B2BPhotoDto> Photos { get; set; } = new();
        public B2BVehicleDto Vehicle { get; set; } = new();
        public B2BCustomerDto Customer { get; set; } = new();
    }

    /// <summary>POST api/b2b-jobcard - mirrors JobCardScanner CreateJobCardRequest + the wizard's vehicle / customer step.</summary>
    public class B2BJobCardCreateRequest
    {
        /// <summary>Admin / SuperAdmin only: DMS dealer code to create the job card for (dealers use their token's code)</summary>
        public string? DealerCode { get; set; }

        // vehicle (found by chassis; created at the dealer when JobCardScanner does not have it yet)
        public string? ChassisNo { get; set; }
        public string? RegNo { get; set; }
        public string? Model { get; set; }
        public string? Variant { get; set; }
        public string? Color { get; set; }
        public string? BatteryNo { get; set; }
        public string? MotorNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? ConverterNo { get; set; }
        public string? ChargerNo { get; set; }
        public DateOnly? PurchaseDate { get; set; }

        // customer (fleet / B2B party) - used only when a new vehicle is created
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }

        // service details
        public int? JobTypeId { get; set; }
        public int? ServiceHeadId { get; set; }
        /// <summary>ServiceMenuMasters.PriorityLabel of the chosen Job Type + Service Head</summary>
        public string? Priority { get; set; }
        public string? ServiceLocationCode { get; set; }
        /// <summary>used only when the dealer has no JobCardScanner WorkshopLocations yet (DMS LocationMaster fallback)</summary>
        public string? ServiceLocationName { get; set; }
        public int? JobSourceId { get; set; }
        public string? JobSourceName { get; set; }
        public string? ManualJobNo { get; set; }
        public string? CouponNo { get; set; }
        public string? SupervisorName { get; set; }
        public string? TechnicianName { get; set; }
        public double? OdometerAtCheckIn { get; set; }
        public int? BatteryLevelAtCheckIn { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        /// <summary>"Customer Voice" (JobCards.CustomerConsentNotes)</summary>
        public string? CustomerVoice { get; set; }
        public List<string> Complaints { get; set; } = new();
    }

    /// <summary>PUT api/b2b-jobcard/{id} - the fields JobCardScanner's own Update allows (UpdateJobCardRequest).</summary>
    public class B2BJobCardUpdateRequest
    {
        public string? Priority { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        public string? AssignedTechnicianName { get; set; }
    }

    public class B2BSaveResultDto
    {
        public Guid Id { get; set; }
        public string JobCardNumber { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
