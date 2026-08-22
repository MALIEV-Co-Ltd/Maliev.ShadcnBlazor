[CmdletBinding()]
param(
    [string] $OutputPath = "samples/Maliev.ShadcnBlazor.Showcase/wwwroot/data/google-fonts-catalog.json"
)

$ErrorActionPreference = "Stop"

$apiKey = [Environment]::GetEnvironmentVariable("GOOGLE_FONTS_API_KEY", "Process")
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "GOOGLE_FONTS_API_KEY must be set for this maintainer-only refresh."
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$resolvedOutput = if ([IO.Path]::IsPathRooted($OutputPath)) {
    [IO.Path]::GetFullPath($OutputPath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputPath))
}
$expectedRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot "samples/Maliev.ShadcnBlazor.Showcase/wwwroot/data"))
if (-not $resolvedOutput.StartsWith($expectedRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputPath must remain inside the Showcase wwwroot/data directory."
}

$endpoint = "https://www.googleapis.com/webfonts/v1/webfonts"
$uri = [UriBuilder]::new($endpoint)
$uri.Query = "sort=alpha&capability=VF&key=$([Uri]::EscapeDataString($apiKey))"
try {
    $response = Invoke-RestMethod -Method Get -Uri $uri.Uri -Headers @{ Accept = "application/json" }
} catch {
    throw "Google Web Fonts Developer API refresh failed. Verify the maintainer credential and network access."
}

function ConvertTo-CatalogId([string] $Family) {
    $normalized = $Family.Normalize([Text.NormalizationForm]::FormD)
    $builder = [Text.StringBuilder]::new()
    foreach ($character in $normalized.ToCharArray()) {
        if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($character) -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
            [void] $builder.Append($character)
        }
    }
    return (($builder.ToString().ToLowerInvariant() -replace "[^a-z0-9]+", "-").Trim("-"))
}

function Get-FontWeights($Item) {
    $weights = foreach ($variant in @($Item.variants)) {
        if ($variant -match "^(?<weight>[1-9]00)(italic)?$") {
            [int] $Matches.weight
        } elseif ($variant -eq "regular" -or $variant -eq "italic") {
            400
        }
    }
    return @($weights | Sort-Object -Unique)
}

function Get-FontAxes($Item) {
    $axes = foreach ($axis in @($Item.axes)) {
        if ($null -ne $axis.tag -and $null -ne $axis.start -and $null -ne $axis.end) {
            [ordered]@{ tag = [string] $axis.tag; minimum = [double] $axis.start; maximum = [double] $axis.end }
        }
    }
    return @($axes | Sort-Object { $_.tag })
}

$families = foreach ($item in @($response.items)) {
    $weights = @(Get-FontWeights $item)
    $axes = @(Get-FontAxes $item)
    $subsets = @($item.subsets | Where-Object { $_ -ne "menu" } | ForEach-Object { [string] $_ } | Sort-Object -Unique)
    if (($weights.Count -eq 0 -and $axes.Count -eq 0) -or $subsets.Count -eq 0) { continue }

    $familyQuery = ([string] $item.family) -replace " ", "+"
    $weightAxis = @($axes | Where-Object tag -eq "wght" | Select-Object -First 1)
    if ($weightAxis.Count -gt 0) {
        $familyQuery += ":wght@$($weightAxis[0].minimum)..$($weightAxis[0].maximum)"
    } elseif ($weights.Count -gt 0) {
        $familyQuery += ":wght@$($weights -join ';')"
    }

    [ordered]@{
        id = ConvertTo-CatalogId ([string] $item.family)
        family = [string] $item.family
        category = [string] $item.category
        subsets = $subsets
        weights = $weights
        axes = $axes
        css2FamilyQuery = $familyQuery
    }
}

$orderedFamilies = [Collections.Generic.List[object]]::new()
foreach ($family in $families) { $orderedFamilies.Add($family) }
$orderedFamilies.Sort([Comparison[object]]{
    param($left, $right)
    [StringComparer]::Ordinal.Compare([string] $left.family, [string] $right.family)
})

$duplicates = @($orderedFamilies | Group-Object id | Where-Object Count -gt 1)
$categories = @($orderedFamilies.category | Sort-Object -Unique)
$expectedCategories = @("display", "handwriting", "monospace", "sans-serif", "serif")
if ($orderedFamilies.Count -lt 100 -or
    @($orderedFamilies | Where-Object { [string]::IsNullOrWhiteSpace($_.id) }).Count -gt 0 -or
    $duplicates.Count -gt 0 -or
    @($orderedFamilies | Where-Object { $_.subsets -contains "thai" }).Count -lt 10 -or
    (Compare-Object $expectedCategories $categories).Count -gt 0) {
    throw "Google Fonts metadata failed broad-catalog integrity checks; the existing snapshot was not changed."
}

$snapshot = [ordered]@{
    schemaVersion = 1
    source = "google-webfonts-developer-api"
    sourceTimestamp = [DateTimeOffset]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", [Globalization.CultureInfo]::InvariantCulture)
    families = @($orderedFamilies)
}

$parent = Split-Path -Parent $resolvedOutput
[IO.Directory]::CreateDirectory($parent) | Out-Null
$temporary = "$resolvedOutput.tmp"
try {
    $json = $snapshot | ConvertTo-Json -Depth 8
    [IO.File]::WriteAllText($temporary, ($json -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
    Move-Item -LiteralPath $temporary -Destination $resolvedOutput -Force
} finally {
    if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force }
}

Write-Output "Refreshed $($snapshot.families.Count) Google Fonts families."
