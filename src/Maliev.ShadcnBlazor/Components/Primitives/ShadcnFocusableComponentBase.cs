using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Components.Primitives;

/// <summary>Base class for components that expose one stable native focus target.</summary>
public abstract class ShadcnFocusableComponentBase : ShadcnComponentBase, IShadcnFocusable
{
    /// <summary>Gets or sets the native element owned by the component.</summary>
    protected ElementReference FocusElement { get; set; }

    /// <inheritdoc />
    public ValueTask FocusAsync(bool preventScroll = false) => FocusElement.FocusAsync(preventScroll);
}
