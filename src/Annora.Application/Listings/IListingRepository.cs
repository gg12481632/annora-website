using Annora.Domain.Listings;

namespace Annora.Application.Listings;

public interface IListingRepository
{
    Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Listing>> GetAllAsync(
        CancellationToken cancellationToken = default);
}