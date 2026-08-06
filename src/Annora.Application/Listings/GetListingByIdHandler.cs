namespace Annora.Application.Listings;

public sealed class GetListingByIdHandler
{
    private readonly IListingRepository _repository;

    public GetListingByIdHandler(
        IListingRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListingResult?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (listing is null)
        {
            return null;
        }

        return new ListingResult(
            listing.Id,
            listing.Title,
            listing.Category,
            listing.Description,
            listing.Price,
            listing.Condition,
            listing.PostalCode,
            listing.City,
            listing.PrimaryImageId,
            listing.CreatedAt);
    }
}