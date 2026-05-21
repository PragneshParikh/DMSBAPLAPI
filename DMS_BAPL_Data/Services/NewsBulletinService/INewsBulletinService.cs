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
    public interface INewsBulletinService
    {
        Task<object> Get();
        Task<int> Create(NewsBulletinMasterViewModel newsBulletinMasterViewModel);
        Task<bool> Delete(int Id);
        Task<NewsBulletinMasterViewModel> GetById(int Id);
        Task<int> Update(NewsBulletinMasterViewModel newsBulletinMasterViewModel);
        //Task<NewsBulletinMasterViewModel> GetByDate(DateTime date);
    }
}
