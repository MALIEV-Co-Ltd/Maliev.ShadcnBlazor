using System.Globalization;

namespace Maliev.ShadcnBlazor.Components.Overlays;

/// <summary>Controls the physical direction in which a Drawer is dismissed.</summary>
public enum ShadcnDrawerSwipeDirection { Up, Right, Down, Left }
/// <summary>Controls Drawer background interaction and focus behavior.</summary>
public enum ShadcnDrawerModalMode { Modal, NonModal, TrapFocus }
/// <summary>Identifies the unit used by a Drawer snap point.</summary>
public enum ShadcnDrawerSnapPointUnit { Fraction, Pixels, Rem }

/// <summary>Represents a validated Drawer snap point.</summary>
public readonly record struct ShadcnDrawerSnapPoint
{
    private ShadcnDrawerSnapPoint(double value, ShadcnDrawerSnapPointUnit unit) { Value = value; Unit = unit; }
    /// <summary>Gets the numeric value.</summary>
    public double Value { get; }
    /// <summary>Gets the unit.</summary>
    public ShadcnDrawerSnapPointUnit Unit { get; }
    /// <summary>Creates a viewport fraction greater than zero and at most one.</summary>
    public static ShadcnDrawerSnapPoint Fraction(double value) => value is > 0 and <= 1 ? new(value, ShadcnDrawerSnapPointUnit.Fraction) : throw new ArgumentOutOfRangeException(nameof(value));
    /// <summary>Creates a positive pixel snap point.</summary>
    public static ShadcnDrawerSnapPoint Pixels(double value) => value > 0 && double.IsFinite(value) ? new(value, ShadcnDrawerSnapPointUnit.Pixels) : throw new ArgumentOutOfRangeException(nameof(value));
    /// <summary>Creates a positive rem snap point.</summary>
    public static ShadcnDrawerSnapPoint Rem(double value) => value > 0 && double.IsFinite(value) ? new(value, ShadcnDrawerSnapPointUnit.Rem) : throw new ArgumentOutOfRangeException(nameof(value));
    /// <summary>Returns the Base UI-compatible CSS representation.</summary>
    public string ToCss() => Unit switch { ShadcnDrawerSnapPointUnit.Fraction => Value.ToString("0.####", CultureInfo.InvariantCulture), ShadcnDrawerSnapPointUnit.Pixels => $"{Value.ToString("0.####", CultureInfo.InvariantCulture)}px", ShadcnDrawerSnapPointUnit.Rem => $"{Value.ToString("0.####", CultureInfo.InvariantCulture)}rem", _ => throw new InvalidOperationException("Unknown drawer snap point unit.") };
    /// <inheritdoc />
    public override string ToString() => ToCss();
}

internal sealed class ShadcnDrawerContext(ShadcnDrawer owner, string contentId)
{
    internal ShadcnDrawer Owner { get; } = owner; internal string ContentId { get; } = contentId; internal string? TitleId { get; set; } internal string? DescriptionId { get; set; }
}
