namespace Annora.Application.Images;

public sealed class CompleteImageUploadHandler
{
    private readonly IImageUploadStorage _storage;

    public CompleteImageUploadHandler(
        IImageUploadStorage storage)
    {
        _storage = storage;
    }

    public Task HandleAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        return _storage.CompleteUploadAsync(
            imageId,
            cancellationToken);
    }
}
