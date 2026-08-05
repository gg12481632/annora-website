using Azure;
using Azure.Data.Tables;

namespace Annora.Infrastructure.Images;

internal sealed class ImageTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "image";

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string BlobName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UploadedAt { get; set; }

    public DateTimeOffset? AttachedAt { get; set; }

    public string? ListingId { get; set; }
}