using DMS_BAPL_Data.Repositories.LOTInspectionRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LOTInspectionService
{
    public class LotInspectionDetailsService : ILotInspectionDetailsService
    {
        private readonly ILotInspectionDetails _lotInspectionDetails;
        public LotInspectionDetailsService(ILotInspectionDetails lotInspectionDetails)
        {
            _lotInspectionDetails = lotInspectionDetails;
        }
        public async Task<bool> InsertDetailsByInvoiceAsync(InsertDetailsByInvoiceViewModel model, string userId)
        {
            try
            {
                return await _lotInspectionDetails.InsertDetailsByInvoiceAsync(model, userId);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception($"Error inserting details by invoice: {ex.Message}", ex);
            }
        }

        public async Task<int> InsertLotDetailsByInvoiceNo(string invoiceNo, int id, string userId)
        {
            return await _lotInspectionDetails.InsertLotDetailsByInvoiceNo(invoiceNo, id, userId);
        }
    }
}
