using api.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetCurrentUser
{
    private readonly ICurrentUserAccessor _userAccessor;

    public GetCurrentUser(
        ICurrentUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    [Function(nameof(GetCurrentUser))]
    public IActionResult Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "users/me")]
        HttpRequest request)
    {
        var user =
            _userAccessor.GetCurrentUser(request);

        if (user is null)
        {
            return new UnauthorizedObjectResult(new
            {
                message = "User is not authenticated."
            });
        }

        return new OkObjectResult(new
        {
            userId = user.UserId,
            displayName = user.DisplayName,
            identityProvider = user.IdentityProvider,
            roles = user.Roles
        });
    }
}
