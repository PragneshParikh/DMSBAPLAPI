using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.FileService
{
    public partial class FileService : IFileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<UploadUrlResponse> GenerateUploadUrl(string fileName)
        {
            string connectionString = _configuration["AzureStorage:ConnectionString"];
            string containerName = _configuration["AzureStorage:ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            string blobName = $"{DateTime.UtcNow.Ticks}_{fileName}";

            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(
                BlobSasPermissions.Create |
                BlobSasPermissions.Write);

            return new UploadUrlResponse
            {
                SasUri = blobClient.GenerateSasUri(sasBuilder).ToString(),
                BlobUrl = blobClient.Uri.ToString()
            };
        }
    }
}
