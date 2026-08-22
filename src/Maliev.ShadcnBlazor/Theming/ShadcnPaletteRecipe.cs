using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Describes how a materialized palette can be reproduced.</summary>
public sealed record ShadcnPaletteRecipe
{
    /// <summary>Creates a portable palette recipe with a defensive token snapshot.</summary>
    /// <param name="algorithmVersion">The deterministic palette algorithm version.</param>
    /// <param name="seed">The deterministic palette seed.</param>
    /// <param name="baseColor">The base-color identifier.</param>
    /// <param name="lockedTokens">The semantic token paths that must remain unchanged.</param>
    [JsonConstructor]
    public ShadcnPaletteRecipe(int algorithmVersion, ulong seed, string baseColor, IReadOnlyList<string> lockedTokens)
    {
        ArgumentNullException.ThrowIfNull(lockedTokens);
        AlgorithmVersion = algorithmVersion;
        Seed = seed;
        BaseColor = baseColor;
        LockedTokens = Array.AsReadOnly(lockedTokens.ToArray());
    }

    /// <summary>Gets the deterministic palette algorithm version.</summary>
    public int AlgorithmVersion { get; init; }

    /// <summary>Gets the deterministic palette seed.</summary>
    public ulong Seed { get; init; }

    /// <summary>Gets the base-color identifier.</summary>
    public string BaseColor { get; init; }

    /// <summary>Gets an immutable snapshot of locked semantic token paths.</summary>
    public IReadOnlyList<string> LockedTokens { get; }
}
