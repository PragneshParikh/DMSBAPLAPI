using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.NewsBulletinService
{
    public interface ICircularService
    {
        Task<object> Get();
        Task<int> Create(CircularMasterViewModel circularMasterViewModel);
        Task<bool> Delete(int Id);
        Task<CircularMasterViewModel> GetById(int Id);
        Task<int> Update(CircularMasterViewModel circularMasterViewModel);
        //Task<CircularMasterViewModel> GetByDate(DateTime date);
    }
}
