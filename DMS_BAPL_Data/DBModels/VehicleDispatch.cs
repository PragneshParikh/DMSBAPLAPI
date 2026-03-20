using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class VehicleDispatch
{
    public int Id { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public string? InvoiceNo { get; set; }

    public int? MfgYear { get; set; }

    public string? ItemCode { get; set; }

    public string? ColrCode { get; set; }

    public string? ChasisNo { get; set; }

    public string? MotorNo { get; set; }

    public string? KeyNo { get; set; }

    public string? ServBkno { get; set; }

    public string? BatteryId { get; set; }

    public string? BatteryNo { get; set; }

    public string? BatteryNo2 { get; set; }

    public string? BatteryNo3 { get; set; }

    public string? BatteryNo4 { get; set; }

    public string? BatteryNo5 { get; set; }

    public string? BatteryNo6 { get; set; }

    public string? EcuSerno { get; set; }

    public string? EcuImEi { get; set; }

    public string? EcuBalMac { get; set; }

    public string? ImmoblizerStatus { get; set; }

    public string? ImmoblizerNo { get; set; }

    public string? BikeSimid { get; set; }

    public string? BikeMobileno { get; set; }

    public string? ChargerNo { get; set; }

    public string? ControllerNo { get; set; }

    public string? SoundbarSerno { get; set; }

    public string? SoundbarBalMac { get; set; }

    public string? Voltage { get; set; }

    public string? Regnumber { get; set; }

    public string? Validity { get; set; }

    public DateOnly? Startdate { get; set; }

    public string? TyreNo1 { get; set; }

    public string? TyreNo2 { get; set; }

    public int? GstIdno { get; set; }

    public string? LocCode { get; set; }

    public string? DealerCode { get; set; }

    public string? BatteryChemistry { get; set; }

    public string? BatteryCapacity { get; set; }

    public string? BatteryMake { get; set; }

    public int? BatteryIdno { get; set; }

    public decimal? Fame2Discount { get; set; }

    public string? Converter { get; set; }

    public string? Vcu { get; set; }

    public string? Ordertype { get; set; }

    public int? MfgMonth { get; set; }

    public bool? IsAccepted { get; set; }
}
