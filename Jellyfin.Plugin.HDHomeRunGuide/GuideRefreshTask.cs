using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.HDHomeRunGuide;

public class GuideRefreshTask : IScheduledTask
{
    private readonly GuideDownloader _downloader;

    public GuideRefreshTask(GuideDownloader downloader)
    {
        _downloader = downloader;
    }

    public string Name => "Download HDHomeRun guide";

    public string Key => "HDHomeRunGuideRefresh";

    public string Description => "Downloads the HDHomeRun XMLTV guide and saves it to the configured file.";

    public string Category => "Live TV";

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);
        await _downloader.DownloadAsync(cancellationToken).ConfigureAwait(false);
        progress.Report(100);
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        var hours = Plugin.Instance?.Configuration.RefreshIntervalHours ?? 12;
        if (hours < 1)
        {
            hours = 1;
        }

        return
        [
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.IntervalTrigger,
                IntervalTicks = TimeSpan.FromHours(hours).Ticks
            }
        ];
    }
}
