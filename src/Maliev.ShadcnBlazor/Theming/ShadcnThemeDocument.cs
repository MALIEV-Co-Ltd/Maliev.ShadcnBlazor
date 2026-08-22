using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Represents the canonical, portable MALIEV Shadcn theme document.</summary>
public sealed record ShadcnThemeDocument
{
    /// <summary>Gets the current portable document schema version.</summary>
    public const int CurrentSchemaVersion = 2;

    /// <summary>Gets the portable document schema version.</summary>
    [JsonPropertyOrder(0)]
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>Gets the human-readable document name.</summary>
    [JsonPropertyOrder(1)]
    public required string Name { get; init; }

    /// <summary>Gets the fully materialized runtime theme.</summary>
    [JsonPropertyOrder(2)]
    public required ShadcnTheme Theme { get; init; }

    /// <summary>Gets application defaults needed to reproduce the preview.</summary>
    [JsonPropertyOrder(3)]
    public required ShadcnThemeApplication Application { get; init; }

    /// <summary>Gets the palette generation recipe and locked semantic tokens.</summary>
    [JsonPropertyOrder(4)]
    public required ShadcnPaletteRecipe Palette { get; init; }

    /// <summary>Gets the portable typography selections and semantic role scale.</summary>
    [JsonPropertyOrder(5)]
    public required ShadcnTypographyScale Typography { get; init; }
}
