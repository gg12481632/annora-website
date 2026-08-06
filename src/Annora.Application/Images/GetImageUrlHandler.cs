namespace Annora.Application.Images;

public sealed class GetImageUrlHandler
{
    private readonly IImageUploadStorage _storage;

    public GetImageUrlHandler(
        IImageUploadStorage storage)
    {
        _storage = storage;
    }

    public Task<Uri?> HandleAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        return _storage.CreateReadUrlAsync(
            imageId,
            cancellationToken);
    }
}
