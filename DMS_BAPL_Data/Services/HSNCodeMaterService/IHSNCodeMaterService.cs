using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.HSNCodeMaterService
{
    public interface IHSNCodeMaterService
    {
        Task<List<HsncodeMaster>> GetAllHSNCodeListAsync(string? search);
        Task<HsncodeMaster?> GetByIdAsync(int id);
        Task<HsncodeMaster> AddAsync(HSNCodeMasterViewModel entity);
        Task<bool> UpdateAsync(int id, HSNCodeMasterViewModel entity);
        Task<byte[]> downloadHSNCodeExcel();
    }
}
