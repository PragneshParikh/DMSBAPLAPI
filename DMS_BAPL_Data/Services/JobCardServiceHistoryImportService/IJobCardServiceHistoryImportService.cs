using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.JobCardServiceHistoryImportService
{
    public interface IJobCardServiceHistoryImportService
    {
        List<JcshFieldViewModel> GetFields();
        /// <summary>Saves the workbook to temp storage and returns headers, sample rows, mapping and destination status.</summary>
        Task<JcshUploadResponse> UploadAsync(Stream excel, string fileName, string userId, CancellationToken ct);
        /// <summary>Starts a background validate (DryRun) / import job. Throws InvalidOperationException if one is already running.</summary>
        JcshJobStatus Start(Guid uploadId, JcshRunOptions options, string userId);
        JcshJobStatus? GetJob(Guid jobId);
        JcshJobStatus? Cancel(Guid jobId);
        bool Discard(Guid uploadId, string userId);
    }

    public interface IJobCardServiceHistoryReadService
    {
        Task<JcshHistoryPage> SearchAsync(JcshHistoryQuery q, CancellationToken ct);
        Task<List<Dictionary<string, object?>>> ByChassisAsync(string chassisNo, CancellationToken ct);
    }
}
