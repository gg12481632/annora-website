namespace Annora.Application.Images;

public sealed record CreateImageUploadResult(
    Guid ImageId,
    Uri UploadUrl,
    DateTimeOffset ExpiresAt);