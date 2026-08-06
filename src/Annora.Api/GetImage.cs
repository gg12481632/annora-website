using Annora.Application.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace api;

public sealed class GetImage
{
    private readonly GetImageUrlHandler _handler;

    public GetImage(GetImageUrlHandler handler)
    {
        _handler = handler;
    }

    [Function(nameof(GetImage))]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "images/{id}")]
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

        var readUrl = await _handler.HandleAsync(
            imageId,
            cancellationToken);

        if (readUrl is null)
        {
            return new NotFoundResult();
        }

        return new RedirectResult(
            readUrl.ToString(),
            permanent: false);
    }
}
