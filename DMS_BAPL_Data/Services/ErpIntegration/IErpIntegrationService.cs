using DMS_BAPL_Utils.ViewModels;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ErpIntegration
{
    public interface IErpIntegrationService
    {
        Task<ErpHttpSubmitResult> SubmitWarrantyClaimLines(int invoiceId, ErpWarrantyClaimSubmitRequest payload);
    }

    public class ErpHttpSubmitResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UniqueId { get; set; }
    }
}