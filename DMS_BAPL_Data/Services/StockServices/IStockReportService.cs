using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.StockReportService
{
    public interface IStockReportService
    {
        Task<List<StockReportViewModel>>
            GetDealerWiseReportAsync();

        Task<List<StockReportViewModel>>
            GetColourWiseReportAsync();
    }
}