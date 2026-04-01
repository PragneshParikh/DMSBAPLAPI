using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.Form22Services
{
    public interface IForm22Service
    {

            Task<Form22MasterViewModel> InsertForm22MasterAsync(Form22MasterViewModel form22MasterViewModel);
            Task<List<Form22Master>> GetForm22MastersAsync(string? search);

            Task<Form22Master> GetForm22MasterByIdAsync(int id);
            Task<Form22Master> UpdateForm22MasterAsync(int id, Form22MasterViewModel form22MasterViewModel);

            Task<byte[]> DownloadForm22MasterExcel();



    }
}
