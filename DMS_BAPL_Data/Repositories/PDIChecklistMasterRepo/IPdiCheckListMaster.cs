using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PDIChecklistMasterRepo
{
    public interface IPdiCheckListMaster
    {

        Task<(bool Success, string Message)> InsertPdiChecklistMaster(PdiChecklistMasterViemModel pdiChecklistMaster);

        Task<(bool Success, string Message)> UpdatePdiChecklistMaster(PdiChecklistMasterViemModel pdiChecklistMaster);

        Task<(bool Success, string Message)> DeletePdiChecklistMaster(int pdicheckId);
        Task<List<PdiChecklistMasterViemModel>> GetPdiChecklistMasterList(string? pdicheckName);
    }
}
