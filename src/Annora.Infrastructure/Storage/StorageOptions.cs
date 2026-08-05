namespace Annora.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string ConnectionString { get; set; } = string.Empty;

    public string ListingsTableName { get; set; } = "Listings";
}