using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DMS_BAPL_Utils.ViewModels.RepairBillViewModel;

namespace DMS_BAPL_Data.Repositories.RepairBillRepo
{
    public interface IRepairBillRepo
    {
        Task<int> InsertRepairBill(RepairBillInsertVM model, string userId);
        Task<List<RepairBillListVM>> GetAllRepairBillList(RepairBillSearchVM search);

        Task<RepairBillUpdateVM?> GetRepairBillById(int id);
        Task<bool> UpdateRepairBill(RepairBillUpdateVM model, string userId);

        Task<RepairBillPerformaVM> generateRepairBillPerformaDetails(string DealerCode, int RepairBillId);
    }
}
