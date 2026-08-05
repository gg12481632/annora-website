namespace Annora.Application.Images;

public interface IImageUploadStorage
{
    Task<CreateImageUploadResult> CreateUploadAsync(
        CreateImageUploadCommand command,
        CancellationToken cancellationToken = default);
}