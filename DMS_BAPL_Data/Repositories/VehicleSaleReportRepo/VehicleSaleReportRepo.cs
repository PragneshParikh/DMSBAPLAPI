using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.VehicleSaleReportRepo
{
    public class VehicleSaleReportRepo : IVehicleSaleReportRepo
    {
        private readonly BapldmsvadContext _context;

        public VehicleSaleReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleSaleReportViewModel>>
            GetVehicleSaleReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            var query =
                from h in _context.VehicleSaleBillHeaders

                join d in _context.VehicleSaleBillDetails
                    on h.Id equals d.VehicleSaleBillId

                join vi in _context.VehicleInwards
                    on d.ChassisNo equals vi.ChasisNo into viJoin
                from vi in viJoin.DefaultIfEmpty()

                join im in _context.ItemMasters
                    on d.ItemCode equals im.Itemcode into imJoin
                from im in imJoin.DefaultIfEmpty()

                join dm in _context.DealerMasters
                    on h.DealerCode equals dm.Dealercode into dmJoin
                from dm in dmJoin.DefaultIfEmpty()

                join lm in _context.LedgerMasters
                    on h.LedgerId equals lm.Id into lmJoin
                from lm in lmJoin.DefaultIfEmpty()

                join city in _context.Cities
                    on lm.City equals city.CityId into cityJoin
                from city in cityJoin.DefaultIfEmpty()

                join state in _context.States
                    on lm.State equals state.StateId into stateJoin
                from state in stateJoin.DefaultIfEmpty()

                join clr in _context.ColorMasters
                    on vi.ColrCode equals clr.Colorcode into clrJoin
                from clr in clrJoin.DefaultIfEmpty()

                where
                    (!fromDate.HasValue
                        || h.SaleDate.Date >= fromDate.Value.Date)

                    &&

                    (!toDate.HasValue
                        || h.SaleDate.Date <= toDate.Value.Date)

                    &&

                    (string.IsNullOrEmpty(dealerCode)
                        || h.DealerCode == dealerCode)

                select new VehicleSaleReportViewModel
                {
                    // SR NO
                    SrNo = d.Id,

                    // MODEL DETAILS
                    ModelCode = im.Itemcode,
                    ModelDescription = im.Itemname,
                    OemModelName = im.Oemmodelname,
                    VehicleGroup = im.Itemdesc,

                    // COLOR
                    ColorCode = clr.Colorname,

                    // VEHICLE
                    ChasisNo = d.ChassisNo,
                    RegNo = d.RegNo,

                    // DEALER
                    DealerCode = dm.Dealercode,
                    DealerName = dm.Compname,
                    DealerCity = dm.City,
                    DealerState = dm.State,

                    // LOCATION
                    Location = h.Location,
                    LocCode = vi.LocCode,
                    LocCity = dm.City,

                    // CUSTOMER
                    Name = h.CustomerName,
                    Address1 = lm.Address,
                    Address2 = "",
                    CustomerState = state.StateName,
                    CustomerCity = city.CityName,
                    Pin = lm.Pin,
                    Email = lm.EMail,
                    MobileNo = lm.MobileNumber,

                    // TYPE
                    Type = h.CustomerType,

                    // BOOKING
                    BookingId = h.BookingId,

                    // DATES
                    DispatchDate =
                        vi.InvoiceDate.HasValue
                        ? vi.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,

                    SaleDate = h.SaleDate,

                    // BILL
                    InvoiceNo = h.SaleBillNo,
                    BillType = h.BillType,

                    // FINANCE
                    FinanceBy = h.Financier,
                    FinancerCode = "",
                    FinancerCategory = "",

                    // EXECUTIVE
                    ExecutiveName = h.SalesExecutive,
                    ProspectName = h.RefName,
                    ProspectMobNo = "",

                    // MOTOR
                    MotorNumber = vi.MotorNo,

                    // BATTERY
                    BatteryNo = vi.BatteryNo,
                    BatteryNo2 = vi.BatteryNo2,
                    BatteryNo3 = vi.BatteryNo3,
                    BatteryNo4 = vi.BatteryNo4,
                    BatteryNo5 = vi.BatteryNo5,
                    BatteryNo6 = vi.BatteryNo6,

                    BatteryCapacity = vi.BatteryCapacity,

                    // SUBSIDY
                    SubsidyAmount = im.Fame2amount,

                    FameIIRequired =
                        im.Fame2amount > 0,

                    // TOTAL
                    TotalAmount = h.TotalAmount,

                    // BILL DATE
                    BillDate = h.CreatedDate
                };

            return await query
                .OrderByDescending(x => x.BillDate)
                .ToListAsync();
        }
    }
}