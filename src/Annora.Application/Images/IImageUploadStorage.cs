namespace Annora.Application.Images;

public interface IImageUploadStorage
{
    Task<CreateImageUploadResult> CreateUploadAsync(
        CreateImageUploadCommand command,
        CancellationToken cancellationToken = default);

    Task CompleteUploadAsync(
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task ValidateForAttachmentAsync(
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task AttachToListingAsync(
        Guid imageId,
        Guid listingId,
        CancellationToken cancellationToken = default);
}