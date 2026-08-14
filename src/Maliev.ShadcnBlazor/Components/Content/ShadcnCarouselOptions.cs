namespace Maliev.ShadcnBlazor.Components.Content;

/// <summary>Configures the package-owned carousel engine.</summary>
public sealed record ShadcnCarouselOptions
{
    public ShadcnCarouselAlign Align { get; init; } = ShadcnCarouselAlign.Start;
    public bool Loop { get; init; }
    public int SlidesToScroll { get; init; } = 1;
    public double DragThreshold { get; init; } = 30;
    public bool RightToLeft { get; init; }
    public bool ReducedMotion { get; init; }

    internal void Validate()
    {
        if (!Enum.IsDefined(Align)) throw new ArgumentOutOfRangeException(nameof(Align), Align, "Unknown carousel alignment.");
        if (SlidesToScroll <= 0) throw new ArgumentOutOfRangeException(nameof(SlidesToScroll));
        if (!double.IsFinite(DragThreshold) || DragThreshold < 0) throw new ArgumentOutOfRangeException(nameof(DragThreshold));
    }
}
