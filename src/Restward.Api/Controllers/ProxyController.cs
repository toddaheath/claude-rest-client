using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Restward.Api.Models.Dtos;
using Restward.Api.Services;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api/proxy")]
[EnableRateLimiting("proxy")]
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
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
            return BadRequest(new { error = "URL must be an absolute HTTP or HTTPS URL" });

        try
        {
            await UrlValidator.ValidateUrlAsync(request.Url);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var response = await _proxyService.ExecuteAsync(request);
        return Ok(response);
    }
}
