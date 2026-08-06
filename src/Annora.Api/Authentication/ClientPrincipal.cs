namespace api.Authentication;

public sealed class ClientPrincipal
{
    public string IdentityProvider { get; init; } = string.Empty;

    public string UserId { get; init; } = string.Empty;

    public string UserDetails { get; init; } = string.Empty;

    public IReadOnlyCollection<string> UserRoles { get; init; } =
        Array.Empty<string>();
}