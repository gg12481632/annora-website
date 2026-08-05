namespace Annora.Application.Listings;

public interface IListingImageStorage
{
    Task UploadPrimaryImageAsync(
        Guid listingId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<ListingImage?> GetPrimaryImageAsync(
        Guid listingId,
        CancellationToken cancellationToken = default);
}

public sealed record ListingImage(
    Stream Content,
    string ContentType);