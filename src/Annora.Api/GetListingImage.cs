using Annora.Application.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetListingImage
{
    private readonly IListingImageStorage _imageStorage;

    public GetListingImage(
        IListingImageStorage imageStorage)
    {
        _imageStorage = imageStorage;
    }

    [Function(nameof(GetListingImage))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
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

        var image = await _imageStorage.GetPrimaryImageAsync(
            listingId,
            cancellationToken);

        if (image is null)
        {
            return new NotFoundResult();
        }

        return new FileStreamResult(
            image.Content,
            image.ContentType);
    }
}