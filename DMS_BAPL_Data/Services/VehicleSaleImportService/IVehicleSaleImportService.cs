using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleSaleImportService
{
    public interface IVehicleSaleImportService
    {
        /// <summary>Target fields + the Excel header aliases each one auto-matches.</summary>
        List<ImportFieldViewModel> GetFields();

        /// <summary>Reads sheet list, headers, auto-mapping and first rows. Writes nothing.</summary>
        VehicleSaleImportPreviewResponse Preview(Stream excel, string fileName, string? sheetName, int headerRow);

        /// <summary>Validates (DryRun = true) or imports the sheet into
        /// VehicleSaleBillHeader / VehicleSaleBillDetails.</summary>
        Task<VehicleSaleImportResult> ImportAsync(Stream excel, string fileName, VehicleSaleImportOptions options, string userId);
    }
}
