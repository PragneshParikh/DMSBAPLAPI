using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.HSRPRepo
{
    public interface IHSRPRepo
    {
        Task<string> GetHSRPLoginTokenAsync();
        Task<bool> ReceiveDispatchAsync(List<HSRPDispatchItem> request);
        Task<List<VehicleSaleBillResponseViewModel>> GetAllInvoicedVehicleForHSRPOrder(string? dealerCode);
        //Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode);
        Task<List<Hsrporder>> CreateBulkHSRPOrder(
    List<HSRPOrderCreateViwModel> orders,
    string accessToken);
        // Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode);
        Task<HSRPOrderAddEditViewModel> GetHSRPOrderByIdAsync(int id);
        // Task<List<Hsrporder>> UpdateBulkHSRPOrder(List<HSRPOrderCreateViwModel> orders);
        // Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode);
        Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders);

        Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<HSRPExcelViewModel>> GetHSRPOrderForExcel(bool isSuperAdmin, string? dealerCode, DateTime? fromDate, DateTime? toDate);
        Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders, string accessToken);
        Task<HSRPFitmentResponse> ReceiveFitmentAsync(HSRPFitmentRequestData request);
    }
}
