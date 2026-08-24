using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HDHomeRunGuide;

public class GuideDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GuideDownloader> _logger;

    public GuideDownloader(IHttpClientFactory httpClientFactory, ILogger<GuideDownloader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Downloads the guide and returns the full path of the file that was written.</summary>
    public async Task<string> DownloadAsync(CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration
            ?? throw new InvalidOperationException("Plugin is not initialised.");

        if (string.IsNullOrWhiteSpace(config.Email))
        {
            throw new InvalidOperationException("Email is not configured.");
        }

        if (string.IsNullOrWhiteSpace(config.DeviceIds))
        {
            throw new InvalidOperationException("Device IDs are not configured.");
        }

        if (string.IsNullOrWhiteSpace(config.OutputPath))
        {
            throw new InvalidOperationException("Output path is not configured.");
        }

        var fileName = string.IsNullOrWhiteSpace(config.FileName) ? "guide.xml" : config.FileName.Trim();
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException("File name contains invalid characters.");
        }

        if (!Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Base URL must be an absolute http(s) URL.");
        }

        var url = $"{baseUri.GetLeftPart(UriPartial.Path)}?Email={Uri.EscapeDataString(config.Email.Trim())}&DeviceIDs={Uri.EscapeDataString(config.DeviceIds.Trim())}";

        var directory = Path.GetFullPath(config.OutputPath.Trim());
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, fileName);

        // Device IDs and email are omitted from logs so shared log files stay safe.
        _logger.LogInformation("Downloading HDHomeRun guide");

        var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // Write to a temp file first so a failed download never truncates a working guide.
        var tempFile = destination + ".tmp";
        long written;
        await using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
        await using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            written = fileStream.Length;
        }

        if (written == 0)
        {
            File.Delete(tempFile);
            throw new InvalidOperationException("The guide response was empty; the existing file was left untouched.");
        }

        File.Move(tempFile, destination, overwrite: true);
        _logger.LogInformation("Saved {Bytes} bytes to {Path}", written, destination);

        return destination;
    }
}
