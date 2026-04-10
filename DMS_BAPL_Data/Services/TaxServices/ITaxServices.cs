using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.TaxServices
{
    public interface ITaxServices
    {
        Task<List<TaxDetailViewModel>> GetTaxDetailsAsync(string itemCode, string userLocation);
    }
}
