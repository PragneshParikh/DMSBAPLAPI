using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.JobTypeMasterRepo
{
    public interface IJobTypeMasterRepo
    {
        Task<int> InsertJobType(JobTypeMasterViewModel jobtypeMasterViewModel, string userId);
        Task<int> UpdateJobTypeName(JobTypeMasterViewModel jobtypeMasterViewModel, string userId);
        Task<int> DeleteJobType(int jobTypeId);
        Task<List<JobTypeMasterViewModel>> GetAllJobType();
        Task<byte[]> DownloadJobTypeMasterExcel();
    }
}
