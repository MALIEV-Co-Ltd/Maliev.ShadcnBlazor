namespace Maliev.ShadcnBlazor.Components.Disclosure;

internal sealed class ShadcnAccordionContext(ShadcnAccordion owner)
{
    private readonly List<(string Value, string TriggerId, bool Disabled)> _items = [];

    internal ShadcnAccordion Owner { get; } = owner;
    internal IReadOnlyList<(string Value, string TriggerId, bool Disabled)> Items => _items;

    internal void Register(string value, string triggerId, bool disabled)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Accordion item values cannot be empty.", nameof(value));
        if (_items.Any(item => string.Equals(item.Value, value, StringComparison.Ordinal)))
            throw new InvalidOperationException($"Accordion item value '{value}' is already registered.");
        _items.Add((value, triggerId, disabled));
    }

    internal void Update(string value, bool disabled)
    {
        var index = _items.FindIndex(item => string.Equals(item.Value, value, StringComparison.Ordinal));
        if (index >= 0) _items[index] = (_items[index].Value, _items[index].TriggerId, disabled);
    }

    internal void Unregister(string value) => _items.RemoveAll(item => string.Equals(item.Value, value, StringComparison.Ordinal));
}

internal sealed record ShadcnAccordionItemContext(ShadcnAccordionContext Accordion, string Value, string TriggerId, string ContentId, bool Disabled)
{
    internal bool IsDisabled => Accordion.Owner.Disabled || Disabled;
    internal bool IsOpen => Accordion.Owner.IsOpen(Value);
}
