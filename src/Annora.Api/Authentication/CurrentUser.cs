namespace api.Authentication;

public sealed record CurrentUser(
    string UserId,
    string DisplayName,
    string IdentityProvider,
    IReadOnlyCollection<string> Roles);