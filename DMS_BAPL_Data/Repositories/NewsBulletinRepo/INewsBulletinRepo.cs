using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.NewsBulletinRepo
{
    public interface INewsBulletinRepo
    {
        Task<IEnumerable<NewsBulletinMasterViewModel>> Get();
        Task<int> Create(NewsBulletinMaster newsBulletinMaster);
        Task<bool> Delete(int Id);
        Task<NewsBulletinMasterViewModel> GetById(int Id);
        Task<int> Update(NewsBulletinMaster newsBulletinMaster);
        //Task<NewsBulletinMasterViewModel> GetByDate(DateTime date);
    }
}
