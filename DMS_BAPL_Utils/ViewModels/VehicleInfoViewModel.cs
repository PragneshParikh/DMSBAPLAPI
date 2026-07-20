using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleInfoViewModel
    {
        public PartyDetailsViewModel PartyDetails { get; set; } = new();
        public DealerInfoViewMode DealerDetails { get; set; } = new();
        public VehicleDetailsViewModel VehicleDetails { get; set; } = new();
    }

    public class DealerInfoViewMode
    {
        public string DealerCode { get; set; }
        public string DealerName { get; set; }
        public string DealerLocation { get; set; }
        public string DealerEmail { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string DealerCity { get; set; }
        public string DealerState { get; set; }
        public string DealerOrder { get; set; }
    }
    public class PartyDetailsViewModel
    {
        public string PartyName { get; set; }
        public string PartyMobile { get; set; }
        public string PartyAltMobile { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Email { get; set; }
        public string? Pin { get; set; }
        public string OwnerOrder { get; set; }
    }

    public class VehicleDetailsViewModel
    {
        public string ChassisNo { get; set; }
        public string RegNo { get; set; }
        public string EngineNo { get; set; }
        public DateTime? SaleDate { get; set; }
        public string ItemCode { get; set; }
        public string ModelName { get; set; }
        public string ColorName { get; set; }
        public DateTime? InsuranceDate { get; set; }
        public DateTime? PollutionDate { get; set; }
        public string PolicyNo { get; set; }
        public DateTime? PolicyExpiryDate { get; set; }
        public string InsuranceCompany { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public string FuelType { get; set; }
        public DateTime? EngineHealthSubscriptionDate { get; set; }
        public DateTime? MonthlySubsidyDate { get; set; }
        public string OwnershipType { get; set; }
        public List<BatteryViewModel> Batteries { get; set; } = new();
        public List<ComponentViewModel> Chargers { get; set; } = new();
        public List<ComponentViewModel> Converters { get; set; } = new();
        public List<ComponentViewModel> Controllers { get; set; } = new();
        public List<ComponentViewModel> Motors { get; set; } = new();
    }
    public class BatteryViewModel
    {
        public int? SerialNo { get; set; }
        public string? BatteryNo { get; set; }
        public string? BatteryMake { get; set; }
        public string Capacity { get; set; }
        public string ChemicalType { get; set; }
    }
    public class ComponentViewModel
    {
        public int? SerialNo { get; set; }
        public string ComponentNo { get; set; }
    }

    public class UpdateVehicleInfoViewModel
    {
        public string ChassisNo { get; set; }
        public string? CustomerAltMobile { get; set; }
        public string? InsuranceNo { get; set; }
        public string? RegNo { get; set; }
        public int? InsuranceCompany { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? InsuranceStartDate { get; set; }


        public List<BatteryUpdateViewModel> Batteries { get; set; } = new();

        public List<ComponentUpdateViewModel> Motors { get; set; } = new();

        public List<ComponentUpdateViewModel> Chargers { get; set; } = new();

        public List<ComponentUpdateViewModel> Controllers { get; set; } = new();

        public List<ComponentUpdateViewModel> Converters { get; set; } = new();
    }
    public class BatteryUpdateViewModel
    {
        public int OrderNo { get; set; }

        public string? BatteryNo { get; set; }

        public string? BatteryCapacity { get; set; }

        public string? BatteryChemical { get; set; }

        public string? BatteryMake { get; set; }
    }
    public class ComponentUpdateViewModel
    {
        public int OrderNo { get; set; }

        public string? ComponentNo { get; set; }
    }
}

