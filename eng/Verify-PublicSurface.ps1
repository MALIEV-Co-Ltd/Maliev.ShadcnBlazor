[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Root,

    [string] $Package
)

$ErrorActionPreference = 'Stop'
$resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
$forbidden = @(
    (@('Legacy', 'Maliev') -join '.'),
    (@('LEGACY', 'ARTIFACT', 'REGISTRY') -join '_'),
    (@('Maliev', 'Workspace', 'Root') -join ''),
    ('github.com/MALIEV-Co-Ltd/' + (@('Legacy', '.') -join ''))
)
$forbiddenDirectories = '(^|/)(bin|obj|node_modules|dist|\.artifacts)(/|$)'
$violations = [System.Collections.Generic.List[string]]::new()

$tracked = @(& git -C $resolvedRoot ls-files)
$untracked = @(& git -C $resolvedRoot ls-files --others --exclude-standard)
$files = @($tracked + $untracked | Sort-Object -Unique)

foreach ($relative in $files) {
    $normalized = $relative.Replace('\', '/')
    if ($normalized -match $forbiddenDirectories) {
        $violations.Add("generated path: $normalized")
        continue
    }

    $path = Join-Path $resolvedRoot $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes -contains 0) {
        continue
    }

    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
    foreach ($term in $forbidden) {
        if ($text.IndexOf($term, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $violations.Add("private identifier in ${normalized}: $term")
        }
    }
}

if ($Package) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $resolvedPackage = (Resolve-Path -LiteralPath $Package).Path
    $archive = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackage)
    try {
        foreach ($entry in $archive.Entries) {
            if ($entry.FullName -match $forbiddenDirectories) {
                $violations.Add("generated package entry: $($entry.FullName)")
                continue
            }

            if ($entry.Length -eq 0 -or $entry.Length -gt 10MB) {
                continue
            }

            $stream = $entry.Open()
            try {
                $buffer = [System.IO.MemoryStream]::new()
                try {
                    $stream.CopyTo($buffer)
                    $bytes = $buffer.ToArray()
                }
                finally {
                    $buffer.Dispose()
                }

                if ($bytes -contains 0) {
                    continue
                }

                $text = [System.Text.Encoding]::UTF8.GetString($bytes)
                foreach ($term in $forbidden) {
                    if ($text.IndexOf($term, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
                        $violations.Add("private identifier in package entry $($entry.FullName): $term")
                    }
                }
                if ($text -match '[A-Za-z]:\\[^\r\n\0]+') {
                    $violations.Add("absolute Windows path in package entry $($entry.FullName)")
                }
            }
            finally {
                $stream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}

if ($violations.Count -gt 0) {
    $violations | Sort-Object -Unique | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "Public surface verified: $($files.Count) repository files inspected."
