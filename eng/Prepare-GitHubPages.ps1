[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PublishDirectory,

    [Parameter(Mandatory = $true)]
    [string] $BasePath
)

$ErrorActionPreference = 'Stop'

if ($BasePath -eq '/' -or
    -not $BasePath.StartsWith('/', [StringComparison]::Ordinal) -or
    -not $BasePath.EndsWith('/', [StringComparison]::Ordinal)) {
    throw 'BasePath must contain a repository path with a leading and trailing slash.'
}

$resolvedPublishDirectory = (Resolve-Path -LiteralPath $PublishDirectory).Path
$indexPath = Join-Path $resolvedPublishDirectory 'index.html'
$frameworkPath = Join-Path $resolvedPublishDirectory '_framework'

if (-not (Test-Path -LiteralPath $indexPath -PathType Leaf)) {
    throw "Published artifact is missing index.html: $indexPath"
}

if (-not (Test-Path -LiteralPath $frameworkPath -PathType Container)) {
    throw "Published artifact is missing the Blazor _framework directory: $frameworkPath"
}

$index = Get-Content -LiteralPath $indexPath -Raw
$localBase = '<base href="/" />'
if (-not $index.Contains($localBase, [StringComparison]::Ordinal)) {
    throw "Expected the local-development base element in $indexPath."
}

$pagesBase = "<base href=`"$BasePath`" />"
$index = $index.Replace($localBase, $pagesBase, [StringComparison]::Ordinal)
Set-Content -LiteralPath $indexPath -Value $index -NoNewline

$fallbackPath = Join-Path $resolvedPublishDirectory '404.html'
Copy-Item -LiteralPath $indexPath -Destination $fallbackPath -Force

foreach ($route in @('theme', 'docs', 'docs/components')) {
    $routeDirectory = Join-Path $resolvedPublishDirectory $route
    New-Item -ItemType Directory -Path $routeDirectory -Force | Out-Null
    Copy-Item -LiteralPath $indexPath -Destination (Join-Path $routeDirectory 'index.html') -Force
}

New-Item -ItemType File -Path (Join-Path $resolvedPublishDirectory '.nojekyll') -Force | Out-Null

if ((Get-Content -LiteralPath $indexPath -Raw).Contains($localBase, [StringComparison]::Ordinal)) {
    throw "GitHub Pages artifact still contains the local base path: $indexPath"
}

Write-Output "Prepared GitHub Pages artifact at $resolvedPublishDirectory with base path $BasePath"
