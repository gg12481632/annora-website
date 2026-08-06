using Microsoft.AspNetCore.Http;

namespace api.Authentication;

public sealed class CurrentUserAccessor :
    ICurrentUserAccessor
{
    public CurrentUser? GetCurrentUser(
        HttpRequest request)
    {
        var principal =
            ClientPrincipalReader.Read(request);

        if (principal is null ||
            string.IsNullOrWhiteSpace(principal.UserId))
        {
            return null;
        }

        return new CurrentUser(
            UserId: principal.UserId,
            DisplayName: principal.UserDetails,
            IdentityProvider: principal.IdentityProvider,
            Roles: principal.UserRoles);
    }

    public CurrentUser GetRequiredUser(
        HttpRequest request)
    {
        return GetCurrentUser(request)
            ?? throw new UnauthorizedAccessException(
                "User is not authenticated.");
    }
}