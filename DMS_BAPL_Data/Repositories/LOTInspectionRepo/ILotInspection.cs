using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DMS_BAPL_Data.Repositories.LOTInspectionRepo
{
    public interface ILotInspection
    {
        Task<int> InsertLotInspectionHeaderAsync(string invoiceNo, string userId);
        Task<bool> UpdateLotInspectionAsync(LotInspectionViewModel model, string userId);
        Task<List<LotInspectionHeaderList>> GetAllLotInspectionHeaderDetailsAsync(string? search);
    }
}
