using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.StockReportRepo
{
    public interface IStockReportRepo
    {
        Task<List<StockReportViewModel>>
            GetDealerWiseReportAsync();

        Task<List<StockReportViewModel>>
            GetColourWiseReportAsync();
    }
}