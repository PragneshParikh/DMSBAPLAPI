using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.LineOrderReportRepo
{
    public interface ILineOrderReportRepo
    {
        Task<PagedResponse<object>> GetPagedAsync(LineOrderReportFilterModel filter);
        Task<List<DmsLineOrderReport>> GetAllAsync(LineOrderReportFilterModel filter);
    }
}