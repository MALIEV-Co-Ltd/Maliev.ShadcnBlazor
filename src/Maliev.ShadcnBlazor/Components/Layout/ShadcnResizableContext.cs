namespace Maliev.ShadcnBlazor.Components.Layout;
internal sealed class ShadcnResizableContext(ShadcnResizableGroup owner)
{
    private readonly List<ShadcnResizablePanelRegistration> _panels = [];
    internal ShadcnResizableGroup Owner { get; } = owner;
    internal IReadOnlyList<ShadcnResizablePanelRegistration> Panels => _panels;
    internal int IndexOf(string id) => _panels.FindIndex(panel => panel.Id == id);
    internal int Register(ShadcnResizablePanelRegistration panel)
    {
        if (string.IsNullOrWhiteSpace(panel.Id)) throw new ArgumentException("Resizable panel IDs cannot be empty.");
        if (_panels.Any(existing => existing.Id == panel.Id)) throw new InvalidOperationException($"Resizable panel ID '{panel.Id}' is already registered.");
        _panels.Add(panel); Owner.NotifyLayoutChanged(); return _panels.Count - 1;
    }
    internal void Unregister(string id) { _panels.RemoveAll(panel => panel.Id == id); Owner.NotifyLayoutChanged(); }
    internal void Update(string registeredId, ShadcnResizablePanelRegistration panel)
    {
        var index = _panels.FindIndex(candidate => candidate.Id == registeredId);
        if (index < 0 || index >= _panels.Count) return;
        if (_panels.Where((_, candidate) => candidate != index).Any(existing => existing.Id == panel.Id)) throw new InvalidOperationException($"Resizable panel ID '{panel.Id}' is already registered.");
        _panels[index] = panel; Owner.NotifyLayoutChanged();
    }
}
internal sealed record ShadcnResizablePanelRegistration(string Id, double MinimumSize, double MaximumSize, double DefaultSize, bool Collapsible, double CollapsedSize);
