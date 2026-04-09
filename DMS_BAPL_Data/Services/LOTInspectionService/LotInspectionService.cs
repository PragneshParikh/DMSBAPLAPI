using DMS_BAPL_Data.Repositories.LOTInspectionRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LOTInspectionService
{
    public class LotInspectionService : ILotInspectionService
    {
        private readonly ILotInspection _lotInspection;

        public LotInspectionService(ILotInspection lotInspection)
        {
            _lotInspection = lotInspection;
        }
        public async Task<int> InsertLotInspectionHeaderAsync(string invoiceNo, string userId)
        {
            return await _lotInspection.InsertLotInspectionHeaderAsync(invoiceNo, userId);
        }
        public async Task<bool> UpdateLotInspectionAsync(LotInspectionViewModel model, string userId, string dealerCode)
        {
            return await _lotInspection.UpdateLotInspectionAsync(model, userId, dealerCode);
        }
        public async Task<List<LotInspectionHeaderList>> GetAllLotInspectionHeaderDetailsAsync(string? search)
        {
            return await _lotInspection.GetAllLotInspectionHeaderDetailsAsync(search);
        }
    }
}
