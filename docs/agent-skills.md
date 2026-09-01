# Agent Skills

Maliev.ShadcnBlazor ships reusable instructions for agents that need to consume
or maintain the library. The packages follow the `SKILL.md` convention and live
under `.agents/skills/`, so compatible agents can discover them in a clone or
install them from GitHub.

## Available packages

| Skill | Use it for | Do not use it for |
| --- | --- | --- |
| `$maliev-shadcnblazor` | Installing, configuring, selecting, composing, theming, or diagnosing the released package in an application | Editing the library repository |
| `$maliev-shadcnblazor-maintainer` | Changing components, API, CSS, JavaScript, catalog, Showcase dossiers, tests, packaging, or release metadata in this repository | Ordinary consumer application work |

Both skills allow implicit invocation when the agent supports it. Naming the
skill explicitly is useful when a request could otherwise be mistaken for a
generic Blazor or React shadcn/ui task.

## Install with the skills CLI

List the packages before installing:

```bash
npx skills add MALIEV-Co-Ltd/Maliev.ShadcnBlazor --list
```

Install both into the current project:

```bash
npx skills add MALIEV-Co-Ltd/Maliev.ShadcnBlazor \
  --skill maliev-shadcnblazor \
  --skill maliev-shadcnblazor-maintainer
```

Install only the consumer skill globally for Codex:

```bash
npx skills add MALIEV-Co-Ltd/Maliev.ShadcnBlazor \
  --skill maliev-shadcnblazor \
  --agent codex \
  --global
```

The CLI's default project installation is preferable for a repository team
because the selected skills and lock information can be reviewed with the
project. Use a global install when the same skill should be available across
unrelated repositories.

## Application AGENTS.md

Installing a skill makes it discoverable; an application-level `AGENTS.md`
defines when the team expects agents to use it. Add a focused rule to the
consuming repository and preserve any stricter rules already there:

```markdown
## Maliev.ShadcnBlazor UI work

- Use `$maliev-shadcnblazor` for package installation, component selection,
  composition, theming, and consumer-side diagnosis.
- Inspect the installed package version and existing app shell before editing.
- Confirm public parameters from the installed assembly or official dossier;
  do not infer React shadcn/ui or MudBlazor APIs.
- Prefer public package components and semantic tokens over copied Showcase
  markup, private selectors, or replacement JavaScript.
- Build before tests and verify the affected keyboard, focus, validation,
  responsive, theme, RTL, reduced-motion, and forced-color states.
```

The consumer skill's
[`agentic-integration.md`](../.agents/skills/maliev-shadcnblazor/references/agentic-integration.md)
reference expands this contract into the sidebar, financial-chart, and
validated-form workflows demonstrated on the documentation home page.

## Install with Codex's skill installer

Codex installations that include the system `skill-installer` can install
directly from the repository paths:

```text
Install these skills from MALIEV-Co-Ltd/Maliev.ShadcnBlazor:
- .agents/skills/maliev-shadcnblazor
- .agents/skills/maliev-shadcnblazor-maintainer
```

The installer places each selected directory under the user's Codex skills
directory. Start a new agent turn after installation so it can discover the new
packages.

## Example prompts

```text
Use $maliev-shadcnblazor to add an accessible order-status dialog to this
Blazor application and verify keyboard focus return.
```

```text
Use $maliev-shadcnblazor-maintainer to add a component parameter with unit,
catalog, dossier, API-snapshot, and browser coverage.
```

## What the packages guarantee

The consumer skill routes agents through package registration, static assets,
real API discovery, typed composition, theming, accessibility, and consumer
verification. The maintainer skill routes agents across implementation, CSS,
interop, public API, catalog, dossiers, unit tests, repository tests, and
Playwright evidence.

Skills provide instructions, not capabilities or authorization. They do not
create unavailable commands, supply credentials, permit publishing, or replace
the repository's `AGENTS.md`. The repository agreement remains authoritative
for all contributions.

## Maintaining the packages

When package setup, repository paths, validation commands, or public boundaries
change, update the matching skill and its references in the same pull request.
Validate every skill directory before committing:

```bash
python <skill-creator>/scripts/quick_validate.py .agents/skills/maliev-shadcnblazor
python <skill-creator>/scripts/quick_validate.py .agents/skills/maliev-shadcnblazor-maintainer
npx skills add . --list
```

`<skill-creator>` is the local path of the installed skill-creator system skill;
it varies by agent installation. The final command verifies repository-level
discovery through the open skills CLI.
