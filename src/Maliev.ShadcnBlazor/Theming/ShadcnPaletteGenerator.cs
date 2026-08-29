using Maliev.ShadcnBlazor.Theming.Internal;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Generates complete, deterministic semantic palettes from portable recipes.</summary>
public static class ShadcnPaletteGenerator
{
    /// <summary>Identifies the algorithm understood by historical four-argument recipe construction.</summary>
    /// <remarks>This remains version one for source compatibility. Use <see cref="VersionTwoAlgorithmVersion"/> for version-two recipes.</remarks>
    public const int CurrentAlgorithmVersion = ShadcnPaletteRecipe.LegacyAlgorithmVersion;

    /// <summary>Identifies the version-two deterministic palette generation algorithm.</summary>
    public const int VersionTwoAlgorithmVersion = ShadcnPaletteRecipe.VersionTwoAlgorithmVersion;

    private static readonly IReadOnlyDictionary<string, (double Hue, double Chroma)> BaseColors =
        new Dictionary<string, (double, double)>(StringComparer.Ordinal)
        {
            ["neutral"] = (0, 0),
            ["stone"] = (55, 0.18),
            ["zinc"] = (285, 0.14),
            ["slate"] = (250, 0.18)
        };

    /// <summary>Creates a deterministic candidate without mutating the source theme.</summary>
    public static ShadcnPaletteGenerationResult Generate(ShadcnTheme source, ShadcnPaletteRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(recipe);
        return recipe.AlgorithmVersion switch
        {
            ShadcnPaletteRecipe.LegacyAlgorithmVersion => GenerateV1(source, recipe),
            VersionTwoAlgorithmVersion => GenerateV2(source, recipe),
            _ => Result(source.DeepClone(),
                [new("palette-unsupported-algorithm", "palette.algorithmVersion",
                    $"Palette algorithm version {recipe.AlgorithmVersion} is not supported.")], [])
        };
    }

    private static ShadcnPaletteGenerationResult GenerateV1(ShadcnTheme source, ShadcnPaletteRecipe recipe)
    {
        var errors = ValidateRecipe(recipe);
        if (errors.Count > 0)
            return Result(source.DeepClone(), errors, []);

        var anchor = BaseColors[recipe.BaseColor];
        var random = new SplitMix64(recipe.Seed);
        var values = new double[16];
        for (var index = 0; index < values.Length; index++)
            values[index] = random.NextUnitDouble();

        var primaryHue = anchor.Chroma < 0.01
            ? values[0] * 360d
            : OklchColor.NormalizeHue(anchor.Hue + ((values[0] - 0.5d) * 12d));
        var harmonies = new[] { (-30d, 30d), (120d, 240d), (150d, 210d) };
        var harmony = harmonies[Math.Min(2, (int)(values[1] * 3d))];
        var secondaryHue = OklchColor.NormalizeHue(primaryHue + harmony.Item1 + ((values[2] - 0.5d) * 8d));
        var accentHue = OklchColor.NormalizeHue(primaryHue + harmony.Item2 + ((values[3] - 0.5d) * 8d));
        var actionChroma = Math.Clamp(Math.Max(anchor.Chroma, 0.12d) * (0.9d + (0.2d * values[4])), 0.10d, 0.14d);
        var destructiveHue = OklchColor.NormalizeHue(25d + ((values[5] - 0.5d) * 8d));
        var chartOffsets = new[] { 0d, 55d, 120d, 200d, 285d };
        var chartHues = chartOffsets.Select((offset, index) =>
            OklchColor.NormalizeHue(primaryHue + offset + ((values[index + 6] - 0.5d) * 10d))).ToArray();

        var light = CreateScheme(source.Light, false, primaryHue, secondaryHue, accentHue, destructiveHue, chartHues, actionChroma);
        var dark = CreateScheme(source.Dark, true, primaryHue, secondaryHue, accentHue, destructiveHue, chartHues, actionChroma);
        var candidate = source with { Light = light, Dark = dark, Metrics = source.Metrics with { } };
        foreach (var path in recipe.LockedTokens)
            candidate = ShadcnPaletteTokenCatalog.Set(candidate, path, ShadcnPaletteTokenCatalog.Get(source, path));

        candidate = RepairUnlockedContrast(candidate, recipe.LockedTokens);

        var validation = ShadcnThemeValidator.Validate(candidate);
        errors.AddRange(validation.Errors);
        foreach (var failure in validation.ContrastResults.Where(result => !result.Passes && result.Kind != ShadcnContrastKind.DisabledState))
        {
            var foregroundPath = $"{failure.Scheme}.{failure.ForegroundToken}";
            var backgroundPath = $"{failure.Scheme}.{failure.BackgroundToken}";
            var locked = recipe.LockedTokens.Contains(foregroundPath, StringComparer.Ordinal) &&
                         recipe.LockedTokens.Contains(backgroundPath, StringComparer.Ordinal);
            errors.Add(new(
                locked ? "palette-locked-constraint" : "palette-constraint-unsatisfied",
                foregroundPath,
                $"Contrast against {failure.BackgroundToken} is {failure.Ratio:0.###}:1; {failure.RequiredRatio:0.###}:1 is required."));
        }

        return Result(candidate, errors, validation.Warnings);
    }

