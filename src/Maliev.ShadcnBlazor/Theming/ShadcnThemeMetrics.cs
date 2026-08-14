namespace Maliev.ShadcnBlazor.Theming;

public sealed record ShadcnThemeMetrics
{
    public required string FontFamily { get; init; }
    public string MonospaceFontFamily { get; init; } = "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace";
    public double RadiusRem { get; init; }
    public double RadiusSmallScale { get; init; }
    public double RadiusMediumScale { get; init; }
    public double RadiusLargeScale { get; init; }
    public double RadiusExtraLargeScale { get; init; }
    public double Radius2ExtraLargeScale { get; init; }
    public double Radius3ExtraLargeScale { get; init; }
    public double Radius4ExtraLargeScale { get; init; }
    public double ControlHeightRem { get; init; }
    public double ControlHeightSmallRem { get; init; }
    public double ControlHeightLargeRem { get; init; }
    public double SpacingScaleMultiplier { get; init; } = 1;
    public double FocusRingWidthPx { get; init; } = 3;
    public double FocusRingOffsetPx { get; init; }
    public int MotionDurationMilliseconds { get; init; } = 150;
    public string MotionEasing { get; init; } = "ease-out";
    public ShadcnReducedMotionBehavior ReducedMotionBehavior { get; init; } = ShadcnReducedMotionBehavior.RespectSystemPreference;
}
