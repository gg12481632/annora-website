namespace Annora.Application.Listings;

public sealed record CreateListingCommand(
    string Title,
    string Category,
    string Description,
    decimal Price,
    string Condition,
    string PostalCode,
    string City,
    string SellerEmail,
    Guid? PrimaryImageId,
    string OwnerId,
    string OwnerName);