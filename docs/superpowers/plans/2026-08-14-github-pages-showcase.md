# GitHub Pages Showcase Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publish the existing interactive component Showcase at `https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/` and add complete-catalog visual proof with explicit baseline review.

**Architecture:** Keep the Blazor WebAssembly Showcase as the only documentation application. Prepare its static publish output with a repository base path, `.nojekyll`, and an SPA fallback, then deploy it through GitHub's Pages artifact flow. Extend the existing Playwright/xUnit infrastructure to enumerate the certified component catalog, capture deterministic dossiers, compare reviewed baselines, and upload diagnostics without allowing CI to rewrite them.

**Tech Stack:** .NET 10, Blazor WebAssembly, xUnit, Microsoft Playwright, PowerShell 7, GitHub Actions, GitHub Pages.

## Global Constraints

- Public URL: `https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/`.
- The demo is static and contains no credentials, telemetry, private endpoints, customer data, or internal identifiers.
- Local development keeps `<base href="/" />`; published Pages output uses `<base href="/Maliev.ShadcnBlazor/" />`.
- GitHub Actions remain pinned to immutable commit SHAs.
- The Pages build has `contents: read`; only deployment has `pages: write` and `id-token: write`.
- NuGet publishing remains exclusively owned by `.github/workflows/release.yml`.
- Visual baselines can change only with `SHADCN_UPDATE_VISUAL_BASELINES=1`; ordinary CI is read-only.
- Deployment is blocked by build, test, artifact, catalog, or visual-proof failure.

---

## File Structure

- `eng/Prepare-GitHubPages.ps1`: transform and validate a published Showcase artifact without modifying source files.
- `tests/Maliev.ShadcnBlazor.RepositoryTests/GitHubPagesTests.cs`: lock the Pages workflow, documentation, and artifact-preparation contract.
- `tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs`: enumerate completed catalog entries and capture deterministic dossier canvases.
- `tests/Maliev.ShadcnBlazor.BrowserTests/Infrastructure/VisualProof.cs`: shared image comparison, baseline update guard, and diagnostic output paths.
- `docs/evidence/component-catalog-baselines/`: reviewed desktop-light and mobile-dark-RTL dossier images.
- `.github/workflows/pages.yml`: build, test, prepare, upload, and deploy the static Showcase.
- `.github/workflows/visual-proof.yml`: run catalog proof and upload screenshots/diffs without deployment.
- `README.md`: expose the live demo beside installation guidance.
- `docs/components.md`: explain interactive dossiers and visual-proof review.

---

### Task 1: Pages-safe static artifact

**Files:**
- Create: `eng/Prepare-GitHubPages.ps1`
- Create: `tests/Maliev.ShadcnBlazor.RepositoryTests/GitHubPagesTests.cs`

**Interfaces:**
- Consumes: Blazor publish directory containing `index.html`.
- Produces: `Prepare-GitHubPages.ps1 -PublishDirectory <path> -BasePath /Maliev.ShadcnBlazor/`, returning a Pages-ready directory with validated `index.html`, `404.html`, and `.nojekyll`.

- [ ] **Step 1: Write the failing repository tests**

Add a test that copies a minimal published artifact to a temporary directory,
executes the real PowerShell script, and asserts its observable outputs:

```csharp
[Fact]
public void PagesPreparationIsRepositoryScopedAndCreatesSpaFallback()
{
    using var fixture = GitHubPagesFixture.Create();
    var result = fixture.RunPreparation("/Maliev.ShadcnBlazor/");
    Assert.Equal(0, result.ExitCode);
    Assert.Contains("<base href=\"/Maliev.ShadcnBlazor/\" />", fixture.Read("index.html"), StringComparison.Ordinal);
    Assert.Equal(fixture.Read("index.html"), fixture.Read("404.html"));
    Assert.True(fixture.Exists(".nojekyll"));
    Assert.Equal(fixture.OriginalSourceIndex, fixture.ReadSourceIndex());
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter GitHubPagesTests
```

Expected: FAIL because `eng/Prepare-GitHubPages.ps1` is missing.

- [ ] **Step 3: Implement the artifact transformer**

The script must resolve the supplied directory, require `index.html` and `_framework`, normalize the base path to leading and trailing slashes, replace only `<base href="/" />`, copy the transformed entry document to `404.html`, create an empty `.nojekyll`, and fail if the transformed artifact still contains `<base href="/" />`.

Core transformation:

