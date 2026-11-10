using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace BjjSM.Api.Endpoints.Curriculum;

public sealed class UploadMediaRequest
{
    public IFormFile File { get; set; } = default!;
}

public sealed class UploadMediaEndpoint : Endpoint<UploadMediaRequest, MediaFileDto>
{
    private readonly IMediaStorageService _storageService;

    public UploadMediaEndpoint(IMediaStorageService storageService)
    {
        _storageService = storageService;
    }

    public override void Configure()
    {
        Post("/api/curriculum/media/upload");
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadMediaRequest req, CancellationToken ct)
    {
        if (req.File is null || req.File.Length == 0)
        {
            AddError("A valid media file must be attached.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        try
        {
            await using var stream = req.File.OpenReadStream();
            var dto = await _storageService.SaveAsync(stream, req.File.FileName, req.File.ContentType, ct);
            await Send.CreatedAtAsync(dto.RelativeUrl, dto, cancellation: ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(400, ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(400, ct);
        }
    }
}

public sealed class ListMediaEndpoint : EndpointWithoutRequest<IReadOnlyList<MediaFileDto>>
{
    private readonly IMediaStorageService _storageService;

    public ListMediaEndpoint(IMediaStorageService storageService)
    {
        _storageService = storageService;
    }

    public override void Configure()
    {
        Get("/api/curriculum/media");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var list = await _storageService.ListAsync(ct);
        await Send.OkAsync(list, ct);
    }
}
