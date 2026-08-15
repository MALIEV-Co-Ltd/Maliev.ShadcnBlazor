# Shadcn-Style Documentation Redesign

## Objective

Replace the current workbench-like Showcase documentation with a public documentation experience that follows the information architecture of Shadcn's component pages: persistent component navigation on the left, a focused documentation article in the center, and an on-page section outline on wide screens. Every component page must demonstrate the live Blazor component and explain how to install, compose, configure, and consume it.

The redesign is scoped to the public Showcase and contributor guidance. It does not change component behavior or public package APIs.

## Reference and design mode

The reference is `https://ui.shadcn.com/docs/components/base/accordion`. The implementation uses its proven documentation grammar rather than copying its branding or incidental pixels:

- persistent categorized component navigation;
- a narrow, readable article column;
- live examples paired with copyable source;
- installation, usage, composition, variants, accessibility, and API sections;
- a sticky “On This Page” outline;
- responsive navigation drawers on smaller screens.

The surface operates in **Read** mode. Comprehension, navigation, and trustworthy code examples take precedence over decorative expression.

## Desktop information architecture

The documentation shell has a sticky global header followed by a three-column grid:

1. **Component sidebar** — `15rem–17rem`, sticky below the header, independently scrollable, and grouped by catalog category. It contains a compact search field, category headings, all 64 component links, and an active-page marker. Status badges and category/status filter selects are removed from the primary navigation because every current component is complete and the Shadcn-style grouped list is faster to scan.
2. **Documentation article** — a centered readable column with a maximum text width near `48rem`. Wide examples and API tables may use the full article width, but prose retains a comfortable line length.
3. **On This Page** — `12rem–14rem`, sticky and generated from the sections actually rendered for the dossier. It links to Overview, Preview, Installation, Usage, Composition, Accessibility, API, Theming, Evidence, and References when those sections exist.

The existing theme and direction controls move into the global header. The current right-side theme drawer no longer occupies a permanent desktop column.

## Responsive behavior

- **Above 80rem:** full three-column layout.
- **48rem–80rem:** component sidebar plus article; “On This Page” is available from a compact article-outline control.
- **Below 48rem:** article-first single column. Components and article outline open as modal drawers with focus restoration, Escape handling, backdrop dismissal, and scroll containment.
- RTL mirrors sidebar/drawer placement and preserves logical reading order.
- No breakpoint may introduce horizontal document overflow. Wide code and API tables scroll within their own containers.

## Component dossier content

Each component dossier renders these sections in order:

1. **Overview** — component name, concise summary, primary Blazor type, category, classification, and previous/next component links.
2. **Interactive examples** — each existing `ComponentExampleDefinition` renders a live preview with its independently mutable controls. Its copyable Razor source sits directly beneath the corresponding preview.
3. **Installation** — a copyable `dotnet add package Maliev.ShadcnBlazor` command and the public stylesheet/import requirements already defined by the package README.
4. **Usage** — the component namespace and the first reviewed Razor example. Content comes from catalog/API/example metadata; no hand-written code sample may drift from the executable preview.
5. **Composition** — the public component types that belong to the dossier, displayed as a readable composition tree derived from API descriptors.
6. **Accessibility** — the existing component-specific notes plus the cross-cutting keyboard, RTL, zoom, forced-color, and reduced-motion expectations.
7. **API Reference** — the existing descriptor tables.
8. **Theming** — the component token groups and a link to the live Theme Studio.
9. **Evidence and references** — the existing certification matrix, pinned source links, and current official documentation link.

Every section receives a stable heading ID and anchor link. Copy controls provide a polite live-region confirmation without moving focus.

## Navigation components and data flow

- `DocumentationLayout.razor` owns shell landmarks, responsive drawer state, focus restoration, and the three-column arrangement.
- `DocumentationHeader.razor` owns branding, component-menu trigger, outline trigger, theme toggle, and direction toggle.
- `DocumentationCatalogRail.razor` consumes `IComponentDocumentationCatalog`, groups entries by `Category`, filters groups by the normalized search query, and marks the current base-relative route.
- A new `DocumentationOnThisPage.razor` consumes an immutable list of section records and renders only available sections.
- A new `ComponentConsumptionGuide.razor` renders installation, usage, composition, and theming from the current dossier's catalog entry, examples, and API descriptors.
- `ComponentDocumentation.razor` composes the article and supplies its section list to the layout through a scoped documentation-page state service. The state is cleared when leaving a dossier so stale outline links never remain.

