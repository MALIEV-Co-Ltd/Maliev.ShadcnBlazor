namespace Maliev.ShadcnBlazor.Components.Actions;

internal sealed class ShadcnToggleGroupContext<TValue>
{
    private readonly List<ItemRegistration> _items = [];
    private readonly Func<TValue, Task> _toggleAsync;

    internal ShadcnToggleGroupContext(Func<TValue, Task> toggleAsync) => _toggleAsync = toggleAsync;

    internal IReadOnlyCollection<TValue> Values { get; private set; } = Array.Empty<TValue>();
    internal bool Multiple { get; private set; }
    internal bool Disabled { get; private set; }
    internal ShadcnToggleVariant Variant { get; private set; }
    internal ShadcnToggleSize Size { get; private set; }
    internal double Spacing { get; private set; }
    internal int RegistrationVersion { get; private set; }

    internal void Update(
        IReadOnlyCollection<TValue> values,
        bool multiple,
        bool disabled,
        ShadcnToggleVariant variant,
        ShadcnToggleSize size,
        double spacing)
    {
        Values = values;
        Multiple = multiple;
        Disabled = disabled;
        Variant = variant;
        Size = size;
        Spacing = spacing;
    }

    internal Guid Register(TValue value, Func<bool> disabled, Action refresh)
    {
        var registration = new ItemRegistration(Guid.NewGuid(), value, disabled, refresh);
        _items.Add(registration);
        RegistrationVersion++;
        return registration.Key;
    }

    internal void Unregister(Guid key)
    {
        var removed = _items.RemoveAll(item => item.Key == key);
        if (removed > 0)
            RegistrationVersion++;
    }

    internal bool IsSelected(TValue value) => Values.Contains(value, EqualityComparer<TValue>.Default);

    internal int GetTabIndex(Guid key)
    {
        var enabled = _items.Where(item => !Disabled && !item.Disabled()).ToArray();
        if (enabled.Length == 0)
            return -1;

        var selected = enabled.FirstOrDefault(item => IsSelected(item.Value));
        var active = selected ?? enabled[0];
        return active.Key == key ? 0 : -1;
    }

    internal Task ToggleAsync(TValue value) => Disabled ? Task.CompletedTask : _toggleAsync(value);

    internal void NotifyItems()
    {
        foreach (var item in _items)
            item.Refresh();
    }

    private sealed record ItemRegistration(Guid Key, TValue Value, Func<bool> Disabled, Action Refresh);
}
