using DMS_BAPL_Utils.ViewModels;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Repositories.UwLineItemRepo
{
    public interface IUwLineItemRepo
    {
        Task InsertUwLineItem(int warrantyJcclaimId, string? userId);
        Task<UwLineItemSearchResultViewModel> SearchUwLineItems(UwLineItemSearchViewModel filter);
        Task<(bool Success, string? ErrorMessage)> ApproveUwLineItem(UwLineItemActionViewModel model, string? userId);
        Task<(bool Success, string? ErrorMessage)> RejectUwLineItem(UwLineItemActionViewModel model, string? userId);
        Task<(bool Success, string? ErrorMessage)> DeleteUwLineItem(int id);
    }
}