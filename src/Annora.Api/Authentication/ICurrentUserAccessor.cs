using Microsoft.AspNetCore.Http;

namespace api.Authentication;

public interface ICurrentUserAccessor
{
    CurrentUser? GetCurrentUser(HttpRequest request);

    CurrentUser GetRequiredUser(HttpRequest request);
}