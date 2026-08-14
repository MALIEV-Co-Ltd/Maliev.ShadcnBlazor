using System.Globalization;
using System.Text;

namespace Maliev.ShadcnBlazor.Components.Overlays;

internal sealed class ShadcnCommandContext(Action changed)
{
    private readonly Dictionary<object, (string Value, string SearchText, bool Disabled)> _items = [];
    internal string Search { get; private set; } = string.Empty;
    internal bool ShouldFilter { get; set; } = true;
    internal event Action? StateChanged;
    internal string ListId { get; } = $"shadcn-command-list-{Guid.NewGuid():N}";
    internal void Register(object key, string value, string searchText, bool disabled)
    {
        if (_items.Any(pair => !ReferenceEquals(pair.Key, key) && string.Equals(pair.Value.Value, value, StringComparison.Ordinal)))
            throw new ArgumentException($"Command item value '{value}' must be unique.", nameof(value));
        var next = (value, searchText, disabled);
        if (_items.TryGetValue(key, out var current) && current == next) return;
        _items[key] = next; Notify();
    }
    internal void Unregister(object key) { if (_items.Remove(key)) Notify(); }
    internal void SetSearch(string? value) { Search = value ?? string.Empty; Notify(); }
    internal bool Matches(string searchText) => !ShouldFilter || string.IsNullOrWhiteSpace(Search) || Normalize(searchText).Contains(Normalize(Search), StringComparison.Ordinal);
    internal bool HasMatch => _items.Values.Any(item => !item.Disabled && Matches(item.SearchText));
    private void Notify() { changed(); StateChanged?.Invoke(); }
    private static string Normalize(string value) => value.Normalize(NormalizationForm.FormKC).ToLower(CultureInfo.CurrentCulture);
}
