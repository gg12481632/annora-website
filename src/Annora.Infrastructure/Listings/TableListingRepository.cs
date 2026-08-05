using Annora.Application.Listings;
using Annora.Domain.Listings;
using Annora.Infrastructure.Storage;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace Annora.Infrastructure.Listings;

public sealed class TableListingRepository : IListingRepository
{
    private readonly TableClient _tableClient;

    public TableListingRepository(
        IOptions<StorageOptions> options)
    {
        var storageOptions = options.Value;

        if (string.IsNullOrWhiteSpace(
            storageOptions.ConnectionString))
        {
            throw new InvalidOperationException(
                "Storage:ConnectionString is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
            storageOptions.ListingsTableName))
        {
            throw new InvalidOperationException(
                "Storage:ListingsTableName is not configured.");
        }

        _tableClient = new TableClient(
            storageOptions.ConnectionString,
            storageOptions.ListingsTableName);
    }

    public async Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(listing);

        var entity = ListingTableEntity.FromDomain(listing);

        await _tableClient.CreateIfNotExistsAsync(
            cancellationToken);

        await _tableClient.AddEntityAsync(
            entity,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Listing>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await _tableClient.CreateIfNotExistsAsync(
            cancellationToken);

        var listings = new List<Listing>();

        await foreach (
            var entity in _tableClient.QueryAsync<ListingTableEntity>(
                entity => entity.PartitionKey == "listing",
                cancellationToken: cancellationToken))
        {
            listings.Add(entity.ToDomain());
        }

        return listings
            .OrderByDescending(listing => listing.CreatedAt)
            .ToArray();
    }

    public async Task<Listing?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _tableClient.CreateIfNotExistsAsync(
            cancellationToken);

        var rowKey = id.ToString("N");

        var response =
            await _tableClient.GetEntityIfExistsAsync<ListingTableEntity>(
                partitionKey: "listing",
                rowKey: rowKey,
                cancellationToken: cancellationToken);

        if (!response.HasValue)
        {
            return null;
        }

        return response.Value.ToDomain();
    }
}