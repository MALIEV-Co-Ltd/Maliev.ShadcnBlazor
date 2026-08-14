namespace Maliev.ShadcnBlazor.Components.Navigation;

internal sealed class ShadcnTabsContext(ShadcnTabs owner)
{
    private readonly List<ShadcnTabsRegistration> _items = [];
    internal ShadcnTabs Owner { get; } = owner;
    internal IReadOnlyList<ShadcnTabsRegistration> Items => _items;
    internal void Register(ShadcnTabsRegistration item)
    {
        if (string.IsNullOrWhiteSpace(item.Value)) throw new ArgumentException("Tab values cannot be empty.", nameof(item));
        if (_items.Any(existing => string.Equals(existing.Value, item.Value, StringComparison.Ordinal))) throw new InvalidOperationException($"Tab value '{item.Value}' is already registered.");
        _items.Add(item);
    }
    internal void Update(string value, bool disabled)
    {
        var index = _items.FindIndex(item => item.Value == value);
        if (index >= 0) _items[index] = _items[index] with { Disabled = disabled };
    }
    internal void Unregister(string value) => _items.RemoveAll(item => item.Value == value);
    internal ShadcnTabsRegistration Get(string value) => _items.FirstOrDefault(item => item.Value == value) ?? throw new InvalidOperationException($"Tab content '{value}' requires a matching trigger rendered before it.");
}

internal sealed record ShadcnTabsRegistration(string Value, string TriggerId, string ContentId, bool Disabled);
