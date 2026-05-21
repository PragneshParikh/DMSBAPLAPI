using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.NewsBulletinAttachmentService
{
    public interface INewsBulletinAttachmentService
    {
        Task<IEnumerable<NewsBulletinMasterAttachment>> Get();
        Task<NewsBulletinMasterAttachment> GetById(int Id);
        Task<IEnumerable<NewsBulletinMasterAttachment>> GetByNewsBulletinId(int Id);
        Task<bool> Insert(List<NewsBulletinMasterAttachment> newsBulletinMasterAttachment);
        Task<bool> Delete(List<int> attachmentIds);
    }
}
