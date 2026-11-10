namespace BjjSM.Modules.Curriculum.Application;

public interface IMediaStorageService
{
    Task<MediaFileDto> SaveAsync(Stream contentStream, string originalFileName, string contentType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaFileDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}
