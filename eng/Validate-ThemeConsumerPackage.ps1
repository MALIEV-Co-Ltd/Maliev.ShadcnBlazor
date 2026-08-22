param(
    [Parameter(Mandatory = $true)]
    [string]$Root,

    [Parameter(Mandatory = $true)]
    [string]$Package
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path -LiteralPath $Root).Path
$packagePath = (Resolve-Path -LiteralPath $Package).Path
$source = Join-Path $repositoryRoot 'samples/Maliev.ShadcnBlazor.ThemeConsumer'
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw "Theme consumer sample was not found at $source."
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $nuspec = @($archive.Entries | Where-Object { $_.FullName -like '*.nuspec' })
    if ($nuspec.Count -ne 1) {
        throw "Expected exactly one nuspec in $packagePath, found $($nuspec.Count)."
    }
    $reader = [System.IO.StreamReader]::new($nuspec[0].Open())
    try {
        $metadata = [System.Xml.Linq.XDocument]::Parse($reader.ReadToEnd())
    }
    finally {
        $reader.Dispose()
    }
    $namespace = $metadata.Root.Name.Namespace
    $version = $metadata.Root.Element($namespace + 'metadata').Element($namespace + 'version').Value
}
finally {
    $archive.Dispose()
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("maliev-theme-consumer-" + [Guid]::NewGuid().ToString('N'))
$consumer = Join-Path $temporaryRoot 'consumer'
$packages = Join-Path $temporaryRoot 'packages'
$config = Join-Path $temporaryRoot 'NuGet.config'

try {
    New-Item -ItemType Directory -Path $consumer, $packages -Force | Out-Null
    Get-ChildItem -LiteralPath $source -Force | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $consumer -Recurse -Force
    }
    foreach ($generated in @('bin', 'obj')) {
        $path = Join-Path $consumer $generated
        if (Test-Path -LiteralPath $path) {
            Remove-Item -LiteralPath $path -Recurse -Force
        }
    }

    $localSource = [System.Security.SecurityElement]::Escape((Split-Path -Parent $packagePath))
    [System.IO.File]::WriteAllText($config, @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$localSource" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <config>
    <add key="globalPackagesFolder" value="$packages" />
  </config>
</configuration>
"@)

    $project = Join-Path $consumer 'Maliev.ShadcnBlazor.ThemeConsumer.csproj'
    $properties = @(
        '-p:UseMalievShadcnPackage=true',
        "-p:MalievShadcnPackageVersion=$version"
    )

    & dotnet restore $project --configfile $config --force-evaluate @properties
    if ($LASTEXITCODE -ne 0) { throw "Theme consumer package restore failed with exit code $LASTEXITCODE." }

    & dotnet restore $project --configfile $config --locked-mode @properties
    if ($LASTEXITCODE -ne 0) { throw "Theme consumer locked restore failed with exit code $LASTEXITCODE." }

    & dotnet build $project -c Release --no-restore @properties
    if ($LASTEXITCODE -ne 0) { throw "Theme consumer package build failed with exit code $LASTEXITCODE." }

    $framework = Join-Path $consumer 'bin/Release/net10.0/wwwroot/_framework'
    if (-not (Test-Path -LiteralPath $framework -PathType Container)) {
        throw "Theme consumer build did not produce a Blazor framework directory."
    }
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}
