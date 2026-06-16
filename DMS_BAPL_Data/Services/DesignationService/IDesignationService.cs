using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DesignationService
{
    public interface IDesignationService
    {
        Task<IEnumerable<DesignationMaster>> Get();
        Task<bool> Insert(DesignationViewModel designationViewModel);
        Task<int> Update(DesignationViewModel designationViewModel);
    }
}
