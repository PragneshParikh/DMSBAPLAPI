using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CircularRepo
{
    public interface ICircularRepo
    {
        Task<IEnumerable<CircularMasterViewModel>> Get();
        Task<int> Create(CircularMaster circularMaster);
        Task<bool> Delete(int Id);
        Task<CircularMasterViewModel> GetById(int Id);
        Task<int> Update(CircularMaster circularMaster);
        //Task<CircularMasterViewModel> GetByDate(DateTime date);
    }
}