## Visual language

The site uses the existing Shadcn semantic tokens, neutral palette, radius system, and theme provider. The redesign removes dashboard-like panels and oversized hero typography in favor of documentation conventions:

- compact 3.5rem header;
- understated borders and active navigation background;
- `2.25rem–2.75rem` component heading rather than a marketing hero;
- prose line-height near `1.7`;
- examples presented as clean bordered canvases with controls and source directly attached;
- monospace only for code, commands, namespaces, and API identifiers;
- no gradients, glass panels, decorative cards, or invented marketing content.

## Accessibility and failure states

- Landmarks are named and skip links target component navigation and article content.
- Active links use `aria-current="page"`.
- Drawer triggers expose `aria-controls` and `aria-expanded`; drawers restore focus to their trigger.
- The categorized sidebar remains a single navigation landmark with nested headings and lists.
- Empty search results announce “No components found” and provide a clear-search action.
- Unknown component slugs preserve the semantic Empty component and link back to the catalog.
- All interactive controls retain visible focus indicators in light, dark, and forced-color modes.
- Reduced motion removes drawer transitions without changing visibility or focus behavior.

## Public `AGENTS.md`

Add a root `AGENTS.md` written for public contributors and coding agents. It must:

- describe the `src`, `samples`, `tests`, `docs`, `eng`, and workflow boundaries;
- require inspection of the working tree and preservation of unrelated changes;
- require build-first, focused-test, relevant-suite, format, public-surface, and browser validation;
- require TDD for behavior changes and matching dossier/evidence updates for component changes;
- preserve native semantics, accessibility, RTL, themes, reduced motion, forced colors, and zoom;
- prohibit secrets, private URLs, customer data, private package feeds, private application dependencies, and application-specific routes or DTOs;
- document coherent commit and pull-request expectations;
- document the NuGet release and GitHub Pages workflows without granting permission to publish or deploy implicitly.

`AGENTS.md`, `CONTRIBUTING.md`, and `SECURITY.md` must agree. A repository contract test locks required public-safety and validation clauses.

## Testing and acceptance

### Contract and component tests

- categorized sidebar contains all 64 unique catalog entries and one active link;
- search filters grouped links and announces zero/one/many results;
- every complete dossier exposes the required stable section IDs;
- installation and usage samples are derived from current package/example metadata;
- “On This Page” contains only rendered section IDs;
- public `AGENTS.md` contains the required validation and safety boundaries;
- no repository-root-relative Showcase links regress GitHub Pages routing.

### Real-browser tests

- desktop at `1440×900`: sidebar and on-page outline are simultaneously visible and sticky; article remains readable with no horizontal overflow;
- tablet at `768×1024`: sidebar remains available and outline collapses;
- mobile at `390×844` and `320×568`: component and outline drawers open, trap the intended interaction area, close with Escape/backdrop, and restore focus;
- keyboard users can search, navigate to another dossier, operate the preview, copy source, and traverse section anchors;
- direct GitHub Pages dossier URLs remain repository-scoped;
- light/LTR, dark/RTL, reduced motion, forced colors, and 200% zoom remain healthy;
- Axe reports no serious or critical violations on representative dossiers.

### Visual proof

Regenerate the documentation screenshots for representative foundation, form, overlay, data, and conversation components at desktop light and mobile dark/RTL. Inspect desktop and mobile together once, fix findings in one batch, and run one confirmation pass.

## Delivery boundaries

Deliver the redesign as coherent commits:

1. approved design/product records;
2. documentation shell and navigation behavior;
3. dossier consumption content and section outline;
4. public `AGENTS.md` and repository contracts;
5. browser/visual proof and final documentation updates.

The final pull request must pass the complete repository, component, browser, public-surface, formatting, security, and Pages gates before administrator merge and live-site verification.
