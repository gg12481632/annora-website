using Annora.Domain.Listings;
using Annora.Application.Images;

namespace Annora.Application.Listings;

public sealed class CreateListingHandler
{
private readonly IListingRepository _repository;
private readonly IImageUploadStorage _imageStorage;

public CreateListingHandler(
    IListingRepository repository,
    IImageUploadStorage imageStorage)
{
    _repository = repository;
    _imageStorage = imageStorage;
}

public async Task<CreateListingResult> HandleAsync(
    CreateListingCommand command,
    CancellationToken cancellationToken = default)
{
    if (command.PrimaryImageId.HasValue)
    {
        await _imageStorage.ValidateForAttachmentAsync(
            command.PrimaryImageId.Value,
            cancellationToken);
    }

    var listing = Listing.Create(
        command.Title,
        command.Category,
        command.Description,
        command.Price,
        command.Condition,
        command.PostalCode,
        command.City,
        command.SellerEmail,
        command.PrimaryImageId);

    await _repository.AddAsync(
        listing,
        cancellationToken);

    if (command.PrimaryImageId.HasValue)
    {
        await _imageStorage.AttachToListingAsync(
            command.PrimaryImageId.Value,
            listing.Id,
            cancellationToken);
    }

    return new CreateListingResult(
        listing.Id,
        listing.Title,
        listing.PrimaryImageId,
        listing.CreatedAt);
}
}

public sealed record CreateListingResult(
    Guid Id,
    string Title,
    Guid? PrimaryImageId,
    DateTimeOffset CreatedAt);