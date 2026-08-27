namespace Maliev.ShadcnBlazor.Components.Selection;

internal sealed class ShadcnRadioGroupContext<TValue>
{
    private readonly List<ItemRegistration> _items = [];
    private readonly Func<TValue, Task> _selectAsync;

    internal ShadcnRadioGroupContext(Func<TValue, Task> selectAsync) => _selectAsync = selectAsync;

    internal TValue Value { get; private set; } = default!;
    internal string Name { get; private set; } = string.Empty;
    internal bool Disabled { get; private set; }
    internal bool ReadOnly { get; private set; }
    internal bool Invalid { get; private set; }
    internal string? AriaDescribedBy { get; private set; }
    internal ShadcnRadioGroupPresentation Presentation { get; private set; }
    internal bool HideIndicator { get; private set; }
    internal int RegistrationVersion { get; private set; }

    internal void Update(TValue value, string name, bool disabled, bool readOnly, bool invalid, string? ariaDescribedBy, ShadcnRadioGroupPresentation presentation, bool hideIndicator)
    {
        Value = value;
        Name = name;
        Disabled = disabled;
        ReadOnly = readOnly;
        Invalid = invalid;
        AriaDescribedBy = ariaDescribedBy;
        Presentation = presentation;
        HideIndicator = hideIndicator;
    }

    internal Guid Register(TValue value, Func<bool> disabled)
    {
        var registration = new ItemRegistration(Guid.NewGuid(), value, disabled);
        _items.Add(registration);
        RegistrationVersion++;
        return registration.Key;
    }

    internal void Unregister(Guid key)
    {
        if (_items.RemoveAll(item => item.Key == key) > 0)
            RegistrationVersion++;
    }

    internal bool IsSelected(TValue value) => EqualityComparer<TValue>.Default.Equals(Value, value);

    internal int GetTabIndex(Guid key)
    {
        var enabled = _items.Where(item => !Disabled && !item.Disabled()).ToArray();
        if (enabled.Length == 0)
            return -1;
        var active = enabled.FirstOrDefault(item => IsSelected(item.Value)) ?? enabled[0];
        return active.Key == key ? 0 : -1;
    }

    internal Task SelectAsync(TValue value) => Disabled || ReadOnly ? Task.CompletedTask : _selectAsync(value);

    private sealed record ItemRegistration(Guid Key, TValue Value, Func<bool> Disabled);
}
