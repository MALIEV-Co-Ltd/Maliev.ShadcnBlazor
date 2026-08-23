[CmdletBinding()]
param(
    [ValidateSet('all', 'lucide', 'tabler', 'phosphor', 'hugeicons')]
    [string]$Library = 'all',
    [string]$DestinationRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$ArchiveDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$allowedElements = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($name in 'circle', 'ellipse', 'g', 'line', 'path', 'polygon', 'polyline', 'rect') { $null = $allowedElements.Add($name) }
$allowedAttributes = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($name in 'clip-rule', 'cx', 'cy', 'd', 'fill', 'fill-rule', 'height', 'opacity', 'points', 'r', 'rx', 'ry', 'stroke', 'stroke-dasharray', 'stroke-dashoffset', 'stroke-linecap', 'stroke-linejoin', 'stroke-miterlimit', 'stroke-width', 'transform', 'width', 'x', 'x1', 'x2', 'y', 'y1', 'y2') { $null = $allowedAttributes.Add($name) }
$rootPresentationAttributes = @('fill', 'opacity', 'stroke', 'stroke-linecap', 'stroke-linejoin', 'stroke-miterlimit', 'stroke-width')
$reservedIdentifiers = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($name in 'Equals', 'Finalize', 'GetHashCode', 'GetType', 'MemberwiseClone', 'ReferenceEquals', 'ToString') { $null = $reservedIdentifiers.Add($name) }
$utf8NoBom = [Text.UTF8Encoding]::new($false)

function Write-AtomicUtf8Lf([string]$Path, [string]$Content) {
    $directory = Split-Path -Parent $Path
    [IO.Directory]::CreateDirectory($directory) | Out-Null
    $temporary = Join-Path $directory ('.' + [IO.Path]::GetFileName($Path) + '.' + [Guid]::NewGuid().ToString('N') + '.tmp')
    [IO.File]::WriteAllText($temporary, ($Content -replace "`r`n", "`n").TrimEnd() + "`n", $utf8NoBom)
    Move-Item -LiteralPath $temporary -Destination $Path -Force
}

function Normalize-PresentationValue([string]$Name, [string]$Value) {
    $normalized = $Value.Trim()
    if ($Name -in 'fill', 'stroke' -and $normalized -notin 'none', 'currentColor') {
        return 'currentColor'
    }
    if ($normalized.Contains('url(', [StringComparison]::OrdinalIgnoreCase) -or
        $normalized.Contains('javascript:', [StringComparison]::OrdinalIgnoreCase) -or
        $normalized.Contains('http:', [StringComparison]::OrdinalIgnoreCase) -or
        $normalized.Contains('https:', [StringComparison]::OrdinalIgnoreCase)) {
        throw "External or executable SVG attribute value is forbidden."
    }
    return $normalized
}

function Sanitize-Element([Xml.XmlElement]$Element, [Xml.XmlDocument]$OutputDocument) {
    if (-not $allowedElements.Contains($Element.LocalName)) {
        throw "SVG element '$($Element.LocalName)' is not allowed."
    }

    $output = $OutputDocument.CreateElement($Element.LocalName)
    foreach ($attribute in @($Element.Attributes)) {
        if ($attribute.Prefix -eq 'xmlns' -or $attribute.Name -eq 'xmlns') { continue }
        if (-not $allowedAttributes.Contains($attribute.LocalName) -or
            $attribute.LocalName.StartsWith('on', [StringComparison]::OrdinalIgnoreCase) -or
            $attribute.LocalName -in 'href', 'xlink:href', 'style') {
            throw "SVG attribute '$($attribute.Name)' is not allowed."
        }
        $output.SetAttribute($attribute.LocalName, (Normalize-PresentationValue $attribute.LocalName $attribute.Value))
    }

    foreach ($child in @($Element.ChildNodes)) {
        if ($child.NodeType -eq [Xml.XmlNodeType]::Element) {
            $output.AppendChild((Sanitize-Element ([Xml.XmlElement]$child) $OutputDocument)) | Out-Null
        }
        elseif ($child.NodeType -in [Xml.XmlNodeType]::Text, [Xml.XmlNodeType]::CDATA -and -not [string]::IsNullOrWhiteSpace($child.Value)) {
            throw 'SVG geometry cannot contain text.'
        }
    }
    return $output
}

function Convert-Icon([IO.FileInfo]$File, [pscustomobject]$Source) {
    if ($File.Length -gt 131072) { throw "Icon '$($File.Name)' exceeds the 128 KiB source limit." }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $settings.IgnoreComments = $true
    $settings.IgnoreProcessingInstructions = $true
    $reader = [Xml.XmlReader]::Create($File.FullName, $settings)
    try {
        $input = [Xml.XmlDocument]::new()
        $input.XmlResolver = $null
        $input.Load($reader)
    }
    finally { $reader.Dispose() }

    if ($input.DocumentElement.LocalName -ne 'svg') { throw "Icon '$($File.Name)' has no SVG root." }
    $viewBox = $input.DocumentElement.GetAttribute('viewBox').Trim()
    $parts = $viewBox -split '\s+'
    $number = 0.0
    if ($parts.Count -ne 4 -or @($parts | Where-Object { -not [double]::TryParse($_, [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$number) }).Count -ne 0) {
        throw "Icon '$($File.Name)' has an invalid viewBox."
    }

    $output = [Xml.XmlDocument]::new()
    $geometry = [Collections.Generic.List[Xml.XmlElement]]::new()
    foreach ($child in @($input.DocumentElement.ChildNodes)) {
        if ($child.NodeType -eq [Xml.XmlNodeType]::Element) {
            $geometry.Add((Sanitize-Element ([Xml.XmlElement]$child) $output))
        }
        elseif ($child.NodeType -in [Xml.XmlNodeType]::Text, [Xml.XmlNodeType]::CDATA -and -not [string]::IsNullOrWhiteSpace($child.Value)) {
            throw "Icon '$($File.Name)' contains text."
        }
    }
    if ($geometry.Count -eq 0) { throw "Icon '$($File.Name)' contains no geometry." }

    $rootAttributes = [ordered]@{}
    foreach ($attributeName in $rootPresentationAttributes) {
        if ($input.DocumentElement.HasAttribute($attributeName)) {
            $rootAttributes[$attributeName] = Normalize-PresentationValue $attributeName $input.DocumentElement.GetAttribute($attributeName)
        }
    }
    if ($rootAttributes.Count -gt 0) {
        $group = $output.CreateElement('g')
        foreach ($entry in $rootAttributes.GetEnumerator()) { $group.SetAttribute($entry.Key, $entry.Value) }
        foreach ($element in $geometry) { $group.AppendChild($element) | Out-Null }
        $content = $group.OuterXml
    }
    else {
        $content = [string]::Concat(($geometry | ForEach-Object OuterXml))
    }

    return [ordered]@{
        library = $Source.id
        name = $File.BaseName
        viewBox = $viewBox
        svgContent = $content
    }
}

function Convert-ToIdentifier([string]$Name) {
    $parts = $Name -split '[^A-Za-z0-9]+' | Where-Object Length
    $identifier = [string]::Concat(($parts | ForEach-Object {
        if ($_.Length -eq 1) { $_.ToUpperInvariant() } else { $_[0].ToString().ToUpperInvariant() + $_.Substring(1) }
    }))
    if ([string]::IsNullOrEmpty($identifier)) { throw "Icon name '$Name' cannot become a C# identifier." }
    if ([char]::IsDigit($identifier[0])) { $identifier = 'Icon' + $identifier }
    if ($reservedIdentifiers.Contains($identifier)) { $identifier = 'Icon' + $identifier }
    return $identifier
}

$manifestPath = Join-Path $PSScriptRoot 'icon-sources.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.schemaVersion -ne 1) { throw 'Unsupported icon source manifest version.' }
$sources = @($manifest.sources | Where-Object { $Library -eq 'all' -or $_.id -eq $Library })
if ($sources.Count -eq 0) { throw "No source matched '$Library'." }

foreach ($source in $sources) {
    $working = Join-Path ([IO.Path]::GetTempPath()) ('maliev-icon-import-' + [Guid]::NewGuid().ToString('N'))
    [IO.Directory]::CreateDirectory($working) | Out-Null
    try {
        $archive = Join-Path $working ($source.id + '.zip')
        $cachedArchive = if ([string]::IsNullOrWhiteSpace($ArchiveDirectory)) { $null } else { Join-Path $ArchiveDirectory ($source.id + '.zip') }
        if ($cachedArchive -and (Test-Path -LiteralPath $cachedArchive)) {
            Copy-Item -LiteralPath $cachedArchive -Destination $archive
        }
        else {
            Invoke-WebRequest -Uri $source.archiveUrl -OutFile $archive
        }
        $actualHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualHash -ne $source.archiveSha256) { throw "Archive hash mismatch for '$($source.id)'." }

        $extracted = Join-Path $working 'extracted'
        Expand-Archive -LiteralPath $archive -DestinationPath $extracted
        $archiveRoot = @(Get-ChildItem -LiteralPath $extracted -Directory)
        if ($archiveRoot.Count -ne 1) { throw "Archive '$($source.id)' must contain one root directory." }
        $sourceDirectory = Join-Path $archiveRoot[0].FullName ($source.sourceDirectory -replace '/', [IO.Path]::DirectorySeparatorChar)
        $filesByName = [Collections.Generic.Dictionary[string, IO.FileInfo]]::new([StringComparer]::Ordinal)
        foreach ($sourceFile in Get-ChildItem -LiteralPath $sourceDirectory -File -Filter $source.sourcePattern) {
            if (-not $filesByName.TryAdd($sourceFile.BaseName, $sourceFile)) { throw "Duplicate icon name '$($sourceFile.BaseName)'." }
        }
        [string[]]$orderedSourceNames = @($filesByName.Keys)
        [Array]::Sort($orderedSourceNames, [StringComparer]::Ordinal)
        if ($orderedSourceNames.Count -eq 0) { throw "No SVG icons found for '$($source.id)'." }

        $icons = [Collections.Generic.List[object]]::new()
        $names = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $identifiers = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($sourceName in $orderedSourceNames) {
            $file = $filesByName[$sourceName]
            $icon = Convert-Icon $file $source
            if (-not $names.Add($icon.name)) { throw "Duplicate icon name '$($icon.name)'." }
            $identifier = Convert-ToIdentifier $icon.name
            if (-not $identifiers.Add($identifier)) { throw "C# icon identifier collision '$identifier'." }
            $icons.Add($icon)
        }

        $projectRoot = Join-Path $DestinationRoot ('src/Maliev.ShadcnBlazor.Icons.' + $source.packageSuffix)
        $catalog = [ordered]@{ schemaVersion = 1; library = $source.id; version = $source.version; commit = $source.commit; icons = $icons }
        Write-AtomicUtf8Lf (Join-Path $projectRoot 'Catalog/icons.json') ($catalog | ConvertTo-Json -Depth 8 -Compress)

        $className = $source.packageSuffix + 'IconNames'
        $lines = [Collections.Generic.List[string]]::new()
        $lines.Add('// <auto-generated />')
        $lines.Add(('namespace Maliev.ShadcnBlazor.Icons.' + $source.packageSuffix + ';'))
        $lines.Add('')
        $lines.Add('/// <summary>Stable names for the checked-in free icon catalog.</summary>')
        $lines.Add(('public static class ' + $className))
        $lines.Add('{')
        foreach ($icon in $icons) {
            $lines.Add(('    /// <summary>Gets the catalog name for {0}.</summary>' -f $icon.name))
            $lines.Add(('    public const string {0} = "{1}";' -f (Convert-ToIdentifier $icon.name), $icon.name))
        }
        $lines.Add('}')
        Write-AtomicUtf8Lf (Join-Path $projectRoot ('Generated/' + $className + '.g.cs')) ([string]::Join("`n", $lines))

        $licenseSource = Join-Path $archiveRoot[0].FullName ($source.licensePath -replace '/', [IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $licenseSource)) { throw "License file missing for '$($source.id)'." }
        Write-AtomicUtf8Lf (Join-Path $projectRoot ('licenses/' + $source.licenseFileName)) (Get-Content -LiteralPath $licenseSource -Raw)
        Write-Host ("Generated {0} icons for {1}." -f $icons.Count, $source.displayName)
    }
    finally {
        if (Test-Path -LiteralPath $working) { Remove-Item -LiteralPath $working -Recurse -Force }
    }
}
