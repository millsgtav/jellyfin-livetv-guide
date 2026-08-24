using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.HDHomeRunGuide.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>Endpoint used to request the XMLTV guide.</summary>
    public string BaseUrl { get; set; } = "https://api.hdhomerun.com/api/xmltv";

    /// <summary>Email address associated with the SiliconDust account.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Comma separated HDHomeRun device IDs, e.g. "1234ABCD,5678EF01".</summary>
    public string DeviceIds { get; set; } = string.Empty;

    /// <summary>Directory the guide file is written to.</summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>File name written into <see cref="OutputPath"/>.</summary>
    public string FileName { get; set; } = "guide.xml";

    /// <summary>How often the scheduled task downloads the guide, in hours.</summary>
    public int RefreshIntervalHours { get; set; } = 12;
}
