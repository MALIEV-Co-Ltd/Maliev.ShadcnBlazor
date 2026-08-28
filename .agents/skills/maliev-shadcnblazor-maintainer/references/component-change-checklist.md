# Component change checklist

Use the lanes that match the affected boundary. A CSS-only file is not a
documentation-only change when it changes the rendered package.

## Trace the slice

| Boundary | Inspect | Update when applicable |
| --- | --- | --- |
| Implementation | `src/Maliev.ShadcnBlazor/Components/` | Razor/C# and XML documentation |
| Styling | `src/Maliev.ShadcnBlazor/wwwroot/css/` | Correct family layer, tokens, logical properties |
| Interop | `src/Maliev.ShadcnBlazor/wwwroot/js/` | Initialization, disposal, rerender, failure behavior |
| Public API | API approval snapshots under tests | Intentional additions or reviewed breaking changes only |
| Catalog | `docs/component-catalog.json` | Canonical slug, category, status, API, evidence |
| Documentation | `samples/Maliev.ShadcnBlazor.Showcase/Documentation/` | Dossier, copyable Razor, API, accessibility, theming |
| Unit/contract | `tests/Maliev.ShadcnBlazor.Tests/` | Focused behavior and contract regression |
| Repository | `tests/Maliev.ShadcnBlazor.RepositoryTests/` | Packaging, catalog, public-boundary regression |
| Browser | `tests/Maliev.ShadcnBlazor.BrowserTests/` | Keyboard, focus, responsive, accessibility, visual evidence |

Use `rg` to locate the exact component name and catalog slug instead of
assuming filenames. Some documented components are compositions rather than a
single Razor file.

## Validation lanes

### Public component behavior or API

```powershell
dotnet restore Maliev.ShadcnBlazor.slnx
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build
```

Run the focused test filter first when one exists, then the complete affected
projects. Add the relevant Playwright filter for interactive, responsive,
accessibility, CSS, overlay, or visual changes.

### Documentation or repository workflow only

Build/test are not automatically useful when no compiled input changed. Run
the strongest structural checks available: skill or schema validation, JSON or
YAML parsing, link/path readback, repository tests covering the document, and
public-surface verification. State why a .NET build is not applicable.

### Before commit

```powershell
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
pwsh -NoProfile -File eng/Verify-PublicSurface.ps1 -Root .
git diff --check
git diff --cached --name-only
```

Do not run `git add .` in a dirty worktree. Stage only the files belonging to
the validated slice and review the staged diff before committing.
