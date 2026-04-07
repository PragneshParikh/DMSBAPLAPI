using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LOTInspectionRepo
{
    public class LotInspectionRepo : ILotInspection
    {
        private readonly BapldmsvadContext _context;
        public LotInspectionRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        // create insert api for lot inspection header based on invoice no saved
        // in LOTinspectionHeader Table 
        public async Task<int> InsertLotInspectionHeaderAsync(string invoiceNo, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //  Check duplicate
                var isExist = await _context.LotinspectionHeaders
                    .AnyAsync(x => x.InvoiceNo == invoiceNo);

                if (isExist)
                    return 0;

                // Get invoice data from DB table
                var invoiceData = await _context.VehicleDispatches
                    .FirstOrDefaultAsync(x => x.InvoiceNo == invoiceNo);

                if (invoiceData == null)
                    return 0;

                //Get sequence LOT No
                var maxLotNo = await _context.LotinspectionHeaders
                                .MaxAsync(x => (int?)x.LotNo) ?? 0;

                var nextLotNo = maxLotNo + 1;

                //  Create LOT inspaction Header
                var header = new LotinspectionHeader
                {
                    InvoiceNo = invoiceData.InvoiceNo,
                    InvoiceDate = (DateOnly)invoiceData.InvoiceDate,
                    LotNo = nextLotNo,
                    ArrivalDate = null,
                    ArrivalTime = null,
                    LrNo = null,
                    LrDate = null,
                    TruckNo = null,
                    TransporterName = null,
                    DriverName = null,
                    DriverContact = null,
                    CommonRemarks = null,
                    VehicleFasteningBracket = null,
                    PlasticCover = null,
                    SupervisorName = null,
                    LocationName = null,
                    DealerCode = invoiceData.DealerCode,
                    LocCode = invoiceData.LocCode,
                    CreatedBy = userId,
                    CreatedDate = (DateTime.Now),
                };

                _context.LotinspectionHeaders.Add(header);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return header.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //update api
        public async Task<bool> UpdateLotInspectionAsync(LotInspectionViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ================= HEADER =================
                var header = await _context.LotinspectionHeaders
                    .FirstOrDefaultAsync(x =>
                        x.InvoiceNo == model.lotInspectedHeaderDetails.invoiceNo &&
                        x.DealerCode == model.lotInspectedHeaderDetails.dealerCode);

                if (header == null) return false;

                if (!string.IsNullOrEmpty(model.lotInspectedHeaderDetails.arrivalDate) &&
                    DateTime.TryParse(model.lotInspectedHeaderDetails.arrivalDate, out var parsedDate))
                {
                    header.ArrivalDate = DateOnly.FromDateTime(parsedDate);
                }

                header.ArrivalTime = model.lotInspectedHeaderDetails.arrivalTime;
                header.LrNo = model.lotInspectedHeaderDetails.lrNo;
                if (!string.IsNullOrEmpty(model.lotInspectedHeaderDetails.lrDate) &&
                 DateTime.TryParse(model.lotInspectedHeaderDetails.lrDate, out var lrParsedDate))
                {
                    header.LrDate = DateOnly.FromDateTime(lrParsedDate);
                }
                header.TruckNo = model.lotInspectedHeaderDetails.truckNo;
                header.TransporterName = model.lotInspectedHeaderDetails.transporterName;
                header.DriverName = model.lotInspectedHeaderDetails.driverName;
                header.DriverContact = model.lotInspectedHeaderDetails.driverContact;
                header.CommonRemarks = model.lotInspectedHeaderDetails.commonRemarks;

                header.VehicleFasteningBracket = model.lotInspectedHeaderDetails.vehicleFasteningBracket;
                header.PlasticCover = model.lotInspectedHeaderDetails.plasticCover;
                header.SupervisorName = model.lotInspectedHeaderDetails.nameSupervisor;
                header.SupervisorName = model.lotInspectedHeaderDetails.nameSupervisor;
                header.LocationName = model.lotInspectedHeaderDetails.locationName;

                header.UpdateBy = userId;
                //                if (!(string.IsNullOrEmpty(model.lotInspectedHeaderDetails.updatedDate) &&
                //!DateTime.            TryParse(model.lotInspectedHeaderDetails.updatedDate, out var parsedUpdateDate)))
                //                {
                //                    header.UpdatedDate = DateOnly.FromDateTime(parsedUpdateDate);
                //                }


                header.UpdatedDate = DateTime.Now;
                header.IsLotInspected = true;

                await _context.SaveChangesAsync();

                // ================= DETAILS =================
                var detailIds = model.lotInspectedDetails.Select(x => x.Id).ToList();

                var dbDetails = await _context.LotinspectionDetails
                    .Where(x => x.LotHeaderId == header.Id)
                    .ToListAsync();

                foreach (var item in model.lotInspectedDetails)
                {
                    var detail = dbDetails.FirstOrDefault(x => x.ChassisNo == item.chassisNo
                                                          && x.LotHeaderId == item.lotHeaderID);

                    if (detail != null)
                    {
                        detail.KeyFobSetQty = item.keyFobSetQty;
                        detail.ChargerQty = item.chargerQty;
                        detail.MirrorsetQty = item.mirrorsetQty;
                        detail.FirstaidkitQty = item.firstaidkitQty;
                        detail.ToolKitQty = item.toolKitQty;

                        detail.OwnersManual = item.ownersManual;
                        detail.IgnitionKeyset = item.ignitionKeyset;
                        detail.AttributeCard = item.attributeCard;
                        detail.ChargingKit = item.chargingKit;
                        if (!string.IsNullOrEmpty(item.inspectionDate) &&
                         DateTime.TryParse(item.inspectionDate, out var inspectionparsedDate))
                        {
                            detail.InspectionDate = DateOnly.FromDateTime(inspectionparsedDate);
                        }
                        detail.VehicleStatus = item.vehicleStatus;
                        detail.DamageDetails = item.damageDetails;
                        detail.ChassisWiseRemarks = item.chassisWiseRemarks;
                        detail.UpdateBy = userId;
                        detail.UpdatedDate = DateTime.Now;

                        //// lotVehicleDamageImage FILE
                        //if (item.lotVehicleDamageImage != null)
                        //{
                        //    var path = Path.Combine(_env.WebRootPath, "uploads");
                        //    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                        //    var fileName = Guid.NewGuid() + Path.GetExtension(item.lotVehicleDamageImage.FileName);
                        //    var filePath = Path.Combine(path, fileName);

                        //    using var stream = new FileStream(filePath, FileMode.Create);
                        //    await item.lotVehicleDamageImage.CopyToAsync(stream);

                        //    detail.LotVehicleDamageImage = fileName;
                        //}
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<List<LotInspectionHeaderList>> GetAllLotInspectionHeaderDetailsAsync(string? search)
        {
            try
            {
                var query = _context.LotinspectionHeaders.AsQueryable();

                //  SEARCH LOGIC
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    // Case 1: Date Range (e.g. 01-04-2026 to 31-03-2027)
                    if (search.Contains("to"))
                    {
                        var parts = search.Split("to");

                        if (parts.Length == 2 &&
                            DateOnly.TryParse(parts[0], out DateOnly fromDate) &&
                            DateOnly.TryParse(parts[1], out DateOnly toDate))
                        {
                            query = query.Where(x =>
                                x.InvoiceDate >= fromDate &&
                                x.InvoiceDate <= toDate
                            );
                        }
                    }
                    // Case 2: Single Date
                    else if (DateOnly.TryParse(search, out DateOnly singleDate))
                    {
                        query = query.Where(x =>
                            x.InvoiceDate == singleDate
                        );
                    }
                    else
                    {
                        //  Case 3: Invoice number based Search
                        query = query.Where(x =>
                            x.InvoiceNo.Contains(search)
                        );
                    }
                }

                var lotHeaderDetails = await query
                    .OrderByDescending(x => x.InvoiceDate)
                    .Select(x => new LotInspectionHeaderList
                    {
                        InvoiceNo = x.InvoiceNo,
                        InvoiceDate = x.InvoiceDate,
                        Lotno = x.LotNo,
                        ArrivalDate = x.ArrivalDate,
                        ArrivalTime = x.ArrivalTime,
                        Lrno = x.LrNo,
                        Lrdate = x.LrDate,
                        TruckNo = x.TruckNo,
                        TransporterName = x.TransporterName,
                        DriverName = x.DriverName,
                        DriverContact = x.DriverContact,
                        CommonReMarks = x.CommonRemarks,
                        VehicleFasteningBracket = x.VehicleFasteningBracket,
                        PlasticCover = x.PlasticCover,
                        NameSupervisor = x.SupervisorName,
                        LocationName = x.LocationName
                    })
                    .ToListAsync();

                return lotHeaderDetails ?? new List<LotInspectionHeaderList>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching lot inspection header details", ex);
            }
        }
    }
}
