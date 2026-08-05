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
}