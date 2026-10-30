using DMS_BAPL_Utils.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleDispatchService
{
    public interface IVehicleDispatchService
    {
        /// <summary>Sheets of the uploaded workbook with row counts and missing required columns. Writes nothing.</summary>
        DispatchSheetsResponse ReadSheets(Stream excel, string fileName);

        /// <summary>Validates (DryRun = true) or imports one sheet of the Cogxim vehicle dispatch report into VehicleInward.</summary>
        Task<VehicleDispatchImportResult> ImportAsync(Stream excel, string fileName, VehicleDispatchImportOptions options, string userId);

        /// <summary>Vehicle Dispatch report (VehicleInward), paged, with totals.</summary>
        Task<VehicleDispatchReportResponse> GetReportAsync(VehicleDispatchReportFilter filter);
    }
}
