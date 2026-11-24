using Microsoft.AspNetCore.Http;

namespace DMS_BAPL_Api.Helpers
{
    /// <summary>
    /// Job card photos on the API server's disk (25.11.2026) - the same way JobCardScanner stores them
    /// (JobCardsController.UploadPhoto: wwwroot/uploads/jobcard-photos/{jobCardId}/{guid}.ext), so no Azure Blob
    /// storage / keys are needed. Program.cs serves the folder at /uploads/jobcard-photos.
    ///   B2B: {JobCardScanner JobCards.Id}/...      B2C: b2c-{BAPLDMSvad JobCardHeader.Id}/...
    /// The saved URL is absolute (JobCardPhotos:PublicBaseUrl in appsettings, else this API's own address), so the
    /// JobCardScanner app can also show B2B photos uploaded from DMS.
    /// </summary>
    public static class JobCardPhotoStorage
    {
        public const string RequestPath = "/uploads/jobcard-photos";
        public const long MaxBytes = 10 * 1024 * 1024;   // 10 MB per photo
        public const int MaxPhotosPerJobCard = 10;

        public static string Root(string contentRootPath) =>
            Path.Combine(contentRootPath, "wwwroot", "uploads", "jobcard-photos");

        public sealed class Saved
        {
            public string Url { get; init; } = "";
            public string FileName { get; init; } = "";
            public string ContentType { get; init; } = "";
            public long Size { get; init; }
        }

        /// <summary>content type + extension from the file's first bytes (never trusts the name / browser type)</summary>
        private static (string Type, string Ext)? Detect(byte[] h, int n)
        {
            if (n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ("image/jpeg", ".jpg");
            if (n >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return ("image/png", ".png");
            if (n >= 6 && h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46) return ("image/gif", ".gif");
            if (n >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50)
                return ("image/webp", ".webp");
            if (n >= 12 && h[4] == 0x66 && h[5] == 0x74 && h[6] == 0x79 && h[7] == 0x70)
            {
                var brand = System.Text.Encoding.ASCII.GetString(h, 8, 4);
                if (brand is "heic" or "heix" or "mif1" or "msf1" or "heif") return ("image/heic", ".heic");
            }
            return null;
        }

        /// <summary>throws InvalidOperationException with a user message for a bad file</summary>
        public static async Task<Saved> SaveAsync(IFormFile file, string folder, HttpRequest request, IConfiguration config,
            string contentRootPath, CancellationToken ct)
        {
            if (file == null || file.Length == 0) throw new InvalidOperationException("Empty file.");
            if (file.Length > MaxBytes) throw new InvalidOperationException($"{file.FileName}: larger than 10 MB.");

            var head = new byte[16];
            int n;
            await using (var s = file.OpenReadStream()) n = await s.ReadAsync(head.AsMemory(0, head.Length), ct);
            var kind = Detect(head, n) ?? throw new InvalidOperationException($"{file.FileName}: only JPG, PNG, WEBP, GIF or HEIC photos can be attached.");

            var safeFolder = string.Concat(folder.Where(c => char.IsLetterOrDigit(c) || c == '-'));
            var dir = Path.Combine(Root(contentRootPath), safeFolder);
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid():N}{kind.Ext}";
            await using (var outFile = File.Create(Path.Combine(dir, name)))
            await using (var input = file.OpenReadStream())
                await input.CopyToAsync(outFile, ct);

            return new Saved
            {
                Url = $"{PublicBase(request, config)}{RequestPath}/{safeFolder}/{name}",
                FileName = Path.GetFileName(file.FileName ?? name),
                ContentType = kind.Type,
                Size = file.Length
            };
        }

        public static string PublicBase(HttpRequest request, IConfiguration config)
        {
            var configured = config["JobCardPhotos:PublicBaseUrl"];
            if (!string.IsNullOrWhiteSpace(configured)) return configured.TrimEnd('/');
            var proto = request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var scheme = string.IsNullOrWhiteSpace(proto) ? request.Scheme : proto.Split(',')[0].Trim();
            return $"{scheme}://{request.Host}{request.PathBase}";
        }

        /// <summary>true for a URL this API produced (used to validate stored photo URLs)</summary>
        public static bool IsOwnPhotoUrl(string? url) =>
            !string.IsNullOrWhiteSpace(url) && url.Length <= 500 && url.Contains(RequestPath + "/", StringComparison.OrdinalIgnoreCase)
            && Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp);
    }
}
