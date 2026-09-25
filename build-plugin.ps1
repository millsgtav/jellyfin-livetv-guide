#requires -Version 5.1
<#
.SYNOPSIS
    Builds the plugin, packages it as a release zip, and updates manifest.json.
.EXAMPLE
    ./build-plugin.ps1 -Version 1.0.0.1 -Changelog "Fix guide path handling"
#>
[CmdletBinding()]
param(
    [string]$Version = '1.0.0.0',
    [string]$Changelog = 'Initial release.',
    [string]$TargetAbi = '12.0.0.0',
    [string]$Repo = 'millsgtav/jellyfin-livetv-guide'
)

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "Version must be in the form x.y.z.b (got '$Version')."
}

$project = 'Jellyfin.Plugin.HDHomeRunGuide'
$artifacts = Join-Path $PSScriptRoot 'artifacts'
$publishDir = Join-Path $artifacts 'publish'
$zipName = "hdhomerun-guide_$Version.zip"
$zipPath = Join-Path $artifacts $zipName

if (Test-Path $artifacts) { Remove-Item $artifacts -Recurse -Force }
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

dotnet publish $project -c Release -o $publishDir `
    "/p:AssemblyVersion=$Version" "/p:FileVersion=$Version" "/p:Version=$Version"
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

Compress-Archive -Path (Join-Path $publishDir "$project.dll") -DestinationPath $zipPath -Force

$checksum = (Get-FileHash -Path $zipPath -Algorithm MD5).Hash.ToLowerInvariant()

$manifestPath = Join-Path $PSScriptRoot 'manifest.json'
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

$entry = [pscustomobject]@{
    version    = $Version
    changelog  = $Changelog
    targetAbi  = $TargetAbi
    sourceUrl  = "https://github.com/$Repo/releases/download/v$Version/$zipName"
    checksum   = $checksum
    timestamp  = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
}

$plugin = $manifest[0]
# Newest version must be first in the list.
$plugin.versions = @($entry) + @($plugin.versions | Where-Object { $_.version -ne $Version })

# Must be BOM-less: Jellyfin's JSON parser rejects a manifest that starts with a BOM.
$json = ConvertTo-Json -InputObject @($plugin) -Depth 10
[System.IO.File]::WriteAllText($manifestPath, $json, (New-Object System.Text.UTF8Encoding $false))

Write-Host ''
Write-Host "Package : $zipPath"
Write-Host "MD5     : $checksum"
Write-Host ''
Write-Host 'Next steps:'
Write-Host "  git add -A; git commit -m ""Release $Version""; git tag v$Version"
Write-Host "  git push origin main --tags"
Write-Host "  gh release create v$Version ""$zipPath"" --title ""v$Version"" --notes ""$Changelog"""
