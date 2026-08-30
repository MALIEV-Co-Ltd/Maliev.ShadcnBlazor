using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Describes how a materialized palette can be reproduced.</summary>
public sealed record ShadcnPaletteRecipe
{
    /// <summary>Identifies a materialized or migrated palette without a reproducible generator recipe.</summary>
    public const int MaterializedAlgorithmVersion = 0;

    /// <summary>Identifies the version-one deterministic palette generation algorithm.</summary>
    public const int LegacyAlgorithmVersion = 1;

    /// <summary>Identifies the current algorithm understood by the historical four-argument constructor.</summary>
    /// <remarks>This remains version one for source compatibility. Use <see cref="CreateV2"/> for version two recipes.</remarks>
    public const int CurrentAlgorithmVersion = LegacyAlgorithmVersion;

    /// <summary>Identifies the version-two portable palette recipe algorithm.</summary>
    public const int VersionTwoAlgorithmVersion = 2;

    /// <summary>Creates a portable palette recipe with a defensive token snapshot.</summary>
    /// <param name="algorithmVersion">The deterministic palette algorithm version.</param>
    /// <param name="seed">The deterministic palette seed.</param>
    /// <param name="baseColor">The base-color identifier.</param>
    /// <param name="lockedTokens">The semantic token paths that must remain unchanged.</param>
    public ShadcnPaletteRecipe(int algorithmVersion, ulong seed, string baseColor, IReadOnlyList<string> lockedTokens)
        : this(algorithmVersion, seed, baseColor, lockedTokens, null, null, null)
    {
    }

    /// <summary>Creates a portable palette recipe from its serialized fields.</summary>
    /// <param name="algorithmVersion">The deterministic palette algorithm version.</param>
    /// <param name="seed">The deterministic palette seed.</param>
    /// <param name="baseColor">The base-color identifier.</param>
    /// <param name="lockedTokens">The semantic token paths that must remain unchanged.</param>
    /// <param name="anchors">The version-two palette anchors.</param>
    /// <param name="harmony">The version-two palette harmony.</param>
    /// <param name="lockedAnchors">The version-two palette anchors that must remain unchanged.</param>
    [JsonConstructor]
    public ShadcnPaletteRecipe(
        int algorithmVersion,
        ulong seed,
        string baseColor,
        IReadOnlyList<string> lockedTokens,
        ShadcnPaletteAnchors? anchors,
        ShadcnPaletteHarmony? harmony,
        IReadOnlyList<ShadcnPaletteAnchorRole>? lockedAnchors)
    {
        ArgumentNullException.ThrowIfNull(lockedTokens);
        AlgorithmVersion = algorithmVersion;
        Seed = seed;
        BaseColor = baseColor;
        LockedTokens = Array.AsReadOnly(lockedTokens.ToArray());
        Anchors = anchors;
        Harmony = harmony;
        LockedAnchors = lockedAnchors is null
            ? null
            : Array.AsReadOnly(lockedAnchors.ToArray());
    }

    /// <summary>Gets the deterministic palette algorithm version.</summary>
    public int AlgorithmVersion { get; init; }

    /// <summary>Gets the deterministic palette seed.</summary>
    public ulong Seed { get; init; }

    /// <summary>Gets whether this is a version-two palette recipe.</summary>
    [JsonIgnore]
    public bool IsVersion2 => AlgorithmVersion == VersionTwoAlgorithmVersion;

    /// <summary>Gets the base-color identifier.</summary>
    public string BaseColor { get; init; }

    /// <summary>Gets an immutable snapshot of locked semantic token paths.</summary>
    public IReadOnlyList<string> LockedTokens { get; }

    /// <summary>Gets the version-two palette anchors, when present.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ShadcnPaletteAnchors? Anchors { get; }

    /// <summary>Gets the version-two palette harmony, when present.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ShadcnPaletteHarmony? Harmony { get; }

    /// <summary>Gets an immutable snapshot of version-two locked palette anchors, when present.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ShadcnPaletteAnchorRole>? LockedAnchors { get; }

    /// <summary>Creates a version-two portable palette recipe with defensive lock snapshots.</summary>
    /// <param name="seed">The deterministic palette seed.</param>
    /// <param name="baseColor">The base-color identifier.</param>
    /// <param name="lockedTokens">The semantic token paths that must remain unchanged.</param>
    /// <param name="anchors">The version-two palette anchors.</param>
    /// <param name="harmony">The version-two palette harmony.</param>
    /// <param name="lockedAnchors">The version-two palette anchors that must remain unchanged.</param>
    /// <returns>A version-two portable palette recipe.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required collection or anchor set is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="harmony"/> or <paramref name="lockedAnchors"/> is unsupported.</exception>
    public static ShadcnPaletteRecipe CreateV2(
        ulong seed,
        string baseColor,
        IReadOnlyList<string> lockedTokens,
        ShadcnPaletteAnchors anchors,
        ShadcnPaletteHarmony harmony,
        IEnumerable<ShadcnPaletteAnchorRole> lockedAnchors)
    {
        ArgumentNullException.ThrowIfNull(lockedTokens);
        ArgumentNullException.ThrowIfNull(anchors);
        ArgumentNullException.ThrowIfNull(lockedAnchors);
        if (!Enum.IsDefined(harmony))
            throw new ArgumentOutOfRangeException(nameof(harmony), harmony, "Unknown palette harmony.");

        var anchorSnapshot = lockedAnchors.ToArray();
        foreach (var role in anchorSnapshot)
        {
            if (!Enum.IsDefined(role))
                throw new ArgumentOutOfRangeException(nameof(lockedAnchors), role, "Unknown palette anchor role.");
        }

        return new(
            VersionTwoAlgorithmVersion,
            seed,
            baseColor,
            lockedTokens,
            anchors,
            harmony,
            anchorSnapshot.Distinct().Order().ToArray());
    }
}
