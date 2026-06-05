using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.FileService
{
    public interface IFileService
    {
        Task<UploadUrlResponse> GenerateUploadUrl(string fileName);
    }
}
