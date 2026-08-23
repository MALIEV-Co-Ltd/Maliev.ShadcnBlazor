# Theme Studio Curated Runway Design

## Status

Approved by the repository owner on 2026-08-23. Tracking issue: [#227](https://github.com/MALIEV-Co-Ltd/Maliev.ShadcnBlazor/issues/227).

Approved visual references:

- `.impeccable/mocks/theme-studio-approved-direction-desktop.png`
- `.impeccable/mocks/theme-studio-approved-direction-mobile.png`

The generated labels, dates, people, and values in those comps are illustrative. Production examples use reviewed fictional Thai manufacturing content and real repository version data.

## Product decision

Theme Studio is not a dashboard and not a second component catalog. Its primary preview is one fixed deck of realistic use-case cards. Every card composes multiple public `Maliev.ShadcnBlazor` components into a coherent workflow. Shuffle changes only a complete curated theme preset; card identity, ordering, copy, and local interaction state remain stable so users can compare themes reliably.

The existing exhaustive component-scenario matrix remains an independent QA surface and is not embedded in the primary Theme Studio journey.

## Information architecture

### Universal app bar

Theme Studio consumes the documentation app bar rather than defining its own header. The app bar owns global light/dark mode and LTR/RTL direction. Its navigation, brand, GitHub link, responsive menu, theme toggle, and direction toggle are shared across documentation and Theme Studio.

The direction action uses a clear bidirectional-text icon from the selected icon boundary and retains an explicit accessible name describing the next state.

### Theme Sidebar

The control rail is built from the public Sidebar composition. Its controls are in-page buttons or disclosure sections; none navigate to component documentation.

Order:

1. Preview device
2. Curated preset
3. Typography
4. Icon library
5. Accessibility and motion
6. Import/export

The raw light/dark semantic-token editors leave the primary workflow. Advanced diagnostics may still report validation and contrast results, but users choose colors through reviewed presets.

Typography and icon-library selection apply only to the preview scope. The documentation chrome retains its own stable font and icon treatment.

### Device controls

- Desktop host: Desktop, Tablet, and Mobile choices.
- Tablet host: Tablet and Mobile choices.
- Mobile host: no device selector; the preview is always mobile.

Device choices live in the Sidebar and never consume preview canvas height.

## Preview topology

### Desktop and fitting landscape tablet

The preview contains two equal-width clipped columns. Cards are distributed deterministically between tracks and preserve their order.

- Left track moves slowly toward block-end.
- Right track moves slowly toward block-start.
- Both tracks loop seamlessly.
- The visual edge treatment masks card content only and never obscures controls outside the runway.

The loop uses one logical interactive sequence and one inert, `aria-hidden` mirror per track. Mirror content cannot receive focus, pointer events, form submission, live-region output, generated IDs, or accessibility-tree exposure. Shared state keeps visible animation frames coherent without creating duplicate announcements.

### Portrait tablet and mobile

The preview becomes one normal document-flow column. It does not auto-scroll. Users scroll naturally, and the preset dock respects safe-area insets.

### Pause behavior

Automatic track motion pauses immediately on:

- pointer enter or pointer down;
- wheel or touch interaction;
- focus entering any preview control;
- keyboard interaction within the preview;
- document visibility loss;
- the persistent Pause control;
- reduced-motion preference or Theme Studio reduced-motion setting.

Temporary pauses resume after a calm inactivity interval without jumping position. Persistent Pause remains until explicitly resumed. Focused content never resumes underneath the user.

## Curated use-case deck

The initial deck contains at least ten independently stateful cards, split across the two tracks:

1. Production capacity and utilization
2. Operator profile and contact settings
3. Quotation files and Dropzone progress
4. Shipping and production handoff address
5. Inspection notification preferences
6. Manufacturing deposit or approval summary
7. Assigned reviewers and contributors
8. Work-order navigation and recurring operations
9. Machine-cell status and live inspection feed
10. Assistant conversation with bounded Message Scroller
11. Issue report and validation states
12. Confirmation or destructive-action workflow

Cards use actual package components and neutral fictional Thai manufacturing data. They are not wrappers around dossier previews. Each card owns an explicit state model and reset contract.

## Component demonstration timeline

A central deterministic timeline advances card state in staggered phases so the runway remains legible. Representative demonstrations include:

- progress values advancing and completing;
- a Dropzone upload entering progress, success, and reset states;
- form fields filling, validating, and returning to a neutral state;
- chat text streaming while auto-follow remains at the end;
- notification or selection state changing;
- a toast entering and settling;
- an approval or export state transitioning.

Animations use real controlled component parameters and callbacks, not decorative CSS imitations. Only a small number of cards animate simultaneously. Pausing the runway also pauses the timeline. Reduced motion replaces interpolated movement with stable end-state changes or a static representative state.

## Curated preset generator

The generator chooses only from reviewed, internally compatible preset definitions. A preset materializes:

- style family;
- neutral base;
- primary and accent palette;
- radius;
- density and spacing;
- border strength;
- surface elevation and shadow treatment;
- input and selection treatment;
- navigation treatment;
- motion profile;
- icon library identifier.

Shuffle selects another preset while preserving card order and local card state. It never generates arbitrary token values. Locked supported values remain exact. Undo/redo, share, JSON, CSS, and bundle export reproduce the visible preset.

## Icon libraries

The core package gains a small shared strongly typed icon rendering contract. Full free upstream catalogs ship in optional companion packages so consumers pay only for the library they select:

- `Maliev.ShadcnBlazor.Icons.Lucide`
- `Maliev.ShadcnBlazor.Icons.Tabler`
- `Maliev.ShadcnBlazor.Icons.Phosphor`
- `Maliev.ShadcnBlazor.Icons.Hugeicons`

The repositories, versions or immutable commits, source hashes, and license texts are pinned. Lucide uses ISC plus any applicable inherited notices. Tabler and Phosphor use MIT. Only Hugeicons Free MIT assets are accepted; Pro assets are forbidden.

Generated icon data must reject scripts, event handlers, external URLs, style injection, foreign objects, malformed view boxes, and unsupported SVG elements or attributes. Package archives contain the applicable licenses and third-party notices. Public API, archive inventory, clean installed consumers, trimming, and deterministic regeneration are tested.

Theme Studio references all four companion packages so users can compare and inspect them. Exported consumption guidance names the exact optional package matching the selected library.

## CodeBlock supporting correction

The toolbar uses this stable layout:

`[compact language selector] [flexible spacer] [fixed-size copy action]`

The redundant static language label is removed when the selector is present. Copy success changes icon/state without changing the button's inline size, toolbar height, or surrounding code geometry. Keyboard, repeated copy, fallback, mobile, RTL, and 200% zoom remain covered.

## Message Scroller containment

The transcript viewport and composer become sibling regions inside the scroller shell:

1. bounded transcript viewport;
2. message content within that viewport;
3. non-scrolling composer below it.

Messages, avatars, bubbles, headers, actions, and statuses cannot cross the composer boundary. The bottom fade belongs to an inner message-content overlay, is inset away from the scrollbar gutter, and never covers the scrollbar or composer. Auto-follow, user-intent pause, unread state, streaming growth, and return-to-end behavior continue within the reduced viewport.

## Accessibility and responsive contract

- Visible Pause/Resume satisfies continuous-motion control requirements.
- Keyboard focus pauses motion and remains visually stable.
- Loop mirrors are inert and absent from the accessibility tree.
- Live demonstrations do not produce duplicate announcements.
- Forced colors preserves card, focus, control, and dock boundaries.
- At 200% zoom, the Sidebar and preview remain operable without document-level horizontal overflow.
- RTL reverses logical layout where appropriate but does not reverse numeric identifiers or code.
- Thai content loads independent of browser locale.
- Mobile targets are at least 44 CSS pixels where coarse input applies.

## Performance contract

Card state is lightweight and centrally scheduled. Offscreen work is suspended where possible. Only the approved card deck mounts; the 198-scenario QA registry is not mounted in Theme Studio. Repeated visual mirrors cannot create timers, observers, network requests, or independent Blazor state machines.

## Validation strategy

Implementation proceeds RED to GREEN with:

- state and registry unit tests for fixed ordering, preset-only Shuffle, timeline pause, and reset;
- Sidebar/app-bar component contracts;
- icon parser, sanitizer, license, source-hash, archive, public API, and installed-consumer tests;
- CodeBlock geometry and copy lifecycle tests;
- Message Scroller DOM and geometry regressions;
- browser tests for counter-scroll direction, seamless reset, pause/resume, focus, wheel, touch, visibility, reduced motion, and mobile static flow;
- browser assertions for preview-scoped typography and icon selection;
- Axe, keyboard, RTL, Thai, forced-colors, 200% zoom, tablet, and mobile coverage;
- reviewed Theme Studio desktop-light, tablet-dark-RTL, and mobile-forced-colors proofs;
- full package, repository, browser, formatting, public-surface, pack, and clean-consumer gates.

## Deliberate exclusions

- No arbitrary color randomization.
- No card-order randomization.
- No primary component-catalog matrix inside Theme Studio.
- No automatic runway motion on mobile or under reduced motion.
- No Hugeicons Pro assets.
- No runtime network fetch for icons, fonts, presets, or card definitions.
- No styling changes outside the preview when preview typography or icon library changes.
