namespace Annora.Application.Listings;

public sealed class GetMyListingsHandler
{
    private readonly IListingRepository _repository;

    public GetMyListingsHandler(
        IListingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ListingResult>>
        HandleAsync(
            string ownerId,
            CancellationToken cancellationToken = default)
    {
        var listings =
            await _repository.GetByOwnerIdAsync(
                ownerId,
                cancellationToken);

        return listings
            .OrderByDescending(listing => listing.CreatedAt)
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