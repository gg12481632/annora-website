using api.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetCurrentUser
{
    [Function(nameof(GetCurrentUser))]
    public IActionResult Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "users/me")]
        HttpRequest request)
    {
        var principal =
            ClientPrincipalReader.Read(request);

        if (principal is null)
        {
            return new UnauthorizedObjectResult(new
            {
                message =
                    "User is not authenticated."
            });
        }

        return new OkObjectResult(new
        {
            userId = principal.UserId,
            displayName =
                principal.UserDetails,
            identityProvider =
                principal.IdentityProvider,
            roles =
                principal.UserRoles
        });
    }
}