namespace Annora.Application.Listings;

public sealed class GetListingsHandler
{
    private readonly IListingRepository _repository;

    public GetListingsHandler(IListingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ListingResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var listings = await _repository.GetAllAsync(
            cancellationToken);

        return listings
            .Select(listing => new ListingResult(
                listing.Id,
                listing.Title,
                listing.Category,
                listing.Description,
                listing.Price,
                listing.Condition,
                listing.PostalCode,
                listing.City,
                listing.PrimaryImageId,
                listing.CreatedAt))
            .ToArray();
    }
}

public sealed record ListingResult(
    Guid Id,
    string Title,
    string Category,
    string Description,
    decimal Price,
    string Condition,
    string PostalCode,
    string City,
    Guid? PrimaryImageId,
    DateTimeOffset CreatedAt);