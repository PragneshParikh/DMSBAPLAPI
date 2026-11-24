using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// B2C job card Photos tab (25.11.2026) - BAPLDMSvad dbo.JobCardPhoto (create it with docs/JobCardPhoto_B2C.sql).
    /// The browser uploads each image to Azure Blob with the existing api/files/generate-upload-url write SAS; this
    /// controller only stores / lists / removes the blob URL rows of a job card. Dealer users can only touch their own
    /// dealer's job cards (JobCardHeader.DealerCode = dealer code in the JWT). B2B photos use api/b2b-jobcard/{id}/photos.
    ///   GET    api/jobcard-photo/{jobCardHeaderId}
    ///   POST   api/jobcard-photo/{jobCardHeaderId}     body { photos: [{ fileName, blobUrl, contentType, fileSize, caption }] }
    ///   DELETE api/jobcard-photo/photo/{photoId}        soft delete
    /// </summary>
    [Route("api/jobcard-photo")]
    [ApiController]
    public class JobCardPhotoController : ControllerBase
    {
        private const int MaxPhotosPerJobCard = 10;
        private readonly IConfiguration _config;
        private readonly ILogger<JobCardPhotoController> _logger;

        public JobCardPhotoController(IConfiguration config, ILogger<JobCardPhotoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        public class PhotoInput
        {
            public string? FileName { get; set; }
            public string? BlobUrl { get; set; }
            public string? ContentType { get; set; }
            public long? FileSize { get; set; }
            public string? Caption { get; set; }
        }

        public class SavePhotosRequest
        {
            public List<PhotoInput> Photos { get; set; } = new();
        }

        private SqlConnection Conn() => new(_config.GetConnectionString("DefaultConnection"));

        private bool IsAdmin => GetUserInfoFromToken.GetUserGroup(HttpContext);
        private string UserId => GetUserInfoFromToken.GetUserIdFromToken(HttpContext) ?? "";

        /// <summary>null = job card not found or not this dealer's</summary>
        private async Task<string?> JobCardDealerAsync(SqlConnection conn, int jobCardHeaderId, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("SELECT ISNULL(DealerCode, '') FROM dbo.JobCardHeader WHERE Id = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = jobCardHeaderId;
            var dealer = await cmd.ExecuteScalarAsync(ct) as string;
            if (dealer == null) return null;
            if (IsAdmin) return dealer;
            var own = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim() ?? "";
            return string.Equals(own, dealer.Trim(), StringComparison.OrdinalIgnoreCase) ? dealer : null;
        }

        private static bool IsBlobUrl(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps
            && u.Host.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase) && url!.Length <= 1000;

        private async Task<IActionResult> Run(Func<CancellationToken, Task<IActionResult>> action, string what, CancellationToken ct)
        {
            if (User?.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(UserId)) return Unauthorized(new { message = "User not authorized" });
            try { return await action(ct); }
            catch (SqlException ex) when (ex.Number == 208)
            {
                return StatusCode(500, new { message = "Table dbo.JobCardPhoto does not exist yet - run docs/JobCardPhoto_B2C.sql on BAPLDMSvad." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job card photo {What} failed", what);
                return StatusCode(500, new { message = $"{what} failed: {ex.GetBaseException().Message}" });
            }
        }

        [HttpGet("{jobCardHeaderId:int}")]
        public Task<IActionResult> List(int jobCardHeaderId, CancellationToken ct) => Run(async c =>
        {
            await using var conn = Conn();
            await conn.OpenAsync(c);
            if (await JobCardDealerAsync(conn, jobCardHeaderId, c) == null) return NotFound(new { message = "Job card not found." });
            await using var cmd = new SqlCommand(@"
                SELECT Id, JobCardHeaderId, FileName, BlobUrl, ContentType, FileSize, Caption, CreatedBy, CreatedDate
                FROM dbo.JobCardPhoto WHERE JobCardHeaderId = @id AND IsDeleted = 0 ORDER BY Id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = jobCardHeaderId;
            var rows = new List<object>();
            await using var rdr = await cmd.ExecuteReaderAsync(c);
            while (await rdr.ReadAsync(c))
            {
                rows.Add(new
                {
                    id = rdr.GetInt32(0),
                    jobCardHeaderId = rdr.GetInt32(1),
                    fileName = rdr.GetString(2),
                    blobUrl = rdr.GetString(3),
                    contentType = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                    fileSize = rdr.IsDBNull(5) ? (long?)null : rdr.GetInt64(5),
                    caption = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                    createdBy = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                    createdDate = DateTime.SpecifyKind(rdr.GetDateTime(8), DateTimeKind.Utc)
                });
            }
            return Ok(rows);
        }, "Photo list", ct);

        [HttpPost("{jobCardHeaderId:int}")]
        public Task<IActionResult> Save(int jobCardHeaderId, [FromBody] SavePhotosRequest req, CancellationToken ct) => Run(async c =>
        {
            var photos = req?.Photos ?? new List<PhotoInput>();
            if (photos.Count == 0) return BadRequest(new { message = "No photos to save." });
            if (photos.Any(p => !IsBlobUrl(p.BlobUrl))) return BadRequest(new { message = "Invalid photo URL." });
            if (photos.Any(p => !string.IsNullOrEmpty(p.ContentType) && !p.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { message = "Only images can be attached." });

            await using var conn = Conn();
            await conn.OpenAsync(c);
            var dealer = await JobCardDealerAsync(conn, jobCardHeaderId, c);
            if (dealer == null) return NotFound(new { message = "Job card not found." });

            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(c);
            await using (var cnt = new SqlCommand("SELECT COUNT(*) FROM dbo.JobCardPhoto WITH (UPDLOCK, HOLDLOCK) WHERE JobCardHeaderId = @id AND IsDeleted = 0", conn, tx))
            {
                cnt.Parameters.Add("@id", SqlDbType.Int).Value = jobCardHeaderId;
                var existing = (int)(await cnt.ExecuteScalarAsync(c) ?? 0);
                if (existing + photos.Count > MaxPhotosPerJobCard)
                    return BadRequest(new { message = $"A job card can have at most {MaxPhotosPerJobCard} photos." });
            }
            foreach (var p in photos)
            {
                await using var ins = new SqlCommand(@"
                    INSERT INTO dbo.JobCardPhoto (JobCardHeaderId, DealerCode, Stage, FileName, BlobUrl, ContentType, FileSize, Caption, CreatedBy)
                    VALUES (@id, @dealer, 'CheckIn', @name, @url, @type, @size, @caption, @by)", conn, tx);
                ins.Parameters.Add("@id", SqlDbType.Int).Value = jobCardHeaderId;
                ins.Parameters.Add("@dealer", SqlDbType.NVarChar, 50).Value = (object?)dealer ?? DBNull.Value;
                ins.Parameters.Add("@name", SqlDbType.NVarChar, 260).Value = Trunc(p.FileName, 260) ?? "photo";
                ins.Parameters.Add("@url", SqlDbType.NVarChar, 1000).Value = p.BlobUrl!;
                ins.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = (object?)Trunc(p.ContentType, 100) ?? DBNull.Value;
                ins.Parameters.Add("@size", SqlDbType.BigInt).Value = (object?)p.FileSize ?? DBNull.Value;
                ins.Parameters.Add("@caption", SqlDbType.NVarChar, 200).Value = (object?)Trunc(p.Caption, 200) ?? DBNull.Value;
                ins.Parameters.Add("@by", SqlDbType.NVarChar, 100).Value = Trunc(UserId, 100) ?? "";
                await ins.ExecuteNonQueryAsync(c);
            }
            await tx.CommitAsync(c);
            return Ok(new { message = $"{photos.Count} photo(s) saved.", count = photos.Count });
        }, "Save photos", ct);

        [HttpDelete("photo/{photoId:int}")]
        public Task<IActionResult> Delete(int photoId, CancellationToken ct) => Run(async c =>
        {
            await using var conn = Conn();
            await conn.OpenAsync(c);
            int jobCardHeaderId;
            await using (var get = new SqlCommand("SELECT JobCardHeaderId FROM dbo.JobCardPhoto WHERE Id = @id AND IsDeleted = 0", conn))
            {
                get.Parameters.Add("@id", SqlDbType.Int).Value = photoId;
                var v = await get.ExecuteScalarAsync(c);
                if (v == null) return NotFound(new { message = "Photo not found." });
                jobCardHeaderId = (int)v;
            }
            if (await JobCardDealerAsync(conn, jobCardHeaderId, c) == null) return NotFound(new { message = "Photo not found." });
            await using var upd = new SqlCommand("UPDATE dbo.JobCardPhoto SET IsDeleted = 1, DeletedBy = @by, DeletedDate = SYSUTCDATETIME() WHERE Id = @id", conn);
            upd.Parameters.Add("@id", SqlDbType.Int).Value = photoId;
            upd.Parameters.Add("@by", SqlDbType.NVarChar, 100).Value = Trunc(UserId, 100) ?? "";
            await upd.ExecuteNonQueryAsync(c);
            return Ok(new { message = "Photo removed." });
        }, "Remove photo", ct);

        private static string? Trunc(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length > max ? s[..max] : s;
        }
    }
}
