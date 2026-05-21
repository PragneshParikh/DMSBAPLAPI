using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.NewsBulletinAttachmentRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.NewsBulletinAttachmentService
{
    public partial class NewsBulletinAttachmentService : INewsBulletinAttachmentService
    {
        private readonly INewsBulletinAttachmentRepo _newsBulletinAttachmentRepo;
        public NewsBulletinAttachmentService(INewsBulletinAttachmentRepo newsBulletinAttachmentRepo)
        {
            _newsBulletinAttachmentRepo = newsBulletinAttachmentRepo;
        }
        Task<bool> INewsBulletinAttachmentService.Delete(List<int> attachmentIds) => _newsBulletinAttachmentRepo.Delete(attachmentIds);

        Task<IEnumerable<NewsBulletinMasterAttachment>> INewsBulletinAttachmentService.Get()
        {
            throw new NotImplementedException();
        }

        Task<NewsBulletinMasterAttachment> INewsBulletinAttachmentService.GetById(int Id)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<NewsBulletinMasterAttachment>> INewsBulletinAttachmentService.GetByNewsBulletinId(int Id)
        {
            throw new NotImplementedException();
        }

        Task<bool> INewsBulletinAttachmentService.Insert(List<NewsBulletinMasterAttachment> newsBulletinMasterAttachment)
       => _newsBulletinAttachmentRepo.Insert(newsBulletinMasterAttachment);
    }
}
