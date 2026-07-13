using Chroma.Application.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[ApiController]
[Route("api/webhooks/{provider}")]
public class WebhooksController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public Task<IActionResult> InboundAsync(string provider, CancellationToken cancellationToken)
    {
        // Skeleton: provider-specific webhook handling will be implemented via integration adapters.
        _ = cancellationToken;
        return Task.FromResult<IActionResult>(Ok(ApiResponse.Ok($"Webhook received for provider '{provider}'.")));
    }
}
