<#
.SYNOPSIS
    Adds immutable external module release artifacts to Release staging.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$configPath = Join-Path $env:GITHUB_WORKSPACE $env:RELEASE_CONFIG_FILE
$stagingDirectory = $env:STAGING_DIRECTORY

if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) { throw "Release configuration was not found: $configPath" }
if (-not (Test-Path -LiteralPath $stagingDirectory -PathType Container)) { throw "Release staging directory was not found: $stagingDirectory" }

$config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
if ($null -eq $config.modules) { throw "Release configuration property 'modules' must be an array. Use [] when no external modules are configured." }
$modules = @($config.modules)
if ($modules.Count -eq 0) { Write-Host 'No external modules are configured.'; exit 0 }

$destinationPaths = @{}
foreach ($module in $modules) {
    $repository = [string]$module.repository
    $projects = @($module.projects)
    if ($repository -notmatch '^[^/\s]+/[^/\s]+$') { throw "Invalid module repository '$repository'. Expected owner/repository." }
    if ($projects.Count -eq 0) { throw "Module '$repository' must define at least one project in 'projects'." }
    foreach ($projectValue in $projects) {
        $project = [string]$projectValue
        $normalizedProject = $project.Replace('\','/').Trim('/')
        if ([string]::IsNullOrWhiteSpace($normalizedProject)) { throw "Module '$repository' contains an empty project path." }
        if ($normalizedProject -match '(^|/)\.\.(/|$)') { throw "Invalid project path '$project' in module '$repository'. Parent directory traversal is not allowed." }
        if ([System.IO.Path]::IsPathRooted($normalizedProject)) { throw "Invalid project path '$project' in module '$repository'. Rooted paths are not allowed." }
        $destinationName = [System.IO.Path]::GetFileName($normalizedProject)
        if ([string]::IsNullOrWhiteSpace($destinationName) -or $destinationName -eq '.' -or $destinationName -eq '..') { throw "Invalid module project path '$project'." }
        if ($destinationPaths.ContainsKey($destinationName)) { throw "Duplicate module project destination '$destinationName'." }
        $destinationPaths[$destinationName] = $repository
    }
}

$downloadRoot = Join-Path $env:RUNNER_TEMP ("MegaModules_" + $env:GITHUB_RUN_ID)
New-Item -ItemType Directory -Path $downloadRoot -Force | Out-Null
$lock = @()

$originalGhToken = $env:GH_TOKEN

function Invoke-GhWithFallback {
    param(
        [Parameter(Mandatory)][scriptblock]$Command,
        [Parameter(Mandatory)][string]$Operation
    )

    $result = & $Command 2>$null
    if ($LASTEXITCODE -eq 0) {
        return $result
    }

    if ([string]::IsNullOrWhiteSpace($env:PRIVATE_SUBMODULE_TOKEN)) {
        throw "$Operation failed with GITHUB_TOKEN and PRIVATE_SUBMODULE_TOKEN is not configured."
    }

    $env:GH_TOKEN = $env:PRIVATE_SUBMODULE_TOKEN
    try {
        $result = & $Command
        if ($LASTEXITCODE -ne 0) {
            throw "$Operation failed with both GITHUB_TOKEN and PRIVATE_SUBMODULE_TOKEN."
        }
        return $result
    }
    finally {
        $env:GH_TOKEN = $originalGhToken
    }
}

function Get-LatestRelease {
    param([Parameter(Mandatory)][string]$Repository)
    $json = & {
        Invoke-GhWithFallback -Operation "Could not get latest published release for '$Repository'." -Command {
            gh api "repos/$Repository/releases/latest" --header "Accept: application/vnd.github+json"
        }
    }
    return ($json | ConvertFrom-Json)
}

function Download-Asset {
    param([Parameter(Mandatory)][string]$Repository,[Parameter(Mandatory)][string]$Tag,[Parameter(Mandatory)][string]$AssetName,[Parameter(Mandatory)][string]$Directory)
    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    Invoke-GhWithFallback -Operation "Could not download asset '$AssetName' from '$Repository' release '$Tag'." -Command {
        gh release download $Tag --repo $Repository --pattern $AssetName --dir $Directory --clobber
    }
    $path = Join-Path $Directory $AssetName
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Downloaded asset was not found: $path" }
    return $path
}

function Copy-ZipEntry {
    param([Parameter(Mandatory)]$Entry,[Parameter(Mandatory)][string]$DestinationRoot,[string]$StripPrefix = '')
    $entryName = $Entry.FullName.Replace('\','/')
    $segments = @($entryName -split '/')
    if ($segments | Where-Object { $_ -eq '..' }) { throw "Unsafe ZIP entry path: '$entryName'." }
    if ([System.IO.Path]::IsPathRooted($entryName)) { throw "Unsafe rooted ZIP entry path: '$entryName'." }
    $normalizedPrefix = $StripPrefix.Replace('\','/').TrimStart('/')
    if ($normalizedPrefix -and -not $normalizedPrefix.EndsWith('/')) { $normalizedPrefix += '/' }
    if ($normalizedPrefix) {
        if (-not $entryName.StartsWith($normalizedPrefix, [System.StringComparison]::Ordinal)) { throw "ZIP entry '$entryName' is outside expected root '$normalizedPrefix'." }
        $relativeName = $entryName.Substring($normalizedPrefix.Length)
    } else { $relativeName = $entryName }
    if ([string]::IsNullOrWhiteSpace($relativeName)) { return }
    $relativeName = $relativeName.Replace('/', [System.IO.Path]::DirectorySeparatorChar)
    $destination = Join-Path $DestinationRoot $relativeName
    if ($Entry.FullName.EndsWith('/')) { New-Item -ItemType Directory -Path $destination -Force | Out-Null; return }
    $parent = Split-Path -Parent $destination
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    $input = $Entry.Open()
    try {
        $output = [System.IO.File]::Create($destination)
        try { $input.CopyTo($output) } finally { $output.Dispose() }
    } finally { $input.Dispose() }
}

function Get-ArchiveRootDirectory {
    param([Parameter(Mandatory)]$Archive)
    $topLevelDirectories = @(
        $Archive.Entries | ForEach-Object {
            $name = $_.FullName.Replace('\','/')
            if ($name -match '^([^/]+)/') { $matches[1] }
        } | Sort-Object -Unique
    )
    if ($topLevelDirectories.Count -ne 1) { throw "Expected exactly one root directory in the selected module artifact; found $($topLevelDirectories.Count)." }
    return [string]$topLevelDirectories[0]
}

function Copy-ProjectFromZip {
    param([Parameter(Mandatory)]$Archive,[Parameter(Mandatory)][string]$Project,[Parameter(Mandatory)][string]$DestinationRoot)
    $normalizedProject = $Project.Replace('\','/').Trim('/')
    $projectPrefix = $normalizedProject.Trim('/') + '/'
    $projectEntries = @($Archive.Entries | Where-Object { $_.FullName.Replace('\','/').StartsWith($projectPrefix, [System.StringComparison]::Ordinal) })
    if ($projectEntries.Count -eq 0) { throw "Project directory '$normalizedProject' was not found in the selected artifact." }

    $destinationName = [System.IO.Path]::GetFileName($normalizedProject)
    $projectDestination = Join-Path $DestinationRoot $destinationName
    foreach ($entry in $projectEntries) {
        Copy-ZipEntry -Entry $entry -DestinationRoot $projectDestination -StripPrefix $normalizedProject
    }
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

for ($moduleIndex = 0; $moduleIndex -lt $modules.Count; $moduleIndex++) {
    $module = $modules[$moduleIndex]
    $repository = [string]$module.repository
    $projects = @($module.projects) | ForEach-Object { ([string]$_).Replace('\','/').Trim('/') }
    Write-Host ("=== External module repository: " + $repository + " ===")
    Write-Host ("Projects: " + ($projects -join ', '))

    $release = Get-LatestRelease -Repository $repository
    $tag = [string]$release.tag_name
    if ([string]::IsNullOrWhiteSpace($tag)) { throw "Latest release of '$repository' does not contain tag_name." }
    $zipAssets = @($release.assets | Where-Object { $_.state -eq 'uploaded' -and $_.name -match '\.zip$' })
    if ($zipAssets.Count -ne 1) { throw "Expected exactly one ZIP release asset in '$repository' release '$tag'; found $($zipAssets.Count)." }
    $asset = $zipAssets[0]
    $assetName = [string]$asset.name
    $digest = [string]$asset.digest
    if ($digest -notmatch '^sha256:[0-9a-fA-F]{64}$') { throw "Release asset '$assetName' in '$repository' has no valid SHA-256 digest." }
    $expectedHash = $digest.Substring(7).ToLowerInvariant()
    $moduleDownloadDirectory = Join-Path $downloadRoot ("module-" + $moduleIndex)
    $archivePath = Download-Asset -Repository $repository -Tag $tag -AssetName $assetName -Directory $moduleDownloadDirectory
    $actualHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -ne $expectedHash) { throw "SHA-256 mismatch for '$repository' release '$tag' asset '$assetName'. Expected '$expectedHash', actual '$actualHash'." }
    Write-Host ("Release fixed: " + $tag)
    Write-Host ("Artifact fixed: " + $assetName)
    Write-Host ("SHA-256 verified: " + $actualHash)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $archiveRoot = Get-ArchiveRootDirectory -Archive $archive
        Write-Host ("Archive root: " + $archiveRoot)
        foreach ($project in $projects) {
            Write-Host ("Extracting project: " + $project)
            Copy-ProjectFromZip -Archive $archive -Project $project -DestinationRoot (Join-Path $stagingDirectory $env:PRODUCT)
        }

        $rootMarkdown = @($archive.Entries | Where-Object {
            $entryName = $_.FullName.Replace('\','/')
            ($entryName -notmatch '/') -and ($entryName -match '\.md$')
        })
        if ($rootMarkdown.Count -ne 1) { throw "Expected exactly one MD file at the ZIP root in '$repository' release '$tag'; found $($rootMarkdown.Count)." }

        $moduleMarkdownName = [System.IO.Path]::GetFileName($rootMarkdown[0].FullName)
        $megaMarkdownPath = Join-Path $stagingDirectory $moduleMarkdownName
        if (Test-Path -LiteralPath $megaMarkdownPath) { throw "Module Markdown '$moduleMarkdownName' would overwrite an existing Mega release file." }
        Copy-ZipEntry -Entry $rootMarkdown[0] -DestinationRoot $stagingDirectory
    }
    finally { $archive.Dispose() }

    $lock += [pscustomobject]@{ repository=$repository; projects=@($projects); release=$tag; artifact=$assetName; sha256=$actualHash }
}

$lockPath = Join-Path $env:RUNNER_TEMP ("mega-modules-" + $env:GITHUB_RUN_ID + ".lock.json")
$lock | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $lockPath -Encoding utf8NoBOM
Write-Host '=== External module selection ==='
$lock | Format-Table -AutoSize | Out-String | Write-Host
Write-Host ("Module selection lock: " + $lockPath)
