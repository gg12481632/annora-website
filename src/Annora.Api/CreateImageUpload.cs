using Annora.Application.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace api;

public sealed class CreateImageUpload
{
    private readonly CreateImageUploadHandler _handler;
    private readonly ILogger<CreateImageUpload> _logger;

    public CreateImageUpload(
        CreateImageUploadHandler handler,
        ILogger<CreateImageUpload> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function(nameof(CreateImageUpload))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "images/uploads")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        CreateImageUploadRequest? body;

        try
        {
            body =
                await request.ReadFromJsonAsync<
                    CreateImageUploadRequest>(
                    cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not parse image upload request.");

            return new BadRequestObjectResult(new
            {
                message =
                    "Request body must contain valid JSON."
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
            var result = await _handler.HandleAsync(
                new CreateImageUploadCommand(
                    body.FileName ?? string.Empty,
                    body.ContentType ?? string.Empty,
                    body.Size),
                cancellationToken);

            return new OkObjectResult(new
            {
                imageId = result.ImageId,
                uploadUrl = result.UploadUrl,
                expiresAt = result.ExpiresAt
            });
        }
        catch (ArgumentException exception)
        {
            return new BadRequestObjectResult(new
            {
                message = exception.Message
            });
        }
    }
}

public sealed class CreateImageUploadRequest
{
    public string? FileName { get; init; }

    public string? ContentType { get; init; }

    public long Size { get; init; }
}