namespace Annora.Application.Images;

public sealed record CreateImageUploadCommand(
    string FileName,
    string ContentType,
    long Size);