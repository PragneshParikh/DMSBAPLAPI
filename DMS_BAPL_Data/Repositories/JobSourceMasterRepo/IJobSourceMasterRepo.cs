using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.JobSourceMasterRepo
{
    public interface IJobSourceMasterRepo
    {
        Task<int> InsertJobSource(JobSourceMasterViewModel jobSourceMasterViewModel, string userId);
        Task<int> UpdateJobSourceName(JobSourceMasterViewModel jobSourceMasterViewModel, string userId);
        Task<int> DeleteJobSource(int jobSourceId);
        Task<List<JobSourceMasterViewModel>> GetAllJobSource();
        Task<byte[]> DownloadjobsourceMasterExcel();
    }
}
