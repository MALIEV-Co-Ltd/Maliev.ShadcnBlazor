namespace Maliev.ShadcnBlazor.Components.Overlays;

internal sealed class ShadcnMenubarContext
{
    private readonly List<IShadcnMenuOwner> _menus = [];
    internal void BeginRender() => _menus.Clear();
    internal int Register(IShadcnMenuOwner menu) { var index = _menus.IndexOf(menu); if (index >= 0) return index; _menus.Add(menu); return _menus.Count - 1; }
    internal void Unregister(IShadcnMenuOwner menu) => _menus.Remove(menu);
    internal async ValueTask OpenAsync(IShadcnMenuOwner menu)
    {
        foreach (var candidate in _menus)
            if (!ReferenceEquals(candidate, menu) && candidate.EffectiveOpen)
                await candidate.SetOpenAsync(false);
        await menu.SetOpenAsync(!menu.EffectiveOpen);
    }
}

internal sealed record ShadcnMenubarMenuContext(int Index);
