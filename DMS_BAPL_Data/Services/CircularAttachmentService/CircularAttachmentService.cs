using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.CircularAttachmentRepo;
using DMS_BAPL_Data.Services.NewsBulletinAttachmentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.CircularAttachmentService
{
    public partial class CircularAttachmentService : ICircularAttachmentService
    {
        private readonly ICircularAttachmentRepo _circularAttachmentRepo;
        public CircularAttachmentService(ICircularAttachmentRepo circularAttachmentRepo)
        {
            _circularAttachmentRepo = circularAttachmentRepo;
        }
        Task<bool> ICircularAttachmentService.Delete(List<int> attachmentIds) => _circularAttachmentRepo.Delete(attachmentIds);

        Task<IEnumerable<CircularMasterAttachment>> ICircularAttachmentService.Get()
        {
            throw new NotImplementedException();
        }

        Task<CircularMasterAttachment> ICircularAttachmentService.GetById(int Id)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<CircularMasterAttachment>> ICircularAttachmentService.GetByCircularId(int Id)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICircularAttachmentService.Insert(List<CircularMasterAttachment> circularMasterAttachment)
       => _circularAttachmentRepo.Insert(circularMasterAttachment);
    }
}
