using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.TermConditionMasterRepo
{
    public interface ITermConditionMasterRepo
    {
        Task<int> AddTermCondition(TermConditionMasterViewModel conditionMasterViewModel, string userId);
        Task<int> UpdateTermCondition(TermConditionMasterViewModel conditionMasterViewModel, string userId);
        Task<int> DeleteTermCondition(int conditionId);
        Task<List<TermandConditionMaster>> GetAllTermCondition();

        Task<byte[]> DownloadTermConditionMasterExcel();
    }
}
