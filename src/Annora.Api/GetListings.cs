using Annora.Application.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetListings
{
    private readonly GetListingsHandler _handler;

    public GetListings(GetListingsHandler handler)
    {
        _handler = handler;
    }

    [Function(nameof(GetListings))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "listings")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var listings = await _handler.HandleAsync(
            cancellationToken);

        return new OkObjectResult(listings);
    }
}