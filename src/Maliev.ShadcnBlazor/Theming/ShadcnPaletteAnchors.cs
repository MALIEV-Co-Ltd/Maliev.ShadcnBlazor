namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Stores the five portable color anchors for a version-two palette recipe.</summary>
public sealed record ShadcnPaletteAnchors(
    string Brand,
    string Support,
    string Highlight,
    string DataA,
    string DataB)
{
    /// <summary>Gets the anchor value for the specified role.</summary>
    /// <param name="role">The palette anchor role.</param>
    /// <returns>The anchor value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="role"/> is unsupported.</exception>
    public string Get(ShadcnPaletteAnchorRole role) => role switch
    {
        ShadcnPaletteAnchorRole.Brand => Brand,
        ShadcnPaletteAnchorRole.Support => Support,
        ShadcnPaletteAnchorRole.Highlight => Highlight,
        ShadcnPaletteAnchorRole.DataA => DataA,
        ShadcnPaletteAnchorRole.DataB => DataB,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
    };

    /// <summary>Creates a copy with the value for the specified role replaced.</summary>
    /// <param name="role">The palette anchor role.</param>
    /// <param name="value">The replacement anchor value.</param>
    /// <returns>A copy with the replacement value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="role"/> is unsupported.</exception>
    public ShadcnPaletteAnchors Set(ShadcnPaletteAnchorRole role, string value) => role switch
    {
        ShadcnPaletteAnchorRole.Brand => this with { Brand = value },
        ShadcnPaletteAnchorRole.Support => this with { Support = value },
        ShadcnPaletteAnchorRole.Highlight => this with { Highlight = value },
        ShadcnPaletteAnchorRole.DataA => this with { DataA = value },
        ShadcnPaletteAnchorRole.DataB => this with { DataB = value },
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
    };
}
