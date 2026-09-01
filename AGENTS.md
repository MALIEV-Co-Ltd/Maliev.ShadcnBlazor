# AGENTS.md

This file defines the working agreement for coding agents and contributors in
the public `Maliev.ShadcnBlazor` repository. Keep changes portable, reviewable,
accessible, and safe for an open-source component library.

## Repository map

- `src/Maliev.ShadcnBlazor/` contains the Razor Class Library, public component
  APIs, theme services, JavaScript modules, and shipped CSS assets.
- `samples/Maliev.ShadcnBlazor.Showcase/` contains the interactive public
  documentation site deployed to GitHub Pages.
- `tests/Maliev.ShadcnBlazor.Tests/` contains component, contract, API snapshot,
  and documentation tests.
- `tests/Maliev.ShadcnBlazor.BrowserTests/` contains Playwright behavior,
  accessibility, responsive, and visual-evidence tests.
- `tests/Maliev.ShadcnBlazor.RepositoryTests/` protects packaging, repository,
  release, and public-boundary contracts.
- `docs/` contains contributor and consumer documentation. Evidence artifacts
  are reviewed records, not scratch output.
- `.github/workflows/` owns CI, GitHub Pages, and NuGet release automation.

## Skill routing

The repository ships two portable Agent Skills under `.agents/skills/`. Select
the workflow before editing:

- Use `$maliev-shadcnblazor` when the task is to install, configure, select, or
  compose the released package in a consuming Blazor application.
- Use `$maliev-shadcnblazor-maintainer` when the task changes this repository's
  components, public API, CSS, JavaScript, theming, documentation dossiers,
  catalog, tests, packaging, or release metadata.

When a supporting agent can discover repository-local skills, allow the
matching skill to load automatically. Otherwise read its `SKILL.md` directly.
Load only the references the skill routes to; do not treat every reference as
mandatory context.

A skill is reusable guidance, not authority. It does not add a tool to the
active session or grant permission to push, deploy, publish, alter external
state, or skip this working agreement. If a command or tool named by a skill is
not available, use an equivalent safe workflow and report the difference.

## Agent guidance synchronization

The agentic workflow shown on the documentation home page is a maintained
consumer contract, not decorative copy. When package registration, asset
loading, component names, public APIs, repository paths, or validation commands
change, review the affected guidance in the same change:

- this working agreement and `docs/agent-skills.md`;
- `.agents/skills/maliev-shadcnblazor/` for consumer integration;
- `.agents/skills/maliev-shadcnblazor-maintainer/` for repository work;
- the documentation site's agentic example and its contract tests.

Update only the guidance affected by the change, but do not leave examples that
name unavailable components or promise unverified behavior. Consumer guidance
must resolve APIs from the installed package version and official dossier;
maintainer guidance must stay aligned with the repository's actual boundaries
and validation commands. Run both skill validators and the agent-skill
repository tests whenever these instructions or skill packages change.

## Working rules

Inspect `git status` and the relevant component, tests, dossier, and public API
before editing. Preserve unrelated work. Make the smallest coherent change and
keep each commit buildable. Use `apply_patch` or another reviewable patching
mechanism; avoid broad mechanical rewrites.

Build before tests. For the affected projects, use the SDK selected by
`global.json` and run Release builds with zero warnings and zero errors before
counting test results. The complete local command sequence is documented in
`CONTRIBUTING.md`.

## Test-driven changes

For a feature or bug fix, first add a focused test that fails for the intended
reason. Implement the smallest production change that makes it pass, then run:

1. The affected project build.
2. Focused component or contract tests.
3. The full package test project.
4. Relevant Playwright browser tests.
5. Formatting, public-surface, and repository checks.

Never weaken a test, snapshot, accessibility assertion, or evidence contract
merely to make a change pass. Update a public API snapshot only for an
intentional reviewed API change.

## Component and API conventions

- Prefer native HTML semantics and strongly typed Blazor parameters and
  callbacks.
- Forward supported unmatched attributes without allowing callers to replace
  component-owned roles, IDs, state, or relationships.
- Keep controlled and uncontrolled state ownership explicit and deterministic.
- Preserve server rendering, hydration, disposal, and rerender behavior.
- Use logical CSS properties so LTR and RTL remain equivalent.
- Use semantic theme tokens; do not add application-specific styling or data
  models to the package.
- Add XML documentation for public APIs and keep examples compilable.

## Accessibility

Every interactive change must preserve native roles, names, keyboard behavior,
focus visibility, disabled and read-only behavior, error relationships, and
high-contrast usability. Test relevant states in light and dark themes, LTR and
RTL, reduced motion, forced colors, and zoom. A passing static render does not
replace a real-browser interaction test.

## Showcase documentation

Every public component belongs in the categorized navigation and needs a real
interactive dossier. A dossier should include a live preview, copyable package
installation and Razor usage, composition guidance, accessibility notes, API
reference, theming tokens, evidence, and source references when available.
Keep the desktop catalog/article/page-outline layout and its accessible drawer
behavior healthy at tablet and mobile widths.

## Evidence and visual baselines

Evidence must come from the real component and declared reference sources. Keep
source, adapter, dependency, screenshot, and computed-style hashes truthful.
Do not inject styles or state in a capture harness that mask production output.
Visual ratios are descriptive, not automatic approval.

Do not update baselines just because a visual test fails. Diagnose the cause,
use the opt-in baseline command documented in `docs/components.md`, verify that
only intended images changed, and inspect every changed image at original
resolution before committing it.

## Public safety

The repository and package are public. Never add credentials, tokens, customer
or employee data, private URLs, proprietary application names, unpublished
architecture, local absolute paths, private fixture data, or artifacts copied
from another product. Examples must use neutral fictional data. Run
`eng/Verify-PublicSurface.ps1` before handing off a change.

Treat copied upstream source and generated evidence as third-party material:
retain attribution, pin versions and commit hashes, and update
`THIRD-PARTY-NOTICES.md` when the dependency or source boundary changes.

## Git and review

Use focused commits that describe the user-visible outcome. Do not stage or
reformat unrelated files. Report the exact builds, tests, browser gates, and
static checks that ran, including pass counts and any remaining risk.

Do not push, open or merge a pull request, deploy GitHub Pages, alter repository
settings, or publish packages unless the repository owner explicitly authorizes
that external action.

## Release boundaries

Version and package metadata must remain consistent across the project,
documentation, and release tag. Do not publish a NuGet package manually. The
release workflow owns packing, provenance, and NuGet publication from an
approved version release. Never print or persist publishing credentials, and
never weaken protected environments or release gates to bypass a failure.
