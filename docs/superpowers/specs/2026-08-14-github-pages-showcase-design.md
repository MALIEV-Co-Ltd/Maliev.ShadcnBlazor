# GitHub Pages Showcase Design

## Objective

Publish the existing `Maliev.ShadcnBlazor.Showcase` application as the public,
interactive component demo at:

`https://maliev-co-ltd.github.io/Maliev.ShadcnBlazor/`

The site must let prospective users explore every cataloged component before
installing the NuGet package and must provide maintainers with reproducible
visual proof for changes.

## Scope

The public site will reuse the existing Blazor WebAssembly Showcase and its
component documentation catalog. It will not introduce a second documentation
application or duplicate component examples.

The deliverable includes:

- A GitHub Pages deployment workflow for the Showcase.
- Repository-path-aware Blazor WebAssembly asset and router configuration.
- Direct navigation and browser refresh support for nested documentation URLs.
- A discoverable live-demo link in the README and repository metadata.
- A visual-proof workflow that exercises every completed catalog component.
- Reviewable screenshot and diff artifacts for pull requests and manual runs.
- Deployment and visual-proof documentation for contributors.

The deliverable excludes custom domains, analytics, authentication, server-side
APIs, telemetry, external databases, and silent visual-baseline regeneration.

## Architecture

### Live application

`samples/Maliev.ShadcnBlazor.Showcase` remains the single source of interactive
documentation. The application is published as static Blazor WebAssembly output.
The Pages build supplies the repository base path `/Maliev.ShadcnBlazor/` while
local development continues to use `/`.

The published artifact will contain:

- The Release output from `dotnet publish`.
- A `.nojekyll` marker so GitHub Pages serves Blazor's underscore-prefixed
  framework and static-web-asset paths unchanged.
- A `404.html` fallback derived from the application entry document so direct
  requests to client-side routes return the Blazor shell.

The existing component catalog remains authoritative for which components are
shown. Demo pages must use public package APIs and deterministic public fixture
data only.

### Deployment workflow

`.github/workflows/pages.yml` will run on pushes to `main` and by manual dispatch.
It will:

1. Check out the exact commit.
2. Install the repository-pinned .NET SDK and restore locked dependencies.
3. Build and test the repository targets needed to trust the demo.
4. Publish the Showcase in Release mode.
5. configure the Pages base path and routing fallback.
6. Upload one Pages artifact.
7. Deploy through the protected `github-pages` environment.

The build job receives `contents: read`. Only the deployment job receives
`pages: write` and `id-token: write`. The workflow does not publish NuGet
packages, modify branches, or use repository secrets.

GitHub Pages must be configured to use GitHub Actions as its publishing source.

### Visual proof

The existing browser-test infrastructure will be extended rather than replaced.
A deterministic visual-proof test will enumerate every completed component in
`docs/component-catalog.json` and verify that each component has a reachable,
interactive dossier.

For each component, the proof matrix will capture representative states across:

- Light and dark themes.
- Desktop and mobile viewports.
- LTR and RTL direction where direction changes layout or interaction.
- Component-specific focused, selected, open, loading, error, or disabled states
  where those states are part of the dossier.

The workflow will run on pull requests, pushes to `main`, and manual dispatch.
It will upload actual screenshots and diffs even when comparison fails. Reviewed
baselines remain committed source artifacts. Updating them requires an explicit
opt-in command or environment switch and a separate reviewed commit; ordinary CI
cannot rewrite or approve them.

The visual-proof workflow validates presentation. Existing component, browser,
accessibility, interaction, and package tests remain the behavioral authority.

## User experience

The repository README will expose a prominent `Live demo` link near installation
instructions. The Showcase home page and catalog will make all completed
components searchable and navigable. Every dossier will retain its live controls,
API ownership notes, accessibility guidance, and source links.

A user opening or refreshing a nested URL such as
`/Maliev.ShadcnBlazor/docs/components/button` must return to the same dossier.
Navigation and static assets must remain inside the repository Pages prefix.

## Safety and privacy

The deployed application is a static public artifact. It must contain no secrets,
credentials, private endpoints, internal hostnames, customer data, or proprietary
application identifiers. Examples use deterministic fictional data and make no
network requests except for downloading the site's own static assets.

Dependency restoration uses the committed lock files. GitHub Actions are pinned
to immutable commit SHAs. Deployment permissions are scoped to Pages and the
`github-pages` environment.

## Failure handling

- Build, test, catalog coverage, or visual-proof failures prevent deployment.
- Missing component dossiers fail the visual-proof test with the component slug.
- Screenshot mismatches preserve actual and diff artifacts for diagnosis.
- A Pages deployment failure leaves the previously deployed site intact.
- Direct-route fallback is covered by an automated check of the published
  artifact and a post-deployment smoke test against the public URL.

## Validation and acceptance

Implementation is accepted when all of the following are true:

1. The repository build and existing test suites remain green.
2. The Showcase publishes successfully with the repository base path.
3. The published artifact contains `.nojekyll`, `404.html`, and valid Blazor
   framework/static-web-asset paths.
4. Every completed catalog component has an interactive public dossier.
5. The visual-proof matrix passes against reviewed baselines and uploads its
   evidence artifact.
6. The Pages workflow deploys from `main` using the `github-pages` environment.
7. The public root and at least one nested dossier URL load successfully over
   HTTPS.
8. The README and repository website metadata point to the live demo.
9. A repository scan finds no private identifiers, credentials, or internal URLs
   in the published artifact.

## Commit boundaries

Implementation should remain independently reviewable:

1. Pages-safe Showcase routing and artifact tests.
2. Complete-catalog visual-proof coverage and evidence handling.
3. GitHub Pages workflow and contributor documentation.
4. Repository metadata activation and verified public deployment.

