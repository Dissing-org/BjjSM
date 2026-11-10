using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace BjjSM.Client.Services;

public sealed class CurriculumMediaApiClient : ICurriculumMediaApiClient
{
    private readonly HttpClient _http;

    public CurriculumMediaApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<MediaFileDto?> UploadMediaAsync(IBrowserFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        // Allow up to 100MB stream
        var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 104857600));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _http.PostAsync("/api/curriculum/media/upload", content, ct);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<MediaFileDto>(cancellationToken: ct);
    }

    public async Task<IReadOnlyList<MediaFileDto>> GetMediaFilesAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<MediaFileDto>>("/api/curriculum/media", ct);
        return result ?? [];
    }
}
