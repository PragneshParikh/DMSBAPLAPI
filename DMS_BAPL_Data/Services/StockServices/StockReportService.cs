using DMS_BAPL_Data.Repositories.StockRepo;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.StockServices
{
    public class StockReportService
        : IStockReportService
    {
        private readonly IStockReportRepo _repo;

        public StockReportService(
            IStockReportRepo repo
        )
        {
            _repo = repo;
        }

        public async Task<List<StockReportViewModel>>
            GetDealerWiseReportAsync()
        {
            return await
                _repo.GetDealerWiseReportAsync();
        }

        public async Task<List<StockReportViewModel>>
            GetColourWiseReportAsync()
        {
            return await
                _repo.GetColourWiseReportAsync();
        }
    }
}