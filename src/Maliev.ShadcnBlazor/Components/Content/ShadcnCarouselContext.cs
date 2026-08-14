namespace Maliev.ShadcnBlazor.Components.Content;

internal sealed class ShadcnCarouselContext(ShadcnCarousel owner)
{
    private readonly Dictionary<Guid, (int Index, double Basis)> _items = [];
    public int Count => _items.Count;
    public int SelectedIndex => owner.EffectiveSelectedIndex;
    public ShadcnCarouselOrientation Orientation => owner.Orientation;
    public ShadcnCarouselOptions Options => owner.Options;
    public bool Disabled => owner.Disabled;
    public string PreviousLabel => owner.PreviousLabel;
    public string NextLabel => owner.NextLabel;
    public void Register(Guid registrationId, int index, double basisPercent)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        if (_items.Any(item => item.Key != registrationId && item.Value.Index == index)) throw new InvalidOperationException($"Carousel item index {index} is duplicated.");
        var value = (index, basisPercent);
        if (_items.TryGetValue(registrationId, out var current) && current == value) return;
        _items[registrationId] = value;
        owner.NotifyItemsChanged();
    }
    public void Unregister(Guid registrationId)
    {
        if (_items.Remove(registrationId)) owner.NotifyItemsChanged();
    }
    public bool IsSelected(Guid registrationId) => PositionOf(registrationId) == SelectedIndex;
    public string SlideLabel(Guid registrationId) => owner.SlideLabelFormatter(PositionOf(registrationId), Count);
    public string TrackStyle
    {
        get
        {
            var ordered = OrderedItems;
            var preceding = ordered.Take(SelectedIndex).Sum(item => item.Value.Basis);
            var selectedBasis = SelectedIndex >= 0 && SelectedIndex < ordered.Count ? ordered[SelectedIndex].Value.Basis : 100;
            if (selectedBasis <= 0) selectedBasis = 100;
            var alignment = Options.Align switch
            {
                ShadcnCarouselAlign.Start => 0,
                ShadcnCarouselAlign.Center => (100 - selectedBasis) / 2,
                ShadcnCarouselAlign.End => 100 - selectedBasis,
                _ => 0
            };
            var offset = preceding - alignment;
            if (Orientation == ShadcnCarouselOrientation.Vertical)
                return $"translate: 0 {(-offset).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}%";
            var signedOffset = Options.RightToLeft ? offset : -offset;
            return $"translate: {signedOffset.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}% 0";
        }
    }
    public string SlideLabel(int index) => owner.SlideLabelFormatter(index, Count);
    public ValueTask PreviousAsync() => owner.PreviousAsync();
    public ValueTask NextAsync() => owner.NextAsync();
    public ValueTask PointerDownAsync(double x, double y, long pointerId) => owner.PointerDownAsync(x, y, pointerId);
    public ValueTask PointerUpAsync(double x, double y, long pointerId) => owner.PointerUpAsync(x, y, pointerId);
    public ValueTask PointerMoveAsync(double x, double y, long pointerId) => owner.PointerMoveAsync(x, y, pointerId);
    public ValueTask PointerCancelAsync() => owner.PointerCancelAsync();
    public string? DragStyle => owner.DragStyle;
    private IReadOnlyList<KeyValuePair<Guid, (int Index, double Basis)>> OrderedItems => _items.OrderBy(item => item.Value.Index).ToArray();
    private int PositionOf(Guid registrationId)
    {
        var ordered = OrderedItems;
        for (var position = 0; position < ordered.Count; position++)
            if (ordered[position].Key == registrationId) return position;
        return -1;
    }
}
