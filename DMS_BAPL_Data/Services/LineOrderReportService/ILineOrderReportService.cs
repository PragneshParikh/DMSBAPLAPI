using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.LineOrderReportService
{
    public interface ILineOrderReportService
    {
        Task<PagedResponse<object>> GetLineOrderReport(LineOrderReportFilterModel filter);
        Task<byte[]> DownloadLineOrderExcel(LineOrderReportFilterModel filter);
    }
}