    private static ShadcnPaletteGenerationResult GenerateV2(ShadcnTheme source, ShadcnPaletteRecipe recipe)
    {
        var errors = ValidateRecipe(recipe);
        var normalized = recipe.Anchors;
        if (normalized is null)
        {
            errors.Add(new("palette-missing-anchors", "palette.anchors", "Version two requires five palette anchors."));
        }
        else
        {
            foreach (var role in Enum.GetValues<ShadcnPaletteAnchorRole>())
            {
                var anchorValue = normalized.Get(role);
                if (anchorValue is { Length: > ShadcnPaletteColorParser.MaximumAnchorLength })
                {
                    errors.Add(new(
                        "palette-anchor-too-long",
                        $"palette.anchors.{AnchorName(role)}",
                        $"Palette anchor must not exceed {ShadcnPaletteColorParser.MaximumAnchorLength} characters."));
                }
                else if (!ShadcnPaletteColorParser.TryNormalize(anchorValue, out _, out var value))
                {
                    errors.Add(new(
                        "palette-invalid-anchor",
                        $"palette.anchors.{AnchorName(role)}",
                        $"{AnchorLabel(role)} must be #rgb, #rrggbb, or oklch(L C H)."));
                }
                else
                {
                    normalized = normalized.Set(role, value);
                }
            }
        }

        if (recipe.Harmony is null)
            errors.Add(new("palette-missing-harmony", "palette.harmony", "Version two requires a palette harmony."));
        else if (!Enum.IsDefined(recipe.Harmony.Value))
            errors.Add(new("palette-invalid-harmony", "palette.harmony", "Palette harmony is not supported."));

        if (recipe.LockedAnchors is null)
        {
            errors.Add(new("palette-missing-locked-anchors", "palette.lockedAnchors", "Version two requires an anchor lock collection."));
        }
        else
        {
            var seen = new HashSet<ShadcnPaletteAnchorRole>();
            for (var index = 0; index < recipe.LockedAnchors.Count; index++)
            {
                var role = recipe.LockedAnchors[index];
                if (!Enum.IsDefined(role))
                    errors.Add(new("palette-invalid-locked-anchor", $"palette.lockedAnchors[{index}]", "Palette anchor role is not supported."));
                else if (!seen.Add(role))
                    errors.Add(new("palette-duplicate-locked-anchor", $"palette.lockedAnchors[{index}]", $"{AnchorLabel(role)} is locked more than once."));
            }
        }

        if (errors.Count > 0)
            return Result(source.DeepClone(), errors, []);

        var anchor = BaseColors[recipe.BaseColor];
        var generated = ShadcnPaletteHarmonyGenerator.Generate(
            recipe.Seed,
            normalized!,
            recipe.Harmony!.Value,
            recipe.LockedAnchors!);
        generated = NormalizeUnlockedAnchorLightness(generated, recipe.LockedAnchors!);
        var candidate = ShadcnPaletteSemanticMapper.Map(source, generated, anchor.Hue, anchor.Chroma);
        foreach (var path in recipe.LockedTokens)
            candidate = ShadcnPaletteTokenCatalog.Set(candidate, path, ShadcnPaletteTokenCatalog.Get(source, path));

        var effectiveLocks = recipe.LockedTokens
            .Concat(ShadcnPaletteSemanticMapper.ProjectLockedAnchorTokens(recipe.LockedAnchors!))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var repairLocks = effectiveLocks
            .Concat(ShadcnPaletteSemanticMapper.ProjectMaterializedAnchorTokens())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        candidate = RepairVersionTwoContrast(candidate, repairLocks);

        var validation = ShadcnThemeValidator.Validate(candidate);
        errors.AddRange(validation.Errors);
        var promotedFailures = validation.ContrastResults
            .Where(result => !result.Passes && result.Kind != ShadcnContrastKind.DisabledState)
            .ToArray();
        foreach (var failure in promotedFailures)
        {
            var foregroundPath = $"{failure.Scheme}.{failure.ForegroundToken}";
            var backgroundPath = $"{failure.Scheme}.{failure.BackgroundToken}";
            var locked = effectiveLocks.Contains(foregroundPath, StringComparer.Ordinal) &&
                         effectiveLocks.Contains(backgroundPath, StringComparer.Ordinal);
            AddUnique(errors, new(
                locked ? "palette-locked-constraint" : "palette-constraint-unsatisfied",
                foregroundPath,
                ContrastMessage(foregroundPath, backgroundPath, failure)));
        }

        var promotedPaths = promotedFailures.Select(failure => $"{failure.Scheme}.{failure.ForegroundToken}").ToHashSet(StringComparer.Ordinal);
        var warnings = validation.Warnings
            .Where(warning => !promotedPaths.Contains(warning.Path))
            .DistinctBy(warning => (warning.Code, warning.Path, warning.Message))
            .ToArray();
        return Result(candidate, errors, warnings, generated);
    }

