using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.HSRPService
{
    public interface IHSRPService
    {
        Task<List<VehicleSaleBillResponseViewModel>> GetAllInvoicedVehicleForHSRPOrder(string? dealerCode);
       // Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode);
        Task<List<Hsrporder>> CreateBulkHSRPOrder(List<HSRPOrderCreateViwModel> order);
       // Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode);
        Task<HSRPOrderAddEditViewModel> GetAllHSRPOrderByIdAsync(int id);
        Task<List<Hsrporder>> UpdateBulkHSRPOrder(List<HSRPOrderCreateViwModel> orders);
        //Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode);
        Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders);

        Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<byte[]> DownloadHSRPExcel(bool isSuperAdmin,string? dealerCode, DateTime? fromDate, DateTime? toDate);
    }
}
