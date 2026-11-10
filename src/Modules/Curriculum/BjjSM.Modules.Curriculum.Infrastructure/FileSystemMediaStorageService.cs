using BjjSM.Modules.Curriculum.Application;

namespace BjjSM.Modules.Curriculum.Infrastructure;

public sealed class FileSystemMediaStorageService : IMediaStorageService
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".gif", ".png", ".jpg", ".jpeg"
    };

    private readonly string _storageDirectory;

    public FileSystemMediaStorageService(string? customBaseDirectory = null)
    {
        var baseDir = !string.IsNullOrWhiteSpace(customBaseDirectory)
            ? customBaseDirectory
            : AppContext.BaseDirectory;

        _storageDirectory = Path.Combine(baseDir, "data", "media", "techniques");
    }

    public async Task<MediaFileDto> SaveAsync(Stream contentStream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentStream);
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name must be specified.", nameof(originalFileName));

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"File extension '{ext}' is not permitted. Allowed: {string.Join(", ", AllowedExtensions)}");
        }

        if (contentStream.CanSeek && contentStream.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File exceeds maximum allowed size of 100MB.");
        }

        Directory.CreateDirectory(_storageDirectory);

        var sanitizedBase = Path.GetFileNameWithoutExtension(originalFileName)
            .Replace(" ", "-")
            .Replace("..", "");
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            sanitizedBase = sanitizedBase.Replace(c, '_');
        }

        var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{sanitizedBase}{ext.ToLowerInvariant()}";
        var filePath = Path.Combine(_storageDirectory, uniqueFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxFileSizeBytes)
        {
            File.Delete(filePath);
            throw new InvalidOperationException("File exceeds maximum allowed size of 100MB.");
        }

        var relativeUrl = $"/media/techniques/{uniqueFileName}";
        return new MediaFileDto(
            uniqueFileName,
            relativeUrl,
            contentType,
            fileInfo.Length,
            DateTimeOffset.UtcNow);
    }

    public Task<IReadOnlyList<MediaFileDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_storageDirectory))
        {
            return Task.FromResult<IReadOnlyList<MediaFileDto>>([]);
        }

        var dirInfo = new DirectoryInfo(_storageDirectory);
        var files = dirInfo.EnumerateFiles()
            .Where(f => AllowedExtensions.Contains(f.Extension))
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new MediaFileDto(
                f.Name,
                $"/media/techniques/{f.Name}",
                ResolveContentType(f.Extension),
                f.Length,
                f.CreationTimeUtc))
            .ToList();

        return Task.FromResult<IReadOnlyList<MediaFileDto>>(files);
    }

    public Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return Task.FromResult(false);

        var safeName = Path.GetFileName(fileName);
        var path = Path.Combine(_storageDirectory, safeName);
        if (File.Exists(path))
        {
            File.Delete(path);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private static string ResolveContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".mp4" => "video/mp4",
        ".webm" => "video/webm",
        ".gif" => "image/gif",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream"
    };
}
