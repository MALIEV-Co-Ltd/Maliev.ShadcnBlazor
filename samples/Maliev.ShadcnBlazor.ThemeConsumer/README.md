# Portable theme consumer

This checked Blazor WebAssembly app proves that an exported canonical
`theme.json` and `theme.css` work with either the repository project reference
or the packed `Maliev.ShadcnBlazor` NuGet package.

From the repository root:

```powershell
dotnet restore samples/Maliev.ShadcnBlazor.ThemeConsumer/Maliev.ShadcnBlazor.ThemeConsumer.csproj --locked-mode
dotnet run --project samples/Maliev.ShadcnBlazor.ThemeConsumer/Maliev.ShadcnBlazor.ThemeConsumer.csproj -c Release
```

The package-only path is exercised by `eng/Validate-ThemeConsumerPackage.ps1`.
It copies this sample to a physical temporary directory, restores exclusively
through the NuGet reference, repeats the restore in locked mode, and builds
without restore.

See the full [portable theme guide](../../docs/theming.md) for export, runtime
loading, provider registration, build diagnostics, migration, and offline font
behavior.
