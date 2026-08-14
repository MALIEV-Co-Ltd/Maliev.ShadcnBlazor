# Contributing

Thank you for helping improve Maliev.ShadcnBlazor.

## Prerequisites

- The .NET SDK version selected by `global.json`.
- PowerShell 7 for repository verification scripts.
- Chromium installed through Playwright for browser tests.

## Local validation

Run the build before the tests:

```powershell
dotnet restore Maliev.ShadcnBlazor.slnx
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
pwsh tests/Maliev.ShadcnBlazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build
```

Before opening a pull request, also run:

```powershell
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
```

## Component changes

- Preserve native HTML semantics and keyboard behavior.
- Add focused unit tests and real-browser coverage for behavior changes.
- Cover light and dark themes, LTR and RTL direction, reduced motion, forced
  colors, and zoom where the component renders or behaves differently.
- Update the public API snapshot when a reviewed API change is intentional.
- Add or update a Showcase dossier so the behavior can be inspected manually.
- Avoid application-specific copy, routes, data models, and styling.

## Pull requests

Keep each pull request focused and describe the user-visible outcome. Include
the validation commands and results. Never commit credentials, tokens,
customer data, private URLs, build output, or local absolute paths.

By participating, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).
