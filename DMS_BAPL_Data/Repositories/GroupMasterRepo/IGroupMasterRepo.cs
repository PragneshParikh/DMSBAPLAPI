using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.GroupMasterRepo
{
    public interface IGroupMasterRepo
    {
        Task<int> InsertGroup(GroupMasterViewModel groupMasterViewModel, string userId);
        Task<int> UpdateGroupName(int id, string groupName, string userId);
        Task<int> DeleteGroup(int groupId);
        Task<List<GroupMasterViewModel>> GetAllGroups();
        Task<byte[]> DownloadGroupMasterExcel();


    }
}
