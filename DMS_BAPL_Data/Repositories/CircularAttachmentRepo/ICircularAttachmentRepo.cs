using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CircularAttachmentRepo
{
    public interface ICircularAttachmentRepo
    {
        Task<IEnumerable<CircularMasterAttachment>> Get();
        Task<CircularMasterAttachment> GetById(int Id);
        Task<IEnumerable<CircularMasterAttachment>> GetByCircularId(int Id);
        Task<bool> Insert(IEnumerable<CircularMasterAttachment> circularMasterAttachment);
        Task<bool> Delete(List<int> attachmentsIds);
    }
}
