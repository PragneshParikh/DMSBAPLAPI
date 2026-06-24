using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class LedgerMaster
{
    public int Id { get; set; }

    public string? LedgerCode { get; set; }

    public string? LedgerName { get; set; }

    public string? LedgerType { get; set; }

    public string? Gstno { get; set; }

    public string? Pan { get; set; }

    public string? AadharNumber { get; set; }

    public string? MobileNumber { get; set; }

    public string? Address { get; set; }

    public int? City { get; set; }

    public int? State { get; set; }

    public string? Pin { get; set; }

    public string? EMail { get; set; }

    public string? Gender { get; set; }

    public string? DealerCode { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? OccupationId { get; set; }

    public string? LedgerVisibility { get; set; }

    public string? AlternateMobileNo { get; set; }

    public string? Address2 { get; set; }

    public virtual City? CityNavigation { get; set; }

    public virtual ICollection<Hsrporder> Hsrporders { get; set; } = new List<Hsrporder>();

    public virtual ICollection<JobCardCustomer> JobCardCustomers { get; set; } = new List<JobCardCustomer>();

    public virtual ICollection<QuotationHeader> QuotationHeaderCustomerLedgers { get; set; } = new List<QuotationHeader>();

    public virtual ICollection<QuotationHeader> QuotationHeaderFinancerLedgers { get; set; } = new List<QuotationHeader>();

    public virtual ICollection<RepairBillHeader> RepairBillHeaderCustomerLedgers { get; set; } = new List<RepairBillHeader>();

    public virtual ICollection<RepairBillHeader> RepairBillHeaderInsurances { get; set; } = new List<RepairBillHeader>();

    public virtual State? StateNavigation { get; set; }

    public virtual ICollection<VehicleSaleBillDetail> VehicleSaleBillDetails { get; set; } = new List<VehicleSaleBillDetail>();
}
