using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.NewsBulletinAttachmentService
{
    public interface ICircularAttachmentService
    {
        Task<IEnumerable<CircularMasterAttachment>> Get();
        Task<CircularMasterAttachment> GetById(int Id);
        Task<IEnumerable<CircularMasterAttachment>> GetByCircularId(int Id);
        Task<bool> Insert(List<CircularMasterAttachment> circularMasterAttachment);
        Task<bool> Delete(List<int> attachmentIds);
    }
}
