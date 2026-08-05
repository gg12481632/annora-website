using Annora.Domain.Listings;

namespace Annora.Application.Listings;

public sealed class CreateListingHandler
{
    private readonly IListingRepository _repository;

    public CreateListingHandler(IListingRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateListingResult> HandleAsync(
        CreateListingCommand command,
        CancellationToken cancellationToken = default)
    {
        var listing = Listing.Create(
            command.Title,
            command.Category,
            command.Description,
            command.Price,
            command.Condition,
            command.PostalCode,
            command.City,
            command.SellerEmail);

        await _repository.AddAsync(
            listing,
            cancellationToken);

        return new CreateListingResult(
            listing.Id,
            listing.Title,
            listing.CreatedAt);
    }
}

public sealed record CreateListingResult(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt);