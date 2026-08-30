namespace Maliev.ShadcnBlazor.Theming.Internal;

internal static class ShadcnPaletteHarmonyGenerator
{
    internal static ShadcnPaletteAnchors Generate(
        ulong seed,
        ShadcnPaletteAnchors anchors,
        ShadcnPaletteHarmony harmony,
        IReadOnlyList<ShadcnPaletteAnchorRole> lockedAnchors)
    {
        _ = ShadcnPaletteColorParser.TryNormalize(anchors.Brand, out var brand, out _);
        var offsets = Offsets(harmony);
        var locked = lockedAnchors.ToHashSet();
        var random = new SplitMix64(seed);
        var hueJitters = new double[5];
        var lightnessUnits = new double[5];
        var chromaUnits = new double[5];
        for (var index = 0; index < hueJitters.Length; index++)
        {
            hueJitters[index] = (random.NextUnitDouble() - 0.5d) * 12d;
            lightnessUnits[index] = random.NextUnitDouble();
            chromaUnits[index] = random.NextUnitDouble();
        }

        // Brand is the stable input basis for a v2 recipe. Seeded harmony generation projects
        // the other unlocked roles from it; re-jittering Brand would make a materialized recipe
        // transform itself again after export and reimport.
        var generated = anchors;
        foreach (var role in Enum.GetValues<ShadcnPaletteAnchorRole>().Where(role => role != ShadcnPaletteAnchorRole.Brand))
        {
            if (locked.Contains(role))
                continue;

            generated = generated.Set(role, GenerateColor(role, brand.Hue, offsets[(int)role]));
        }

        return generated;

        string GenerateColor(ShadcnPaletteAnchorRole role, double baseHue, double offset)
        {
            var index = (int)role;
            var lightnessRange = harmony == ShadcnPaletteHarmony.Free ? 0.30d : 0.08d;
            var lightness = Math.Clamp(
                brand.Lightness + ((lightnessUnits[index] - 0.5d) * lightnessRange),
                0.38d,
                0.68d);
            var chromaScale = harmony == ShadcnPaletteHarmony.Free
                ? 0.65d + (0.70d * chromaUnits[index])
                : 0.90d + (0.20d * chromaUnits[index]);
            var chroma = Math.Clamp(Math.Max(brand.Chroma, 0.10d) * chromaScale, 0.08d, 0.24d);
            var color = new OklchColor(
                lightness,
                chroma,
                OklchColor.NormalizeHue(baseHue + offset + hueJitters[index]));
            return color.ToCss();
        }
    }

    private static ReadOnlySpan<double> Offsets(ShadcnPaletteHarmony harmony) => harmony switch
    {
        ShadcnPaletteHarmony.Free => [0d, 71d, 143d, 214d, 286d],
        ShadcnPaletteHarmony.Analogous => [0d, 30d, -30d, 60d, -60d],
        ShadcnPaletteHarmony.Complementary => [0d, 180d, 30d, 210d, -30d],
        ShadcnPaletteHarmony.Triadic => [0d, 120d, 240d, 60d, 300d],
        _ => throw new ArgumentOutOfRangeException(nameof(harmony), harmony, "Unknown palette harmony.")
    };
}
