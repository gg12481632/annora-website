using Annora.Application.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace api;

public sealed class CompleteImageUpload
{
    private readonly CompleteImageUploadHandler _handler;
    private readonly ILogger<CompleteImageUpload> _logger;

    public CompleteImageUpload(
        CompleteImageUploadHandler handler,
        ILogger<CompleteImageUpload> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function(nameof(CompleteImageUpload))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "images/{id}/complete")]
        HttpRequest request,
        string id,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var imageId))
        {
            return new BadRequestObjectResult(new
            {
                message = "Image id must be a valid GUID."
            });
        }

        try
        {
            await _handler.HandleAsync(
                imageId,
                cancellationToken);

            return new OkObjectResult(new
            {
                imageId,
                status = "Uploaded"
            });
        }
        catch (KeyNotFoundException exception)
        {
            return new NotFoundObjectResult(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(
                exception,
                "Could not complete upload {ImageId}.",
                imageId);

            return new BadRequestObjectResult(new
            {
                message = exception.Message
            });
        }
    }
}