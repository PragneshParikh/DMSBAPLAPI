using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PartsInwardService
{
    public interface IPartInwardService
    {
        Task<IEnumerable<PartsInward>> Get();
        Task<IEnumerable<PartsInward>> GetPartInwardByDealerAsync(string dealerCode);
        Task<bool> UpdateByInvoice(string invoiceNo, string dealerCode);
        Task<object> PartsInward(PartsInwardViewModel partsInwardViewModel);
    }
}
