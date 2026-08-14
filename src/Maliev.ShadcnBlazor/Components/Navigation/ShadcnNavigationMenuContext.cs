using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Components.Navigation;

internal sealed class ShadcnNavigationMenuContext(ShadcnNavigationMenu owner)
{
    private readonly List<ShadcnNavigationMenuRegistration> _items = [];
    private readonly Dictionary<string, RenderFragment?> _contents = new(StringComparer.Ordinal);
    internal ShadcnNavigationMenu Owner { get; } = owner;
    internal event Action? ContentChanged;
    internal IReadOnlyList<ShadcnNavigationMenuRegistration> Items => _items;
    internal void Register(ShadcnNavigationMenuRegistration item) { if (string.IsNullOrWhiteSpace(item.Value)) throw new ArgumentException("Navigation menu values cannot be empty."); if (_items.Any(existing => existing.Value == item.Value)) throw new InvalidOperationException($"Navigation menu value '{item.Value}' is already registered."); _items.Add(item); }
    internal void Update(string value, bool disabled) { var i = _items.FindIndex(item => item.Value == value); if (i >= 0) _items[i] = _items[i] with { Disabled = disabled }; }
    internal void Unregister(string value) => _items.RemoveAll(item => item.Value == value);
    internal bool RegisterContent(string value, RenderFragment? content)
    {
        if (content is null || _contents.TryGetValue(value, out var existing) && ReferenceEquals(existing, content)) return false;
        _contents[value] = content;
        return true;
    }
    internal ShadcnNavigationMenuRegistration? ActiveRegistration => _items.FirstOrDefault(item => item.Value == Owner.EffectiveValue);
    internal ShadcnNavigationMenuItemContext? ActiveItemContext => ActiveRegistration is { } registration ? new(this, registration) : null;
    internal RenderFragment? ActiveContent => Owner.EffectiveValue is { } value && _contents.TryGetValue(value, out var content) ? content : null;
    internal void NotifyContentChanged() => ContentChanged?.Invoke();
}
internal sealed record ShadcnNavigationMenuRegistration(string Value, string TriggerId, string ContentId, bool Disabled);
internal sealed record ShadcnNavigationMenuItemContext(ShadcnNavigationMenuContext Menu, ShadcnNavigationMenuRegistration Registration)
{
    internal bool Open => Menu.Owner.EffectiveValue == Registration.Value;
    internal bool Disabled => Menu.Owner.Disabled || Registration.Disabled;
}
