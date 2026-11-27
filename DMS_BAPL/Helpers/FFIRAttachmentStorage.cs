using Microsoft.AspNetCore.Http;
using System.Text;

namespace DMS_BAPL_Api.Helpers;

public static class FFIRAttachmentStorage
{
    public const long MaxImageBytes = 10 * 1024 * 1024;
    public const long MaxVideoBytes = 100 * 1024 * 1024;

    public sealed record MediaInfo(string MediaType, string ContentType, string Extension);

    public sealed record Saved(string Url, string FileName, string ContentType, string MediaType, long Size, string Path);

    public static async Task<MediaInfo> ValidateAsync(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("The selected file is empty.");

        var header = new byte[16];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(), ct);
        var media = Detect(header, read);
        if (media == null)
            throw new InvalidOperationException($"{file.FileName}: only JPG, PNG, GIF, WEBP, MP4, or MOV files are supported.");

        var maxBytes = media.MediaType == "Image" ? MaxImageBytes : MaxVideoBytes;
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"{file.FileName}: exceeds the {maxBytes / (1024 * 1024)} MB file limit.");

        return media;
    }

    public static async Task<Saved> SaveAsync(
        IFormFile file,
        MediaInfo media,
        int ffirId,
        HttpRequest request,
        IConfiguration config,
        string contentRootPath,
        CancellationToken ct)
    {
        var folder = $"ffir-{ffirId}";
        var directory = Path.Combine(JobCardPhotoStorage.Root(contentRootPath), folder);
        Directory.CreateDirectory(directory);

        var name = $"{Guid.NewGuid():N}{media.Extension}";
        var path = Path.Combine(directory, name);
        await using (var output = File.Create(path))
        await using (var input = file.OpenReadStream())
            await input.CopyToAsync(output, ct);

        return new Saved(
            $"{JobCardPhotoStorage.PublicBase(request, config)}{JobCardPhotoStorage.RequestPath}/{folder}/{name}",
            Path.GetFileName(file.FileName),
            media.ContentType,
            media.MediaType,
            file.Length,
            path);
    }

    private static MediaInfo? Detect(byte[] header, int length)
    {
        if (length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return new MediaInfo("Image", "image/jpeg", ".jpg");
        if (length >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            return new MediaInfo("Image", "image/png", ".png");
        if (length >= 6 && Encoding.ASCII.GetString(header, 0, 3) == "GIF")
            return new MediaInfo("Image", "image/gif", ".gif");
        if (length >= 12
            && Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
            && Encoding.ASCII.GetString(header, 8, 4) == "WEBP")
            return new MediaInfo("Image", "image/webp", ".webp");

        if (length >= 12 && Encoding.ASCII.GetString(header, 4, 4) == "ftyp")
        {
            var brand = Encoding.ASCII.GetString(header, 8, 4);
            if (brand == "qt  ")
                return new MediaInfo("Video", "video/quicktime", ".mov");
            if (brand is "isom" or "iso2" or "mp41" or "mp42" or "M4V " or "avc1" or "dash" or "MSNV")
                return new MediaInfo("Video", "video/mp4", ".mp4");
        }

        return null;
    }
}
