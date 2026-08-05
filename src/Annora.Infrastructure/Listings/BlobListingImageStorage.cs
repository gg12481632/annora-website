using Annora.Application.Listings;
using Annora.Infrastructure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Annora.Infrastructure.Listings;

public sealed class BlobListingImageStorage : IListingImageStorage
{
    private readonly BlobContainerClient _containerClient;

    public BlobListingImageStorage(
        IOptions<StorageOptions> options)
    {
        var storageOptions = options.Value;

        if (string.IsNullOrWhiteSpace(
            storageOptions.ConnectionString))
        {
            throw new InvalidOperationException(
                "Storage connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
            storageOptions.ImagesContainerName))
        {
            throw new InvalidOperationException(
                "Images container name is not configured.");
        }

        _containerClient = new BlobContainerClient(
            storageOptions.ConnectionString,
            storageOptions.ImagesContainerName);
    }

    public async Task UploadPrimaryImageAsync(
        Guid listingId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: cancellationToken);

        var blobClient = _containerClient.GetBlobClient(
            GetBlobName(listingId));

        await blobClient.DeleteIfExistsAsync(
            cancellationToken: cancellationToken);

        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            cancellationToken);
    }

    public async Task<ListingImage?> GetPrimaryImageAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(
            GetBlobName(listingId));

        var exists = await blobClient.ExistsAsync(
            cancellationToken);

        if (!exists.Value)
        {
            return null;
        }

        var response = await blobClient.DownloadStreamingAsync(
            cancellationToken: cancellationToken);

        var contentType =
            response.Value.Details.ContentType
            ?? "application/octet-stream";

        return new ListingImage(
            response.Value.Content,
            contentType);
    }

    private static string GetBlobName(Guid listingId)
    {
        return $"listings/{listingId:N}/primary";
    }
}