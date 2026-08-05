using Annora.Application.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetListingById
{
    private readonly GetListingByIdHandler _handler;

    public GetListingById(
        GetListingByIdHandler handler)
    {
        _handler = handler;
    }

    [Function(nameof(GetListingById))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "listings/{id}")]
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

        var listing = await _handler.HandleAsync(
            listingId,
            cancellationToken);

        if (listing is null)
        {
            return new NotFoundObjectResult(new
            {
                message = $"Listing '{listingId}' was not found."
            });
        }

        return new OkObjectResult(listing);
    }
}