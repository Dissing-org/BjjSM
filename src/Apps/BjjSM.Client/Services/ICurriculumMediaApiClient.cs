using Microsoft.AspNetCore.Components.Forms;

namespace BjjSM.Client.Services;

public sealed record MediaFileDto(
    string FileName,
    string RelativeUrl,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAt);

public interface ICurriculumMediaApiClient
{
    Task<MediaFileDto?> UploadMediaAsync(IBrowserFile file, CancellationToken ct = default);
    Task<IReadOnlyList<MediaFileDto>> GetMediaFilesAsync(CancellationToken ct = default);
}
