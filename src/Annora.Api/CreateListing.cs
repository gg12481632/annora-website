using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace api;

public sealed class CreateListing
{
    private readonly ILogger<CreateListing> _logger;

    public CreateListing(ILogger<CreateListing> logger)
    {
        _logger = logger;
    }

    [Function("CreateListing")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "listings")]
        HttpRequest request)
    {
        CreateListingRequest? listing;

        try
        {
            listing = await request.ReadFromJsonAsync<CreateListingRequest>();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "The request body could not be parsed.");

            return new BadRequestObjectResult(new
            {
                message = "Request body must contain valid JSON."
            });
        }

        if (listing is null)
        {
            return new BadRequestObjectResult(new
            {
                message = "Request body is required."
            });
        }

        var validationErrors = Validate(listing);

        if (validationErrors.Count > 0)
        {
            return new BadRequestObjectResult(new
            {
                message = "The listing is not valid.",
                errors = validationErrors
            });
        }

        var result = new ListingResponse(
            Id: Guid.NewGuid(),
            Title: listing.Title!.Trim(),
            Status: "Created",
            CreatedAt: DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Created listing {ListingId} with title {ListingTitle}.",
            result.Id,
            result.Title);

        return new ObjectResult(result)
        {
            StatusCode = StatusCodes.Status201Created
        };
    }

    private static Dictionary<string, string[]> Validate(
        CreateListingRequest listing)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(listing.Title) ||
            listing.Title.Trim().Length < 5)
        {
            errors["title"] =
            [
                "Titlen skal indeholde mindst 5 tegn."
            ];
        }

        if (string.IsNullOrWhiteSpace(listing.Description) ||
            listing.Description.Trim().Length < 20)
        {
            errors["description"] =
            [
                "Beskrivelsen skal indeholde mindst 20 tegn."
            ];
        }

        if (string.IsNullOrWhiteSpace(listing.Category))
        {
            errors["category"] =
            [
                "Der skal vælges en kategori."
            ];
        }

        if (listing.Price < 0)
        {
            errors["price"] =
            [
                "Prisen må ikke være negativ."
            ];
        }

        if (listing.Location is null ||
            string.IsNullOrWhiteSpace(listing.Location.PostalCode) ||
            listing.Location.PostalCode.Length != 4 ||
            !listing.Location.PostalCode.All(char.IsDigit))
        {
            errors["postalCode"] =
            [
                "Postnummeret skal bestå af fire cifre."
            ];
        }

        if (listing.Location is null ||
            string.IsNullOrWhiteSpace(listing.Location.City))
        {
            errors["city"] =
            [
                "By skal udfyldes."
            ];
        }

        if (listing.Seller is null ||
            string.IsNullOrWhiteSpace(listing.Seller.Email) ||
            !new EmailAddressAttribute().IsValid(listing.Seller.Email))
        {
            errors["email"] =
            [
                "Der skal angives en gyldig e-mailadresse."
            ];
        }

        return errors;
    }
}

public sealed class CreateListingRequest
{
    public string? Title { get; init; }

    public string? Category { get; init; }

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public string? Condition { get; init; }

    public ListingLocation? Location { get; init; }

    public ListingSeller? Seller { get; init; }

    public IReadOnlyCollection<ListingImage>? Images { get; init; }
}

public sealed class ListingLocation
{
    public string? PostalCode { get; init; }

    public string? City { get; init; }
}

public sealed class ListingSeller
{
    public string? Email { get; init; }
}

public sealed class ListingImage
{
    public string? Name { get; init; }

    public long Size { get; init; }

    public string? Type { get; init; }
}

public sealed record ListingResponse(
    Guid Id,
    string Title,
    string Status,
    DateTimeOffset CreatedAt);