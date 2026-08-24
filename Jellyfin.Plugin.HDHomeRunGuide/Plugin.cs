using System;
using System.Collections.Generic;
using Jellyfin.Plugin.HDHomeRunGuide.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.HDHomeRunGuide;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "HDHomeRun Live TV Guide";

    public override string Description => "Downloads the HDHomeRun XMLTV guide and saves it to a local file for use as an XMLTV guide source.";

    public override Guid Id => Guid.Parse("9f8b1c3a-2d47-4e6b-9a51-7c2e0d4f8b13");

    public IEnumerable<PluginPageInfo> GetPages() =>
    [
        new PluginPageInfo
        {
            Name = Name,
            EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html"
        }
    ];
}
