namespace Maliev.ShadcnBlazor.Components.Primitives;

/// <summary>
/// Defines the common programmatic focus contract for components with a deterministic focus entry target.
/// </summary>
public interface IShadcnFocusable
{
    /// <summary>Moves focus to the component's documented focus entry target after it has rendered.</summary>
    /// <param name="preventScroll">Whether the browser should avoid scrolling the focused target into view.</param>
    ValueTask FocusAsync(bool preventScroll = false);
}