```powershell
$indexPath = Join-Path $resolvedPublishDirectory 'index.html'
$index = Get-Content -LiteralPath $indexPath -Raw
$expected = '<base href="/" />'
if (-not $index.Contains($expected, [StringComparison]::Ordinal)) {
    throw "Expected the local-development base element in $indexPath."
}
$index = $index.Replace($expected, "<base href=`"$BasePath`" />", [StringComparison]::Ordinal)
Set-Content -LiteralPath $indexPath -Value $index -NoNewline
Copy-Item -LiteralPath $indexPath -Destination (Join-Path $resolvedPublishDirectory '404.html') -Force
New-Item -ItemType File -Path (Join-Path $resolvedPublishDirectory '.nojekyll') -Force | Out-Null
```

- [ ] **Step 4: Build a temporary publish artifact and verify GREEN**

Run:

```powershell
dotnet publish samples/Maliev.ShadcnBlazor.Showcase/Maliev.ShadcnBlazor.Showcase.csproj -c Release --no-restore -o artifacts/pages-publish
./eng/Prepare-GitHubPages.ps1 -PublishDirectory artifacts/pages-publish/wwwroot -BasePath /Maliev.ShadcnBlazor/
Test-Path artifacts/pages-publish/wwwroot/.nojekyll
Test-Path artifacts/pages-publish/wwwroot/404.html
Select-String -Path artifacts/pages-publish/wwwroot/index.html -SimpleMatch '<base href="/Maliev.ShadcnBlazor/" />'
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter GitHubPagesTests
```

Expected: publish succeeds, both files exist, the base path matches, and the focused test passes.

- [ ] **Step 5: Commit the Pages artifact slice**

```powershell
git add eng/Prepare-GitHubPages.ps1 tests/Maliev.ShadcnBlazor.RepositoryTests/GitHubPagesTests.cs
git commit -m "feat: prepare showcase for GitHub Pages"
```

---

### Task 2: Complete-catalog visual proof

**Files:**
- Create: `tests/Maliev.ShadcnBlazor.BrowserTests/Infrastructure/VisualProof.cs`
- Create: `tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs`
- Create: `docs/evidence/component-catalog-baselines/*.png`

**Interfaces:**
- Consumes: `docs/component-catalog.json`, `ShowcaseServerFixture.BaseUri`, and each dossier's `component-preview-canvas` test id.
- Produces: `VisualProof.CompareOrUpdateAsync(string slug, string mode, byte[] actual)` and deterministic reviewed PNGs named `<slug>--desktop-light.png` and `<slug>--mobile-dark-rtl.png`.

- [ ] **Step 1: Write the failing catalog coverage test**

Parse the public catalog and require exactly two proof modes for every `Complete` entry:

```csharp
[Fact]
public void EveryCompletedCatalogEntryHasTwoReviewedBaselines()
{
    var catalog = ComponentCatalogProof.LoadCompleted(FindRoot());
    var baselineDirectory = Path.Combine(FindRoot(), "docs", "evidence", "component-catalog-baselines");
    foreach (var slug in catalog)
    {
        Assert.True(File.Exists(Path.Combine(baselineDirectory, $"{slug}--desktop-light.png")), $"Missing desktop proof for {slug}.");
        Assert.True(File.Exists(Path.Combine(baselineDirectory, $"{slug}--mobile-dark-rtl.png")), $"Missing mobile proof for {slug}.");
    }
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --filter ComponentCatalogVisualProofTests
```

Expected: FAIL because the proof helper and reviewed baselines do not exist.

- [ ] **Step 3: Implement deterministic dossier capture**

Use one fresh browser context per mode, navigate to `/docs/components/{slug}`, wait for `component-dossier`, assert the component is complete, disable animations through reduced-motion emulation, and capture `component-preview-canvas`. For dark/RTL, use the existing documentation theme and direction controls and assert the document state changed before capture.

Comparison must use the repository's existing pixel-comparison implementation and emit actual/diff files under `artifacts/visual-proof/<slug>/<mode>/`. Update committed baselines only when:

```csharp
var update = string.Equals(
    Environment.GetEnvironmentVariable("SHADCN_UPDATE_VISUAL_BASELINES"),
    "1",
    StringComparison.Ordinal);
```

Ordinary comparison must fail with the differing pixel count, ratio, and diagnostic path.

- [ ] **Step 4: Generate and manually inspect the initial proof set**

Run:

```powershell
$env:SHADCN_UPDATE_VISUAL_BASELINES='1'
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --filter ComponentCatalogVisualProofTests
Remove-Item Env:SHADCN_UPDATE_VISUAL_BASELINES
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --filter ComponentCatalogVisualProofTests
```

Expected: the opt-in run writes two images per completed slug; the ordinary rerun passes without modifying them. Inspect the contact sheet or each generated image at original resolution and reject blank, clipped, occluded, loading, or wrong-theme captures.

- [ ] **Step 5: Commit the visual-proof slice**

```powershell
git add tests/Maliev.ShadcnBlazor.BrowserTests/Infrastructure/VisualProof.cs tests/Maliev.ShadcnBlazor.BrowserTests/ComponentCatalogVisualProofTests.cs docs/evidence/component-catalog-baselines
git commit -m "test: add complete component visual proof"
```

---

### Task 3: Secure Pages and visual-proof workflows

**Files:**
- Create: `.github/workflows/pages.yml`
- Create: `.github/workflows/visual-proof.yml`
- Modify: `tests/Maliev.ShadcnBlazor.RepositoryTests/WorkflowSecurityTests.cs`

**Interfaces:**
- Consumes: `eng/Prepare-GitHubPages.ps1`, the Showcase project, and `ComponentCatalogVisualProofTests`.
- Produces: one GitHub Pages deployment artifact and one downloadable `component-visual-proof` diagnostics artifact.

- [ ] **Step 1: Extend workflow-security tests and verify RED**

Require both workflows, pinned action SHAs, workload restore, locked restore, Pages permissions isolated to deploy, no NuGet publishing commands, no secrets, and visual-proof artifact upload with `if: always()`.

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter WorkflowSecurityTests
```

Expected: FAIL because both workflows are missing.

- [ ] **Step 2: Add the Pages workflow**

Create separate `build` and `deploy` jobs. The build job runs on `ubuntu-24.04`, restores pinned workloads and locked packages, runs repository and component tests, publishes the Showcase, prepares the artifact, and uses pinned `actions/upload-pages-artifact`. The deploy job uses:

```yaml
permissions:
  contents: read
  pages: write
  id-token: write
environment:
  name: github-pages
  url: ${{ steps.deployment.outputs.page_url }}
needs: build
```

Use pinned `actions/deploy-pages` and set concurrency to one active Pages deployment without canceling an in-progress deployment.

- [ ] **Step 3: Add the visual-proof workflow**

Run on pull requests, pushes to `main`, and manual dispatch. Build the BrowserTests project on `windows-2022`, install Chromium through the generated Playwright script, run only `ComponentCatalogVisualProofTests`, and upload `artifacts/visual-proof/**` with `if: always()` and a seven-day retention. Do not set `SHADCN_UPDATE_VISUAL_BASELINES`.

- [ ] **Step 4: Validate workflow security and YAML structure**

Run:

```powershell
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter WorkflowSecurityTests
git diff --check
```

Expected: workflow tests pass and no whitespace errors remain.

- [ ] **Step 5: Commit the automation slice**

```powershell
git add .github/workflows/pages.yml .github/workflows/visual-proof.yml tests/Maliev.ShadcnBlazor.RepositoryTests/WorkflowSecurityTests.cs
git commit -m "ci: publish interactive showcase to Pages"
```

---

### Task 4: Public documentation and live deployment

**Files:**
- Modify: `README.md`
- Modify: `docs/components.md`
- Modify: `tests/Maliev.ShadcnBlazor.RepositoryTests/PublicDocumentationTests.cs`

**Interfaces:**
- Consumes: the stable Pages URL and workflow names.
- Produces: discoverable live-demo and contributor visual-proof instructions.

- [ ] **Step 1: Add failing documentation assertions**

Require the README to contain the exact live URL and `docs/components.md` to document `SHADCN_UPDATE_VISUAL_BASELINES=1`, its opt-in meaning, and CI's read-only behavior.

- [ ] **Step 2: Run the focused documentation test and verify RED**

```powershell
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --filter PublicDocumentationTests
```

Expected: FAIL because the live demo and visual-proof instructions are absent.

- [ ] **Step 3: Update public documentation**

Add a prominent README link:

```markdown
[Explore every component in the live interactive demo](https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/)
```

Document how contributors run visual proof normally and how they explicitly regenerate reviewed baselines. State that baseline regeneration must be inspected and committed separately.

- [ ] **Step 4: Run full local validation**

Run in order:

```powershell
dotnet workload restore Maliev.ShadcnBlazor.slnx
dotnet restore Maliev.ShadcnBlazor.slnx --locked-mode
dotnet build Maliev.ShadcnBlazor.slnx -c Release --no-restore
dotnet test tests/Maliev.ShadcnBlazor.RepositoryTests/Maliev.ShadcnBlazor.RepositoryTests.csproj -c Release --no-build --no-restore
dotnet test tests/Maliev.ShadcnBlazor.Tests/Maliev.ShadcnBlazor.Tests.csproj -c Release --no-build --no-restore
dotnet test tests/Maliev.ShadcnBlazor.BrowserTests/Maliev.ShadcnBlazor.BrowserTests.csproj -c Release --no-build --no-restore
dotnet format Maliev.ShadcnBlazor.slnx --verify-no-changes --no-restore
git diff --check
```

Expected: zero build warnings/errors, all suites pass, formatting is clean, and only intended files are changed.

- [ ] **Step 5: Commit documentation**

```powershell
git add README.md docs/components.md tests/Maliev.ShadcnBlazor.RepositoryTests/PublicDocumentationTests.cs
git commit -m "docs: link the public component showcase"
```

- [ ] **Step 6: Push and enable GitHub Pages**

Push the validated commits to `main`. Configure the repository Pages source to `workflow` if it is not already configured. Wait for the Pages workflow, visual-proof workflow, and required CI checks to complete.

- [ ] **Step 7: Verify the public boundary**

Read back repository Pages metadata and verify over HTTPS:

```text
https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/
https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/docs/components/button
```

Confirm the root and nested dossier return the Blazor application, the Button dossier is interactive, static assets load under `/Maliev.ShadcnBlazor/`, the README and repository website metadata link to the demo, and no internal identifiers or secrets appear in the published artifact.
