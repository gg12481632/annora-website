using Annora.Application.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace api;

public sealed class CreateListing
{
    private readonly CreateListingHandler _handler;
    private readonly ILogger<CreateListing> _logger;

    public CreateListing(
        CreateListingHandler handler,
        ILogger<CreateListing> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function(nameof(CreateListing))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "listings")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        CreateListingRequest? body;

        try
        {
            body = await request.ReadFromJsonAsync<CreateListingRequest>(
                cancellationToken);
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

        if (body is null)
        {
            return new BadRequestObjectResult(new
            {
                message = "Request body is required."
            });
        }

        try
        {
            var command = new CreateListingCommand(
                body.Title ?? string.Empty,
                body.Category ?? string.Empty,
                body.Description ?? string.Empty,
                body.Price,
                body.Condition ?? string.Empty,
                body.Location?.PostalCode ?? string.Empty,
                body.Location?.City ?? string.Empty,
                body.Seller?.Email ?? string.Empty);

            var result = await _handler.HandleAsync(
                command,
                cancellationToken);

            return new ObjectResult(new
            {
                id = result.Id,
                title = result.Title,
                status = "Created",
                createdAt = result.CreatedAt
            })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(
                exception,
                "Listing validation failed.");

            return new BadRequestObjectResult(new
            {
                message = exception.Message
            });
        }
    }
}

public sealed class CreateListingRequest
{
    public string? Title { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? Condition { get; init; }
    public ListingLocationRequest? Location { get; init; }
    public ListingSellerRequest? Seller { get; init; }
}

public sealed class ListingLocationRequest
{
    public string? PostalCode { get; init; }
    public string? City { get; init; }
}

public sealed class ListingSellerRequest
{
    public string? Email { get; init; }
}