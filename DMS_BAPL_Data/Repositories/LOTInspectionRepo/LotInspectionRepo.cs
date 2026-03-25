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
                    throw new Exception("Invoice already accepted");

                // Get invoice data from DB table
                var invoiceData = await _context.LotinspectionHeaders
                    .FirstOrDefaultAsync(x => x.InvoiceNo == invoiceNo);

                if (invoiceData == null)
                    throw new Exception("Invoice not found");

                //Get sequence LOT No
                var lotNo = await _context
                    .Set<TempNumber>()
                    .FromSqlRaw("SELECT NEXT VALUE FOR LotNo_Seq AS Value")
                    .Select(x => x.Value)
                    .FirstAsync();

                //  Create LOT inspaction Header
                var header = new LotinspectionHeader
                {
                    InvoiceNo = invoiceData.InvoiceNo,
                    InvoiceDate = invoiceData.InvoiceDate,
                    LotNo = lotNo, //  sequence used LOT No
                    ArrivalDate = invoiceData.ArrivalDate,
                    ArrivalTime = invoiceData.ArrivalTime,
                    LrNo = invoiceData.LrNo,
                    LrDate = invoiceData.LrDate,
                    TruckNo = invoiceData.TruckNo,
                    TransporterName = invoiceData.TransporterName,
                    DriverName = invoiceData.DriverName,
                    DriverContact = invoiceData.DriverContact,
                    CommonRemarks = invoiceData.CommonRemarks,
                    VehicleFasteningBracket = invoiceData.VehicleFasteningBracket,
                    PlasticCover = invoiceData.PlasticCover,
                    SupervisorName = invoiceData.SupervisorName,
                    DealerCode = invoiceData.DealerCode,
                    LocCode = invoiceData.LocCode,
                    CreatedBy =  userId,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                };

                _context.LotinspectionHeaders.Add(header);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return header.Id; 
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
