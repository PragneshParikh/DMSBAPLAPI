using Microsoft.AspNetCore.Http;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ChassisImportRequest
    {
        public IFormFile File { get; set; }
    }
}