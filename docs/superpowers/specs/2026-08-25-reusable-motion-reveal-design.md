# Reusable Motion and Scroll Reveal Design

## Summary

`Maliev.ShadcnBlazor` will provide an opt-in motion layer that lets consumers reveal composed UI when it enters a scroll viewport and coordinate purposeful child animations. Theme Studio will consume this public API instead of owning a private `IntersectionObserver` implementation.

The system must preserve server-rendered visibility, component state, keyboard behavior, and reduced-motion accessibility. Theme changes and rerenders must update existing elements rather than recreate the preview.

## Goals

- Make scroll reveal a reusable library capability rather than Theme Studio-only behavior.
- Give consumers typed Blazor parameters for reveal timing, effect, threshold, staggering, pause, and one-time behavior.
- Coordinate component-aware animation for charts, progress, metrics, messages, status content, and form sections without blocking interaction.
- Keep animations smooth on desktop and mobile and deterministic during rerenders.
- Respect browser and application reduced-motion settings.
- Migrate the curated Theme Studio Bento showcase to the public motion API.

## Non-goals

- No animation framework dependency.
- No global automatic animation of every component instance.
- No layout-changing animation of dimensions, grid tracks, or document position.
- No replay caused by theme Shuffle, locale changes, or ordinary Blazor rerenders.
- No removal or replacement of component-specific interaction feedback already owned by individual components.

## Public component API

### `ShadcnRevealGroup`

`ShadcnRevealGroup` owns one observer and coordinates all descendant `ShadcnReveal` components.

Public parameters:

- `ChildContent`: rendered content.
- `Tag`: semantic wrapper element, default `div`, restricted to a reviewed safe set.
- `Threshold`: visible intersection ratio, default `0.08`.
- `RootMargin`: validated CSS margin, default `32px 0px`.
- `Stagger`: delay between sibling reveals, default `60ms`, with a bounded total delay.
- `Once`: reveal only once, default `true`.
- `Paused`: suspend pending motion without hiding content.
- `ReducedMotion`: explicit application preference in addition to the browser media query.
- `Disabled`: render all descendants visible without observation.

The group forwards non-owned attributes, exposes `data-slot="reveal-group"`, and creates no animation until JavaScript enhancement succeeds.

### `ShadcnReveal`

`ShadcnReveal` is an opt-in semantic wrapper registered with its nearest reveal group.

Public parameters:

- `ChildContent`: revealed content.
- `Tag`: semantic wrapper element, default `div`.
- `Effect`: typed `ShadcnRevealEffect` value.
- `Delay`: per-item delay added after group staggering.
- `Duration`: optional duration override.
- `Cascade`: enables descendant component choreography.
- `Disabled`: keeps this item visible and unobserved.

Initial effects:

- `Fade`
- `Rise`
- `Scale`
- `Clip`
- `None`

The default effect is `Rise`. Effects use opacity, transform, or a bounded clip path. They do not animate layout-driving properties.

## Runtime lifecycle

1. Server rendering and the first browser paint show all content normally.
2. On first interactive render, `ShadcnRevealGroup` imports the package JavaScript module and registers its root.
3. The module marks only eligible, not-yet-visible descendants as pending. Items already intersecting begin immediately, avoiding a flash from visible to hidden.
4. One `IntersectionObserver` per group changes an item from `pending` to `revealing`, then to `revealed` after `animationend` with a bounded timeout fallback.
5. When `Once` is enabled, revealed elements are unobserved and never reset during rerenders.
6. Mutation observation registers newly inserted reveal items without reprocessing stable nodes.
7. Pausing completes in-flight reveals and leaves pending content visible; resuming observes unrevealed content again.
8. Disposal disconnects observers, mutation observers, event handlers, and timers.

Stable identity is based on the existing DOM node. Theme Studio keeps stable `@key` values, so Shuffle updates CSS variables and parameters without replaying entry motion or resetting interactive state.

## Component-aware choreography

When `Cascade` is enabled, the reveal scope coordinates existing library components through public `data-slot` contracts:

- chart series enter from their baseline with a short capped series stagger;
- progress indicators grow from logical inline start;
- metric values and status groups fade into their final hierarchy;
- conversation messages reveal in source order without clipping Thai text;
- form sections reveal as groups while controls remain immediately operable;
- skeletons, spinners, overlays, and persistent loops retain their own component lifecycle.

Choreography is scoped beneath `ShadcnReveal`; it does not alter component state or accessible content. Consumers can opt out with `Cascade="false"`.

## Accessibility and input behavior

- `prefers-reduced-motion: reduce`, `ReducedMotion`, or `Disabled` makes every item immediately visible and disables reveal/cascade animation.
- Reveal wrappers add no roles, names, focus order, or live-region announcements.
- Focused elements are never hidden or displaced.
- Pointer, keyboard, touch, drag, and form interactions remain available while an animation runs.
- Forced-colors styling does not depend on opacity or blur to communicate state.
- Logical properties and transform origins preserve LTR and RTL behavior.

## Theme Studio migration

- Replace the Theme-specific `attachBentoReveal` lifecycle with `ShadcnRevealGroup` at the curated preview boundary.
- Wrap each stable Bento item payload in `ShadcnReveal`, using a calm capped stagger and `Cascade="true"`.
- Remove Theme Studio CSS rules and JavaScript that own reveal state.
- Keep the existing Bento grid, card registry, stable ordering, stable keys, manual scrolling, interaction pause behavior, and component animation state.
- Use several related effects according to content meaning rather than assigning a random effect per card.
- Do not animate the company app bar, settings sidebar, or preview layout itself when themes change.

## Styling and performance budget

- Control feedback remains within 100-200ms.
- Card reveals use 360-520ms with `cubic-bezier(0.16, 1, 0.3, 1)`.
- Descendant staggering is capped so a card completes within 800ms.
- Only opacity, transform, and bounded clip path are used for entry motion.
- `will-change` exists only while an item is pending or revealing.
- Hidden tabs and non-visible groups do not run recurring work.
- The observer and mutation callback batch DOM reads and writes.

## Testing

### Component and contract tests

- Public parameters render expected data attributes and validated CSS variables.
- Invalid thresholds, durations, stagger values, root margins, and tags fail deterministically.
- Disabled and reduced-motion states render visible content.
- Package CSS contains effect, cascade, RTL, forced-colors, and reduced-motion contracts.
- Public API snapshot changes are intentional and reviewed.

### Browser tests

- Below-fold cards start visible before enhancement, become pending after registration, and reveal when manually scrolled into view.
- Revealed cards do not replay after theme Shuffle, locale changes, or unrelated rerenders.
- Newly inserted reveal items register once.
- Charts, progress, messages, and grouped content animate under a reveal scope.
- Reduced-motion mode has no reveal animation and no hidden content.
- Desktop, tablet, and mobile paths preserve scrolling, interaction, focus, and stable geometry.
- Disposal and navigation produce no JavaScript errors or lingering observers.

### Repository validation

- Release builds complete with zero warnings and zero errors.
- Focused component and browser tests pass before the full package and repository suites.
- The public-surface verification script passes.
- The Impeccable detector runs once against the final changed UI targets.

## Compatibility and rollout

The feature is additive and opt-in. Existing components do not animate merely because the package is upgraded. The Theme Studio migration is the first consumer and serves as the interactive dossier for the new motion API. No dependency, schema, storage, or network contract changes are required.
