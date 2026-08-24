using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HDHomeRunGuide.Api;

[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("HDHomeRunGuide")]
[Produces(MediaTypeNames.Application.Json)]
public class GuideController : ControllerBase
{
    private readonly GuideDownloader _downloader;
    private readonly ILogger<GuideController> _logger;

    public GuideController(GuideDownloader downloader, ILogger<GuideController> logger)
    {
        _downloader = downloader;
        _logger = logger;
    }

    /// <summary>Downloads the guide immediately using the saved configuration.</summary>
    [HttpPost("Refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RefreshResult>> Refresh(CancellationToken cancellationToken)
    {
        try
        {
            var path = await _downloader.DownloadAsync(cancellationToken).ConfigureAwait(false);
            return Ok(new RefreshResult(true, path));
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Net.Http.HttpRequestException or System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            _logger.LogError(ex, "Guide download failed");
            return BadRequest(new RefreshResult(false, ex.Message));
        }
    }

    public record RefreshResult(bool Success, string Message);
}
