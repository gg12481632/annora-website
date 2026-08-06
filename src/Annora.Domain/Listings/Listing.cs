namespace Annora.Domain.Listings;

public sealed class Listing
{
    public Guid Id { get; }
    public string Title { get; }
    public string Category { get; }
    public string Description { get; }
    public decimal Price { get; }
    public string Condition { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string SellerEmail { get; }
    public Guid? PrimaryImageId { get; }    
    public DateTimeOffset CreatedAt { get; }

    private Listing(
        Guid id,
        string title,
        string category,
        string description,
        decimal price,
        string condition,
        string postalCode,
        string city,
        string sellerEmail,
        Guid? primaryImageId,        
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Category = category;
        Description = description;
        Price = price;
        Condition = condition;
        PostalCode = postalCode;
        City = city;
        SellerEmail = sellerEmail;
        PrimaryImageId = primaryImageId;
        CreatedAt = createdAt;
    }

    public static Listing Create(
        string title,
        string category,
        string description,
        decimal price,
        string condition,
        string postalCode,
        string city,
        string sellerEmail,
        Guid? primaryImageId)
    {
        return new Listing(
            Guid.NewGuid(),
            title.Trim(),
            category.Trim(),
            description.Trim(),
            price,
            condition.Trim(),
            postalCode,
            city.Trim(),
            sellerEmail.Trim(),
            primaryImageId,
            DateTimeOffset.UtcNow);
    }

    public static Listing Restore(
        Guid id,
        string title,
        string category,
        string description,
        decimal price,
        string condition,
        string postalCode,
        string city,
        string sellerEmail,
        Guid? primaryImageId,
        DateTimeOffset createdAt)
    {
        return new Listing(
            id,
            title,
            category,
            description,
            price,
            condition,
            postalCode,
            city,
            sellerEmail,
            primaryImageId,
            createdAt);
    }

}