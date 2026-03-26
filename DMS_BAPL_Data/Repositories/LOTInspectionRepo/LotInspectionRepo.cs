using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
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
        // create insert api for lot inspection header
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
                    DealerCode = invoiceData.DealerCode,
                    LocCode = invoiceData.LocCode,
                    CreatedBy = userId,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
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
    }
}
