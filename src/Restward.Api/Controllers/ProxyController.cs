using Microsoft.AspNetCore.Mvc;
using Restward.Api.Models.Dtos;
using Restward.Api.Services;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api/proxy")]
public class ProxyController : ControllerBase
{
    private readonly ProxyService _proxyService;

    public ProxyController(ProxyService proxyService)
    {
        _proxyService = proxyService;
    }

    [HttpPost]
    public async Task<ActionResult<ProxyResponseDto>> Execute([FromBody] ProxyRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { error = "URL is required" });

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
            return BadRequest(new { error = "URL must be an absolute HTTP or HTTPS URL" });

        var response = await _proxyService.ExecuteAsync(request);
        return Ok(response);
    }
}
