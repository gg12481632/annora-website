using Annora.Application.Images;
using Annora.Infrastructure.Storage;
using Azure.Data.Tables;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace Annora.Infrastructure.Images;

public sealed class ImageUploadStorage : IImageUploadStorage
{
    private static readonly TimeSpan UploadLifetime =
        TimeSpan.FromMinutes(15);

    private readonly StorageOptions _options;
    private readonly TableClient _tableClient;
    private readonly BlobContainerClient _containerClient;
    private readonly StorageSharedKeyCredential _credential;

    public ImageUploadStorage(
        IOptions<StorageOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException(
                "Storage connection string is not configured.");
        }

        var connectionValues =
            ParseConnectionString(_options.ConnectionString);

        _credential = new StorageSharedKeyCredential(
            connectionValues["AccountName"],
            connectionValues["AccountKey"]);

        _tableClient = new TableClient(
            _options.ConnectionString,
            _options.ImagesTableName);

        _containerClient = new BlobContainerClient(
            _options.ConnectionString,
            _options.ImagesContainerName);
    }

    public async Task<CreateImageUploadResult> CreateUploadAsync(
        CreateImageUploadCommand command,
        CancellationToken cancellationToken = default)
    {
        var imageId = Guid.NewGuid();
        var blobName = $"originals/{imageId:N}/original";

        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.Add(UploadLifetime);

        await _tableClient.CreateIfNotExistsAsync(
            cancellationToken);

        await _containerClient.CreateIfNotExistsAsync(
            cancellationToken: cancellationToken);

        var entity = new ImageTableEntity
        {
            PartitionKey = "image",
            RowKey = imageId.ToString("N"),
            OriginalFileName = command.FileName,
            BlobName = blobName,
            ContentType = command.ContentType,
            Size = command.Size,
            Status = "Pending",
            CreatedAt = createdAt
        };

        await _tableClient.AddEntityAsync(
            entity,
            cancellationToken);

        var blobClient =
            _containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = createdAt.AddMinutes(-1),
            ExpiresOn = expiresAt,
            Protocol = SasProtocol.Https
        };

        sasBuilder.SetPermissions(
            BlobSasPermissions.Create |
            BlobSasPermissions.Write);

        var sas = sasBuilder.ToSasQueryParameters(
            _credential);

        var uploadUrl = new UriBuilder(blobClient.Uri)
        {
            Query = sas.ToString()
        }.Uri;

        return new CreateImageUploadResult(
            imageId,
            uploadUrl,
            expiresAt);
    }

    private static Dictionary<string, string>
        ParseConnectionString(string connectionString)
    {
        return connectionString
            .Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(
                parts => parts[0],
                parts => parts[1],
                StringComparer.OrdinalIgnoreCase);
    }

    public async Task CompleteUploadAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var rowKey = imageId.ToString("N");

        var imageResponse =
            await _tableClient.GetEntityIfExistsAsync<ImageTableEntity>(
                partitionKey: "image",
                rowKey: rowKey,
                cancellationToken: cancellationToken);

        if (!imageResponse.HasValue)
        {
            throw new KeyNotFoundException(
                $"Image '{imageId}' was not found.");
        }

        var image = imageResponse.Value;

        if (!string.Equals(
            image.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Image '{imageId}' is not pending.");
        }

        var blobClient =
            _containerClient.GetBlobClient(image.BlobName);

        var exists = await blobClient.ExistsAsync(
            cancellationToken);

        if (!exists.Value)
        {
            throw new InvalidOperationException(
                "The uploaded blob does not exist.");
        }

        var properties = await blobClient.GetPropertiesAsync(
            cancellationToken: cancellationToken);

        if (properties.Value.ContentLength != image.Size)
        {
            throw new InvalidOperationException(
                "The uploaded file size does not match the requested size.");
        }

        if (!string.Equals(
            properties.Value.ContentType,
            image.ContentType,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The uploaded content type does not match the requested type.");
        }

        image.Status = "Uploaded";
        image.UploadedAt = DateTimeOffset.UtcNow;

        await _tableClient.UpdateEntityAsync(
            image,
            image.ETag,
            TableUpdateMode.Replace,
            cancellationToken);
    }

    public async Task ValidateForAttachmentAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var image = await GetImageAsync(
            imageId,
            cancellationToken);

        if (!string.Equals(
            image.Status,
            "Uploaded",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Image '{imageId}' is not ready for attachment.");
        }

        if (!string.IsNullOrWhiteSpace(image.ListingId))
        {
            throw new InvalidOperationException(
                $"Image '{imageId}' is already attached to a listing.");
        }
    }

    public async Task AttachToListingAsync(
        Guid imageId,
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        var image = await GetImageAsync(
            imageId,
            cancellationToken);

        if (!string.Equals(
            image.Status,
            "Uploaded",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Image '{imageId}' is not ready for attachment.");
        }

        image.Status = "Attached";
        image.ListingId = listingId.ToString("D");
        image.AttachedAt = DateTimeOffset.UtcNow;

        await _tableClient.UpdateEntityAsync(
            image,
            image.ETag,
            TableUpdateMode.Replace,
            cancellationToken);
    }

    private async Task<ImageTableEntity> GetImageAsync(
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var response =
            await _tableClient.GetEntityIfExistsAsync<ImageTableEntity>(
                partitionKey: "image",
                rowKey: imageId.ToString("N"),
                cancellationToken: cancellationToken);

        if (!response.HasValue)
        {
            throw new KeyNotFoundException(
                $"Image '{imageId}' was not found.");
        }

        return response.Value;
    }

    public async Task<Uri?> CreateReadUrlAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _tableClient.GetEntityIfExistsAsync<ImageTableEntity>(
                partitionKey: "image",
                rowKey: imageId.ToString("N"),
                cancellationToken: cancellationToken);

        if (!response.HasValue)
        {
            return null;
        }

        var image = response.Value;

        if (!string.Equals(
                image.Status,
                "Attached",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var blobClient =
            _containerClient.GetBlobClient(image.BlobName);

        var exists = await blobClient.ExistsAsync(
            cancellationToken);

        if (!exists.Value)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = image.BlobName,
            Resource = "b",
            StartsOn = now.AddMinutes(-1),
            ExpiresOn = now.AddMinutes(10),
            Protocol = SasProtocol.Https
        };

        sasBuilder.SetPermissions(
            BlobSasPermissions.Read);

        var sas = sasBuilder.ToSasQueryParameters(
            _credential);

        return new UriBuilder(blobClient.Uri)
        {
            Query = sas.ToString()
        }.Uri;
    }

}
