using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.WarrantyJobCardClaimRepo
{
    public interface IWarrantyJobCardClaimRepo
    {
        Task<int> InsertWarrantyJCClaim(WarrantyJCClaimViewModel model, string userId);
    }
}
