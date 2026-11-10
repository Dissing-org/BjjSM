namespace BjjSM.Modules.Curriculum.Application;

public sealed record MediaFileDto(
    string FileName,
    string RelativeUrl,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAt);
