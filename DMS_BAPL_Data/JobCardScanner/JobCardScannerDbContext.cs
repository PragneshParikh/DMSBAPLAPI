using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.JobCardScanner
{
    /// <summary>
    /// B2B Job Card (24.11.2026): separate DbContext for the JobCardScanner database (connection string
    /// "JobCardScannerDb" - keep the password in user secrets / environment / deployment config). BAPLDMSvad
    /// (DefaultConnection) is not touched.
    ///
    /// Maps ONLY existing JobCardScanner tables and the columns the B2B Job Card module reads or writes. Names, types,
    /// lengths and enum-as-string conversions are copied from the JobCardScanner source
    /// (backend/JobCardScanner.Api/Models/*.cs, Data/JobCardScannerDbContext.cs, Services/WorkshopLocationStore.cs).
    /// Every NOT NULL column of the tables written (JobCards, JobCardComplaints, JobCardStageHistories, Customers,
    /// Vehicles, Counters) is mapped. JobCardScanner owns this schema: never run EF migrations against this context.
    /// </summary>
    public class JobCardScannerDbContext : DbContext
    {
        public JobCardScannerDbContext(DbContextOptions<JobCardScannerDbContext> options) : base(options) { }

        public DbSet<JcsDealer> Dealers => Set<JcsDealer>();
        public DbSet<JcsCustomer> Customers => Set<JcsCustomer>();
        public DbSet<JcsVehicle> Vehicles => Set<JcsVehicle>();
        public DbSet<JcsWorkflowStage> WorkflowStages => Set<JcsWorkflowStage>();
        public DbSet<JcsJobCard> JobCards => Set<JcsJobCard>();
        public DbSet<JcsJobCardComplaint> JobCardComplaints => Set<JcsJobCardComplaint>();
        public DbSet<JcsJobCardStageHistory> JobCardStageHistories => Set<JcsJobCardStageHistory>();
        public DbSet<JcsCounter> Counters => Set<JcsCounter>();
        public DbSet<JcsServiceMenuMaster> ServiceMenuMasters => Set<JcsServiceMenuMaster>();
        public DbSet<JcsComplaintMaster> ComplaintMasters => Set<JcsComplaintMaster>();
        public DbSet<JcsWorkshopLocation> WorkshopLocations => Set<JcsWorkshopLocation>();
        public DbSet<JcsJobCardPhoto> JobCardPhotos => Set<JcsJobCardPhoto>();
        public DbSet<JcsUser> Users => Set<JcsUser>();
        public DbSet<JcsTechnician> Technicians => Set<JcsTechnician>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<JcsDealer>(e =>
            {
                e.ToTable("Dealers", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Code).HasMaxLength(30).IsRequired();
                e.Property(x => x.BaplDmsDealerCode).HasMaxLength(30);
                e.Property(x => x.City).HasMaxLength(100);
                e.Property(x => x.State).HasMaxLength(100);
            });

            b.Entity<JcsCustomer>(e =>
            {
                e.ToTable("Customers", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Mobile).HasMaxLength(30).IsRequired();
                e.Property(x => x.Email).HasMaxLength(200);
                e.Property(x => x.Address).HasMaxLength(300);
                e.Property(x => x.City).HasMaxLength(100);
                e.Property(x => x.State).HasMaxLength(100);
                e.Property(x => x.OutstandingAmount).HasColumnType("decimal(12,2)");
                e.HasOne(x => x.Dealer).WithMany().HasForeignKey(x => x.DealerId);
            });

            b.Entity<JcsVehicle>(e =>
            {
                e.ToTable("Vehicles", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Model).HasMaxLength(100).IsRequired();
                e.Property(x => x.Variant).HasMaxLength(60);
                e.Property(x => x.Color).HasMaxLength(40);
                e.Property(x => x.RegNo).HasMaxLength(30);
                e.Property(x => x.Vin).HasMaxLength(50);
                e.Property(x => x.BatteryNo).HasMaxLength(50);
                e.Property(x => x.MotorNo).HasMaxLength(50);
                e.Property(x => x.ControllerNo).HasMaxLength(50);
                e.Property(x => x.ConverterNo).HasMaxLength(50);
                e.Property(x => x.ChargerNo).HasMaxLength(50);
                e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
                e.HasOne(x => x.Dealer).WithMany().HasForeignKey(x => x.DealerId);
            });

            b.Entity<JcsWorkflowStage>(e =>
            {
                e.ToTable("WorkflowStages", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.StageKey).HasMaxLength(60).IsRequired();
                e.Property(x => x.Label).HasMaxLength(120).IsRequired();
            });

            b.Entity<JcsJobCard>(e =>
            {
                e.ToTable("JobCards", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.JobCardNumber).HasMaxLength(40).IsRequired();
                // enum names stored as strings, exactly like JobCardScanner's own DbContext
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                e.Property(x => x.ServiceType).HasConversion<string>().HasMaxLength(30);
                e.Property(x => x.Source).HasConversion<string>().HasMaxLength(30);
                e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
                e.Property(x => x.TrackingToken).HasMaxLength(80);
                e.Property(x => x.CustomerConsentNotes).HasMaxLength(2000);
                e.Property(x => x.AssignedTechnicianName).HasMaxLength(120);
                e.Property(x => x.BaplJobType).HasMaxLength(60);
                e.Property(x => x.BaplServiceLocation).HasMaxLength(200);
                e.Property(x => x.BaplSupervisorName).HasMaxLength(120);
                e.Property(x => x.BaplTechnicianName).HasMaxLength(120);
                e.Property(x => x.BaplManualJobNo).HasMaxLength(40);
                e.Property(x => x.BaplJobSourceName).HasMaxLength(60);
                e.Property(x => x.BaplServiceHeadName).HasMaxLength(120);
                e.Property(x => x.BaplServiceTypeName).HasMaxLength(120);
                e.Property(x => x.BaplServiceLocationCode).HasMaxLength(20);
                e.HasOne(x => x.Dealer).WithMany().HasForeignKey(x => x.DealerId);
                e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId);
                e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
                e.HasOne(x => x.CurrentStage).WithMany().HasForeignKey(x => x.CurrentStageId);
                e.HasMany(x => x.Complaints).WithOne().HasForeignKey(c => c.JobCardId);
            });

            b.Entity<JcsJobCardComplaint>(e =>
            {
                e.ToTable("JobCardComplaints", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Description).HasMaxLength(500).IsRequired();
                e.Property(x => x.Category).HasMaxLength(80);
            });

            b.Entity<JcsJobCardStageHistory>(e =>
            {
                e.ToTable("JobCardStageHistories", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Notes).HasMaxLength(500);
            });

            b.Entity<JcsCounter>(e =>
            {
                e.ToTable("Counters", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.CounterType).HasMaxLength(40).IsRequired();
                e.Property(x => x.Prefix).HasMaxLength(20);
            });

            b.Entity<JcsServiceMenuMaster>(e =>
            {
                e.ToTable("ServiceMenuMasters", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.JobTypeName).HasMaxLength(60).IsRequired();
                e.Property(x => x.ServiceHeadName).HasMaxLength(120).IsRequired();
                e.Property(x => x.PriorityValue).HasMaxLength(10).IsRequired();
                e.Property(x => x.PriorityLabel).HasMaxLength(30).IsRequired();
            });

            b.Entity<JcsComplaintMaster>(e =>
            {
                e.ToTable("ComplaintMasters", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.ComplaintText).HasMaxLength(300).IsRequired();
            });

            // dbo.Users - read only: Supervisor/Advisor list (Role stored as the StaffRole name)
            b.Entity<JcsUser>(e =>
            {
                e.ToTable("Users", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(200);
                e.Property(x => x.Role).HasMaxLength(30);
                e.Property(x => x.WorkLocationCodes).HasMaxLength(2000);
            });

            // dbo.Technicians (JobCardScanner Models/Technicians.cs) - read only: Technician list
            b.Entity<JcsTechnician>(e =>
            {
                e.ToTable("Technicians", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(150);
                e.Property(x => x.LocationCode).HasMaxLength(20);
                e.Property(x => x.LocationName).HasMaxLength(200);
            });

            // dbo.JobCardPhotos (JobCardScanner Models/JobCard.cs JobCardPhoto; Stage stored as the PhotoStage name)
            b.Entity<JcsJobCardPhoto>(e =>
            {
                e.ToTable("JobCardPhotos", "dbo");
                e.HasKey(x => x.Id);
                e.Property(x => x.Stage).HasMaxLength(20).IsRequired();
                e.Property(x => x.Url).HasMaxLength(500).IsRequired();
                e.Property(x => x.Caption).HasMaxLength(200);
            });

            // dbo.WorkshopLocations (JobCardScanner Services/WorkshopLocationStore.cs), read only here. Its key
            // column is not declared in the JobCardScanner source, so it is mapped keyless.
            b.Entity<JcsWorkshopLocation>(e =>
            {
                e.ToTable("WorkshopLocations", "dbo");
                e.HasNoKey();
            });
        }
    }

    // ---------------- enums (names stored as strings - JobCardScanner Models/JobCard.cs) ----------------
    public enum JcsJobCardStatus { Open, InProgress, PendingCustomerApproval, PendingQc, PendingClosure, PendingInvoice, Closed, Cancelled }
    public enum JcsServiceType { FreeService, PaidService, Warranty, AccidentRepair, Breakdown, Pdi, GoodwillService }
    public enum JcsJobCardSource { WalkIn, PickupAndDrop, Breakdown, Scheduled, Online }
    public enum JcsJobCardPriority { Normal, High, Urgent }

    // ---------------- entities ----------------
    public class JcsDealer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? BaplDmsDealerCode { get; set; }
    }

    public class JcsCustomer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string Mobile { get; set; } = default!;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public Guid DealerId { get; set; }
        public JcsDealer? Dealer { get; set; }
        public decimal OutstandingAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class JcsVehicle
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public JcsCustomer? Customer { get; set; }
        public Guid DealerId { get; set; }
        public JcsDealer? Dealer { get; set; }
        public string Model { get; set; } = default!;
        public string? Variant { get; set; }
        public string? Color { get; set; }
        public string? RegNo { get; set; }
        /// <summary>Chassis number</summary>
        public string? Vin { get; set; }
        public string? BatteryNo { get; set; }
        public string? MotorNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? ConverterNo { get; set; }
        public string? ChargerNo { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public DateOnly? LastServiceDate { get; set; }
        public DateOnly? InsuranceExpiry { get; set; }
        public DateOnly? NextServiceDueDate { get; set; }
        public double Odometer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class JcsWorkflowStage
    {
        public Guid Id { get; set; }
        public Guid? DealerId { get; set; }
        public string StageKey { get; set; } = default!;
        public string Label { get; set; } = default!;
        public int Seq { get; set; }
        public bool Active { get; set; }
        public bool IsTerminal { get; set; }
    }

    public class JcsJobCard
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string JobCardNumber { get; set; } = default!;
        public Guid DealerId { get; set; }
        public JcsDealer? Dealer { get; set; }
        public Guid VehicleId { get; set; }
        public JcsVehicle? Vehicle { get; set; }
        public Guid CustomerId { get; set; }
        public JcsCustomer? Customer { get; set; }
        public JcsJobCardStatus Status { get; set; } = JcsJobCardStatus.Open;
        public JcsServiceType ServiceType { get; set; }
        public JcsJobCardSource Source { get; set; }
        public JcsJobCardPriority Priority { get; set; } = JcsJobCardPriority.Normal;
        public Guid? CurrentStageId { get; set; }
        public JcsWorkflowStage? CurrentStage { get; set; }
        public Guid? ServiceAdvisorId { get; set; }
        public Guid? AssignedTechnicianId { get; set; }
        public string? AssignedTechnicianName { get; set; }
        public double OdometerAtCheckIn { get; set; }
        public int? BatteryLevelAtCheckIn { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        public DateTime? ActualDeliveryAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string TrackingToken { get; set; } = Guid.NewGuid().ToString("N");
        public string? CustomerConsentNotes { get; set; }
        public string? BaplJobType { get; set; }
        public string? BaplServiceLocation { get; set; }
        public string? BaplSupervisorName { get; set; }
        public string? BaplTechnicianName { get; set; }
        public string? BaplManualJobNo { get; set; }
        public int? BaplJobTypeId { get; set; }
        public int? BaplJobSourceId { get; set; }
        public string? BaplCouponNo { get; set; }
        public string? BaplJobSourceName { get; set; }
        public int? BaplServiceHeadId { get; set; }
        public string? BaplServiceHeadName { get; set; }
        public int? BaplServiceTypeId { get; set; }
        public string? BaplServiceTypeName { get; set; }
        public string? BaplServiceLocationCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedById { get; set; }
        public List<JcsJobCardComplaint> Complaints { get; set; } = new();
    }

    public class JcsJobCardComplaint
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobCardId { get; set; }
        public string Description { get; set; } = default!;
        public string? Category { get; set; }
        public bool IsCustomerVoice { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class JcsJobCardStageHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobCardId { get; set; }
        public Guid StageId { get; set; }
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExitedAt { get; set; }
        public Guid? ChangedById { get; set; }
        public string? Notes { get; set; }
    }

    public class JcsCounter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DealerId { get; set; }
        public string CounterType { get; set; } = default!;
        public string? Prefix { get; set; }
        public int CurrentValue { get; set; }
    }

    public class JcsServiceMenuMaster
    {
        public Guid Id { get; set; }
        public int JobTypeId { get; set; }
        public string JobTypeName { get; set; } = default!;
        public int ServiceHeadId { get; set; }
        public string ServiceHeadName { get; set; } = default!;
        public string PriorityValue { get; set; } = default!;
        public string PriorityLabel { get; set; } = default!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class JcsComplaintMaster
    {
        public Guid Id { get; set; }
        public string ComplaintText { get; set; } = default!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class JcsJobCardPhoto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobCardId { get; set; }
        /// <summary>PhotoStage name: CheckIn / Inspection / Repair / Qc / Delivery / PartSuggestion</summary>
        public string Stage { get; set; } = "CheckIn";
        public string Url { get; set; } = default!;
        public string? Caption { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid? PartSuggestionId { get; set; }
        public Guid? UploadedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>dbo.Users (only the columns the Supervisor list needs)</summary>
    public class JcsUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid? DealerId { get; set; }
        /// <summary>StaffRole name, e.g. "Supervisor"</summary>
        public string Role { get; set; } = default!;
        public bool Active { get; set; }
        /// <summary>JSON array of workshop location codes; empty = all locations</summary>
        public string? WorkLocationCodes { get; set; }
    }

    /// <summary>dbo.Technicians</summary>
    public class JcsTechnician
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public string Name { get; set; } = default!;
        public string LocationCode { get; set; } = default!;
        public string? LocationName { get; set; }
        public bool Active { get; set; }
    }

    public class JcsWorkshopLocation
    {
        public Guid DealerId { get; set; }
        public string LocCode { get; set; } = default!;
        public string LocName { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
