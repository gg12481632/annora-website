using Annora.Application.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace api;

public sealed class UploadListingImage
{
    private const long MaximumFileSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    private readonly IListingRepository _listingRepository;
    private readonly IListingImageStorage _imageStorage;
    private readonly ILogger<UploadListingImage> _logger;

    public UploadListingImage(
        IListingRepository listingRepository,
        IListingImageStorage imageStorage,
        ILogger<UploadListingImage> logger)
    {
        _listingRepository = listingRepository;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    [Function(nameof(UploadListingImage))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "listings/{id}/image")]
        HttpRequest request,
        string id,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var listingId))
        {
            return new BadRequestObjectResult(new
            {
                message = "Listing id must be a valid GUID."
            });
        }

        var listing = await _listingRepository.GetByIdAsync(
            listingId,
            cancellationToken);

        if (listing is null)
        {
            return new NotFoundObjectResult(new
            {
                message = $"Listing '{listingId}' was not found."
            });
        }

        if (!request.HasFormContentType)
        {
            return new BadRequestObjectResult(new
            {
                message = "Request must use multipart/form-data."
            });
        }

        var form = await request.ReadFormAsync(
            cancellationToken);

        var image = form.Files.GetFile("image");

        if (image is null || image.Length == 0)
        {
            return new BadRequestObjectResult(new
            {
                message = "An image file is required."
            });
        }

        if (image.Length > MaximumFileSize)
        {
            return new BadRequestObjectResult(new
            {
                message = "Image must not exceed 5 MB."
            });
        }

        if (!AllowedContentTypes.Contains(image.ContentType))
        {
            return new BadRequestObjectResult(new
            {
                message = "Only JPEG, PNG and WebP are supported."
            });
        }

        await using var content = image.OpenReadStream();

        await _imageStorage.UploadPrimaryImageAsync(
            listingId,
            content,
            image.ContentType,
            cancellationToken);

        _logger.LogInformation(
            "Uploaded primary image for listing {ListingId}.",
            listingId);

        return new OkObjectResult(new
        {
            listingId,
            imageUrl = $"/api/listings/{listingId}/image"
        });
    }
}