    internal static bool SupportsBaseColor(string value) => BaseColors.ContainsKey(value);
    internal static bool SupportsLock(string value) => ShadcnPaletteTokenCatalog.IsPath(value);

    private static string ContrastMessage(
        string foregroundPath,
        string backgroundPath,
        ShadcnContrastResult failure) =>
        FormattableString.Invariant(
            $"Contrast between {foregroundPath} and {backgroundPath} is {failure.Ratio:0.###}:1; {failure.RequiredRatio:0.###}:1 is required.");

    private static ShadcnTheme RepairUnlockedContrast(ShadcnTheme candidate, IReadOnlyList<string> locks)
    {
        var locked = locks.ToHashSet(StringComparer.Ordinal);
        for (var pass = 0; pass < 3; pass++)
        {
            var failures = ShadcnThemeValidator.Validate(candidate).ContrastResults
                .Where(result => !result.Passes && result.Kind != ShadcnContrastKind.DisabledState)
                .ToArray();
            if (failures.Length == 0)
                break;

            var changed = false;
            foreach (var failure in failures)
            {
                var foregroundPath = $"{failure.Scheme}.{failure.ForegroundToken}";
                var backgroundPath = $"{failure.Scheme}.{failure.BackgroundToken}";
                if (!locked.Contains(foregroundPath))
                {
                    candidate = ShadcnPaletteTokenCatalog.Set(candidate, foregroundPath,
                        failure.Scheme == "light" ? "oklch(0.1000 0.0000 0.00)" : "oklch(0.9850 0.0000 0.00)");
                    changed = true;
                }
                else if (!locked.Contains(backgroundPath))
                {
                    var darkSurface = ShadcnPaletteTokenCatalog.Set(candidate, backgroundPath, "oklch(0.1000 0.0000 0.00)");
                    var lightSurface = ShadcnPaletteTokenCatalog.Set(candidate, backgroundPath, "oklch(0.9850 0.0000 0.00)");
                    candidate = MatchingContrast(lightSurface, failure).Ratio >= MatchingContrast(darkSurface, failure).Ratio
                        ? lightSurface
                        : darkSurface;
                    changed = true;
                }
            }
            if (!changed)
                break;
        }
        return candidate;
    }

    private static ShadcnContrastResult MatchingContrast(ShadcnTheme theme, ShadcnContrastResult target) =>
        ShadcnThemeValidator.Validate(theme).ContrastResults.First(result =>
            result.Scheme == target.Scheme && result.ForegroundToken == target.ForegroundToken &&
            result.BackgroundToken == target.BackgroundToken && result.Kind == target.Kind);

