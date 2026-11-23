using Microsoft.Extensions.Configuration;

namespace DMS_BAPL_Data.Services.JobCardServiceHistoryImportService
{
    /// <summary>
    /// Where the Job Card Service History lives - one place for the import, read, report and lookup code.
    /// Table: dbo.DMS_ServiceHistory (the table the JobCardScanner app's Service History page reads).
    /// Connection: "ServiceHistoryConnection" when it is set in appsettings.json (table in another database),
    /// otherwise the API's normal "DefaultConnection" (table in BAPLDMSvad).
    /// </summary>
    public static class JcshTarget
    {
        public const string TableName = "dbo.DMS_ServiceHistory";
        public const string ConnectionName = "ServiceHistoryConnection";
        public const string FallbackConnectionName = "DefaultConnection";

        public static string? ConnectionString(IConfiguration config)
        {
            var own = config.GetConnectionString(ConnectionName);
            return string.IsNullOrWhiteSpace(own) ? config.GetConnectionString(FallbackConnectionName) : own;
        }

        /// <summary>Which connection string is in use, for messages.</summary>
        public static string ConnectionLabel(IConfiguration config) =>
            string.IsNullOrWhiteSpace(config.GetConnectionString(ConnectionName)) ? FallbackConnectionName : ConnectionName;
    }
}
