using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.Form22MasterRepo
{
    public interface IForm22MasterRepo
    {

        Task<Form22MasterViewModel> InsertForm22MasterAsync(Form22MasterViewModel form22MasterViewModel);

        Task<List<Form22Master>> GetForm22MastersAsync(string? search);

        Task<Form22Master> GetForm22MasterByIdAsync(int id);

        Task<Form22Master> UpdateForm22MasterAsync(int id, Form22MasterViewModel form22MasterViewModel);

        Task<List<OemModelViewModel>> GetOemmodelMastersList();
    }
}
