namespace Annora.Application.Images;

public sealed class CreateImageUploadHandler
{
    private const long MaximumFileSize =
        5 * 1024 * 1024;

    private static readonly HashSet<string>
        AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    private readonly IImageUploadStorage _storage;

    public CreateImageUploadHandler(
        IImageUploadStorage storage)
    {
        _storage = storage;
    }

    public Task<CreateImageUploadResult> HandleAsync(
        CreateImageUploadCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            throw new ArgumentException(
                "File name is required.");
        }

        if (!AllowedContentTypes.Contains(
            command.ContentType))
        {
            throw new ArgumentException(
                "Only JPEG, PNG and WebP are supported.");
        }

        if (command.Size <= 0)
        {
            throw new ArgumentException(
                "File size must be greater than zero.");
        }

        if (command.Size > MaximumFileSize)
        {
            throw new ArgumentException(
                "Image must not exceed 5 MB.");
        }

        return _storage.CreateUploadAsync(
            command,
            cancellationToken);
    }
}