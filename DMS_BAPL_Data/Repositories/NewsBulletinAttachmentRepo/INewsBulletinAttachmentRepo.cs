using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.NewsBulletinAttachmentRepo
{
    public interface INewsBulletinAttachmentRepo
    {
        Task<IEnumerable<NewsBulletinMasterAttachment>> Get();
        Task<NewsBulletinMasterAttachment> GetById(int Id);
        Task<IEnumerable<NewsBulletinMasterAttachment>> GetByNewsBulletinId(int Id);
        Task<bool> Insert(IEnumerable<NewsBulletinMasterAttachment> newsBulletinMasterAttachment);
        Task<bool> Delete(List<int> attachmentsIds);
    }
}