    private static List<ShadcnThemeValidationMessage> ValidateRecipe(ShadcnPaletteRecipe recipe)
    {
        var errors = new List<ShadcnThemeValidationMessage>();
        if (string.IsNullOrWhiteSpace(recipe.BaseColor) || !SupportsBaseColor(recipe.BaseColor))
            errors.Add(new("palette-invalid-base-color", "palette.baseColor", "Base color must be neutral, stone, zinc, or slate."));
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < recipe.LockedTokens.Count; index++)
        {
            var path = recipe.LockedTokens[index];
            if (!SupportsLock(path))
                errors.Add(new("palette-invalid-lock", $"palette.lockedTokens[{index}]", $"'{path}' is not a lockable semantic color token."));
            else if (!seen.Add(path))
                errors.Add(new("palette-duplicate-lock", $"palette.lockedTokens[{index}]", $"'{path}' is locked more than once."));
        }
        return errors;
    }

    private static ShadcnPaletteAnchors NormalizeUnlockedAnchorLightness(
        ShadcnPaletteAnchors anchors,
        IReadOnlyList<ShadcnPaletteAnchorRole> lockedAnchors)
    {
        var locked = lockedAnchors.ToHashSet();
        foreach (var role in Enum.GetValues<ShadcnPaletteAnchorRole>())
        {
            if (locked.Contains(role))
                continue;
            _ = ShadcnPaletteColorParser.TryNormalize(anchors.Get(role), out var color, out _);
            anchors = anchors.Set(role, (color with { Lightness = 0.55d }).ToCss());
        }
        return anchors;
    }

    private static ShadcnTheme RepairVersionTwoContrast(ShadcnTheme candidate, IReadOnlyList<string> locks)
    {
        var locked = locks.ToHashSet(StringComparer.Ordinal);
        for (var pass = 0; pass < 8; pass++)
        {
            var failures = ShadcnThemeValidator.Validate(candidate).ContrastResults
                .Where(result => !result.Passes && result.Kind != ShadcnContrastKind.DisabledState)
                .ToArray();
            if (failures.Length == 0)
                break;

            var changed = false;
            foreach (var failure in failures)
            {
                var foregroundPath = $"{failure.Scheme}.{failure.ForegroundToken}";
                var backgroundPath = $"{failure.Scheme}.{failure.BackgroundToken}";
                if (!locked.Contains(foregroundPath) && TryRepairEndpoint(candidate, foregroundPath, failure, out var repaired))
                {
                    candidate = repaired;
                    changed = true;
                }
                else if (!locked.Contains(backgroundPath) && TryRepairEndpoint(candidate, backgroundPath, failure, out repaired))
                {
                    candidate = repaired;
                    changed = true;
                }
            }
            if (!changed)
                break;
        }
        return candidate;
    }

    private static bool TryRepairEndpoint(
        ShadcnTheme candidate,
        string path,
        ShadcnContrastResult failure,
        out ShadcnTheme repaired)
    {
        repaired = candidate;
        var currentValue = ShadcnPaletteTokenCatalog.Get(candidate, path);
        if (!ShadcnPaletteColorParser.TryNormalize(currentValue, out var color, out _))
            return false;

        foreach (var lightness in Enumerable.Range(0, 101)
                     .Select(value => value / 100d)
                     .OrderBy(value => Math.Abs(value - color.Lightness))
                     .ThenBy(value => value))
        {
            var next = (color with { Lightness = lightness }).ToCss();
            var proposed = ShadcnPaletteTokenCatalog.Set(candidate, path, next);
            if (MatchingContrast(proposed, failure).Passes)
            {
                repaired = proposed;
                return true;
            }
        }
        return false;
    }

    private static void AddUnique(
        ICollection<ShadcnThemeValidationMessage> messages,
        ShadcnThemeValidationMessage message)
    {
        if (!messages.Any(existing => existing.Code == message.Code && existing.Path == message.Path && existing.Message == message.Message))
            messages.Add(message);
    }

    private static string AnchorName(ShadcnPaletteAnchorRole role) => role switch
    {
        ShadcnPaletteAnchorRole.Brand => "brand",
        ShadcnPaletteAnchorRole.Support => "support",
        ShadcnPaletteAnchorRole.Highlight => "highlight",
        ShadcnPaletteAnchorRole.DataA => "dataA",
        ShadcnPaletteAnchorRole.DataB => "dataB",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
    };

    private static string AnchorLabel(ShadcnPaletteAnchorRole role) => role switch
    {
        ShadcnPaletteAnchorRole.Brand => "Brand",
        ShadcnPaletteAnchorRole.Support => "Support",
        ShadcnPaletteAnchorRole.Highlight => "Highlight",
        ShadcnPaletteAnchorRole.DataA => "Data A",
        ShadcnPaletteAnchorRole.DataB => "Data B",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
    };

    private static ShadcnColorScheme CreateScheme(
        ShadcnColorScheme source,
        bool dark,
        double primaryHue,
        double secondaryHue,
        double accentHue,
        double destructiveHue,
        IReadOnlyList<double> chartHues,
        double actionChroma)
    {
        string Color(double lightness, double chroma, double hue) => new OklchColor(lightness, chroma, hue).ToCss();
        var low = dark ? 0.145d : 0.985d;
        var high = dark ? 0.985d : 0.145d;
        var actionForeground = dark ? 0.145d : 0.985d;
        return new ShadcnColorScheme
        {
            Background = Color(low, 0.006, primaryHue),
            Foreground = Color(high, 0.008, primaryHue),
            Card = Color(dark ? 0.19 : 0.995, 0.006, primaryHue),
            CardForeground = Color(high, 0.008, primaryHue),
            Popover = Color(dark ? 0.19 : 0.995, 0.006, primaryHue),
            PopoverForeground = Color(high, 0.008, primaryHue),
            Primary = Color(dark ? 0.72 : 0.45, actionChroma, primaryHue),
            PrimaryForeground = Color(actionForeground, 0.004, primaryHue),
            Secondary = Color(dark ? 0.28 : 0.925, actionChroma * 0.35, secondaryHue),
            SecondaryForeground = Color(high, 0.008, secondaryHue),
            Muted = Color(dark ? 0.25 : 0.94, Math.Min(0.025, actionChroma * 0.12), primaryHue),
            MutedForeground = Color(dark ? 0.75 : 0.35, 0.012, primaryHue),
            Accent = Color(dark ? 0.30 : 0.91, actionChroma * 0.85, accentHue),
            AccentForeground = Color(high, 0.008, accentHue),
            Destructive = Color(dark ? 0.68 : 0.48, 0.19, destructiveHue),
            DestructiveForeground = Color(actionForeground, 0.004, destructiveHue),
            Border = Color(dark ? 0.65 : 0.55, 0.02, primaryHue),
            Input = Color(dark ? 0.65 : 0.55, 0.02, primaryHue),
            Ring = Color(dark ? 0.90 : 0.18, 0.05, primaryHue),
            Chart1 = Color(dark ? 0.70 : 0.50, actionChroma, chartHues[0]),
            Chart2 = Color(dark ? 0.65 : 0.55, Math.Clamp(actionChroma * 0.85, 0.08, 0.14), chartHues[1]),
            Chart3 = Color(dark ? 0.75 : 0.45, Math.Clamp(actionChroma * 0.95, 0.08, 0.14), chartHues[2]),
            Chart4 = Color(dark ? 0.60 : 0.60, Math.Clamp(actionChroma * 0.80, 0.08, 0.14), chartHues[3]),
            Chart5 = Color(dark ? 0.68 : 0.50, Math.Clamp(actionChroma * 0.90, 0.08, 0.14), chartHues[4]),
            Sidebar = Color(dark ? 0.175 : 0.97, 0.006, primaryHue),
            SidebarForeground = Color(high, 0.008, primaryHue),
            SidebarPrimary = Color(dark ? 0.72 : 0.45, actionChroma * 0.95, primaryHue),
            SidebarPrimaryForeground = Color(actionForeground, 0.004, primaryHue),
            SidebarAccent = Color(dark ? 0.275 : 0.92, actionChroma * 0.30, accentHue),
            SidebarAccentForeground = Color(high, 0.008, accentHue),
            SidebarBorder = Color(dark ? 0.65 : 0.55, 0.02, primaryHue),
            SidebarRing = Color(dark ? 0.90 : 0.18, 0.05, primaryHue),
            ShadowExtraSmall = source.ShadowExtraSmall,
            ShadowSmall = source.ShadowSmall,
            ShadowMedium = source.ShadowMedium
        };
    }

    private static ShadcnPaletteGenerationResult Result(
        ShadcnTheme theme,
        IEnumerable<ShadcnThemeValidationMessage> errors,
        IEnumerable<ShadcnThemeValidationMessage> warnings,
        ShadcnPaletteAnchors? activeAnchors = null) =>
        new(theme, errors.ToArray(), warnings.ToArray()) { ActiveAnchors = activeAnchors };
}
