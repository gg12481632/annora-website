using Annora.Domain.Listings;
using Azure;
using Azure.Data.Tables;

namespace Annora.Infrastructure.Listings;

internal sealed class ListingTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "listing";

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double Price { get; set; }

    public string Condition { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string SellerEmail { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public static ListingTableEntity FromDomain(Listing listing)
    {
        return new ListingTableEntity
        {
            PartitionKey = "listing",
            RowKey = listing.Id.ToString("N"),
            Title = listing.Title,
            Category = listing.Category,
            Description = listing.Description,
            Price = Convert.ToDouble(listing.Price),
            Condition = listing.Condition,
            PostalCode = listing.PostalCode,
            City = listing.City,
            SellerEmail = listing.SellerEmail,
            CreatedAt = listing.CreatedAt
        };
    }
}