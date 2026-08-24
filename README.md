# HDHomeRun Live TV Guide (Jellyfin plugin)

Downloads the HDHomeRun XMLTV guide and saves it to a local file, replacing the
manual Postman request.

`GET {BaseUrl}?Email={Email}&DeviceIDs={DeviceIds}` → `{OutputPath}\{FileName}`

## Configurable fields

| Field | Default |
| --- | --- |
| Guide API URL | `https://api.hdhomerun.com/api/xmltv` |
| SiliconDust account email | *(empty)* |
| Device IDs (comma separated) | *(empty)* |
| Output folder | *(empty)* |
| File name | `guide.xml` |
| Refresh interval (hours) | `12` |

## Install from the plugin repository

In Jellyfin: **Dashboard → Plugins → Repositories → +**

- Repository name: `HDHomeRun Live TV Guide`
- Repository URL: `https://raw.githubusercontent.com/millsgtav/jellyfin-livetv-guide/main/manifest.json`

Then **Catalog → Live TV → HDHomeRun Live TV Guide → Install**, and restart Jellyfin.

## Publishing a release

```powershell
./build-plugin.ps1 -Version 1.0.0.1 -Changelog "What changed"
```

This publishes the DLL, zips it to `artifacts/`, computes the MD5, and prepends the
new version to [manifest.json](manifest.json). Then commit the manifest and attach the
zip to a GitHub release tagged `v<version>` — the `sourceUrl` in the manifest points at
that release asset:

```powershell
git add -A; git commit -m "Release 1.0.0.1"; git tag v1.0.0.1
git push origin main --tags
gh release create v1.0.0.1 artifacts/hdhomerun-guide_1.0.0.1.zip --title v1.0.0.1 --notes "What changed"
```

The repository must be public, and the manifest must be committed *after* the release
asset exists, otherwise Jellyfin's download will 404.

## Manual build / install

```powershell
dotnet publish Jellyfin.Plugin.HDHomeRunGuide -c Release
```

Copy `Jellyfin.Plugin.HDHomeRunGuide/bin/Release/net8.0/publish/Jellyfin.Plugin.HDHomeRunGuide.dll`
into a new folder under the Jellyfin plugins directory, e.g.
`%ProgramData%\Jellyfin\Server\plugins\HDHomeRun Live TV Guide_1.0.0.0\`, then restart Jellyfin.

## Use

1. Dashboard → Plugins → HDHomeRun Live TV Guide → fill in the fields → **Save**.
2. **Save and download now** writes the file immediately.
3. Dashboard → Scheduled Tasks → *Download HDHomeRun guide* runs on the configured interval.
4. Dashboard → Live TV → Guide Data Providers → add an XMLTV source pointing at the saved file.

The Jellyfin service account must have write access to the output folder. If the
folder is a mapped network drive, use a UNC path (`\\server\share\...`) since
drive letters are per-user.
