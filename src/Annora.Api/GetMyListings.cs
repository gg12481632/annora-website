using Annora.Application.Listings;
using api.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetMyListings
{
    private readonly GetMyListingsHandler _handler;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetMyListings(
        GetMyListingsHandler handler,
        ICurrentUserAccessor currentUserAccessor)
    {
        _handler = handler;
        _currentUserAccessor = currentUserAccessor;
    }

    [Function(nameof(GetMyListings))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "my/listings")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser =
            _currentUserAccessor.GetCurrentUser(request);

        if (currentUser is null)
        {
            return new UnauthorizedObjectResult(new
            {
                message = "User is not authenticated."
            });
        }

        var listings =
            await _handler.HandleAsync(
                currentUser.UserId,
                cancellationToken);

        return new OkObjectResult(listings);
    }
}