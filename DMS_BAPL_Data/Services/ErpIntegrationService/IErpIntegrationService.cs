using System.Threading.Tasks;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.ErpIntegration
{
    public interface IErpIntegrationService
    {
        Task<ErpSubmitResult> SubmitWarrantyClaimLines(ErpWarrantyClaimSubmitRequest request);
    }
}