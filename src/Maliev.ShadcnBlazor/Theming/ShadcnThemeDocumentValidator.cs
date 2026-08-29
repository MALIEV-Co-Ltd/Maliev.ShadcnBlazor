namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Validates portable theme documents before they are applied or exported.</summary>
public static class ShadcnThemeDocumentValidator
{
    /// <summary>Validates a portable theme document.</summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>The complete validation result.</returns>
    public static ShadcnThemeValidationResult Validate(ShadcnThemeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var errors = new List<ShadcnThemeValidationMessage>();
        var warnings = new List<ShadcnThemeValidationMessage>();
        var contrast = new List<ShadcnContrastResult>();

        if (document.SchemaVersion != ShadcnThemeDocument.CurrentSchemaVersion)
            errors.Add(new("unsupported-document-schema", "schemaVersion", $"Theme document schema version must be {ShadcnThemeDocument.CurrentSchemaVersion}."));
        if (string.IsNullOrWhiteSpace(document.Name) || document.Name.Length > 100 || document.Name.Any(char.IsControl))
            errors.Add(new("invalid-document-name", "name", "Document name must be between 1 and 100 characters and contain no control characters."));
        if (document.Theme is null)
            errors.Add(new("required-theme", "theme", "A materialized theme is required."));
        else
        {
            var themeResult = ShadcnThemeValidator.Validate(document.Theme);
            errors.AddRange(themeResult.Errors.Select(message => Prefix("theme", message)));
            warnings.AddRange(themeResult.Warnings.Select(message => Prefix("theme", message)));
            contrast.AddRange(themeResult.ContrastResults);
            if (!string.Equals(document.Name, document.Theme.Name, StringComparison.Ordinal))
                errors.Add(new("incompatible-name", "name", "Document name must match theme.name."));
        }

        ValidateApplication(document.Application, errors);
        ValidatePalette(document.Palette, errors);
        ValidateTypography(document, errors);
        return new(errors.AsReadOnly(), warnings.AsReadOnly(), contrast.AsReadOnly());
    }

    private static void ValidateApplication(ShadcnThemeApplication? application, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (application is null)
        {
            errors.Add(new("required-application", "application", "Application defaults are required."));
            return;
        }

        ValidateIdentifier(application.Preset, "application.preset", errors);
        ValidateIdentifier(application.Style, "application.style", errors);
        ValidateIdentifier(application.BaseColor, "application.baseColor", errors);
        ValidateIdentifier(application.IconLibrary, "application.iconLibrary", errors);
        ValidateIdentifier(application.MenuAccent, "application.menuAccent", errors);
        ValidateIdentifier(application.MenuColor, "application.menuColor", errors);
        if (!Enum.IsDefined(application.DefaultDirection))
            errors.Add(new("invalid-direction", "application.defaultDirection", "Default direction is unsupported."));
        if (!Enum.IsDefined(application.ReducedMotionBehavior))
            errors.Add(new("invalid-reduced-motion", "application.reducedMotionBehavior", "Reduced-motion behavior is unsupported."));
        if (string.IsNullOrWhiteSpace(application.DefaultLocale) || application.DefaultLocale.Length > 35 ||
            application.DefaultLocale.Any(character => !(char.IsAsciiLetterOrDigit(character) || character == '-')))
            errors.Add(new("invalid-locale", "application.defaultLocale", "Default locale must be a compact BCP 47 language tag."));
    }

    private static void ValidatePalette(ShadcnPaletteRecipe? palette, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (palette is null)
        {
            errors.Add(new("required-palette", "palette", "Palette recipe is required."));
            return;
        }

        if (palette.AlgorithmVersion is not ShadcnPaletteRecipe.MaterializedAlgorithmVersion and
            not ShadcnPaletteRecipe.LegacyAlgorithmVersion and
            not ShadcnPaletteRecipe.CurrentAlgorithmVersion)
            errors.Add(new("invalid-palette-algorithm", "palette.algorithmVersion", "Palette algorithm version must identify materialized values or a supported deterministic algorithm."));
        ValidateIdentifier(palette.BaseColor, "palette.baseColor", errors);
        if (palette.LockedTokens is null)
            errors.Add(new("required-locked-tokens", "palette.lockedTokens", "Locked token list is required."));
        else
        {
            if (palette.LockedTokens.Count > 128)
                errors.Add(new("too-many-locked-tokens", "palette.lockedTokens", "At most 128 locked tokens are supported."));
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var token in palette.LockedTokens)
            {
                if (string.IsNullOrWhiteSpace(token) || token.Length > 100 || !seen.Add(token) ||
                    token.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '.' or '-')) ||
                    !ShadcnPaletteGenerator.SupportsLock(token))
                    errors.Add(new("invalid-locked-token", "palette.lockedTokens", "Locked tokens must be unique semantic token paths."));
            }
        }

        if (palette.IsVersion2)
        {
            if (palette.Anchors is null)
                errors.Add(new("required-palette-anchors", "palette.anchors", "Palette anchors are required for algorithm version 2."));
            else if (palette.Anchors.Brand is null || palette.Anchors.Support is null ||
                     palette.Anchors.Highlight is null || palette.Anchors.DataA is null || palette.Anchors.DataB is null)
                errors.Add(new("invalid-palette-anchors", "palette.anchors", "Palette anchors must define all five non-null string values."));
            if (palette.Harmony is null)
                errors.Add(new("required-palette-harmony", "palette.harmony", "Palette harmony is required for algorithm version 2."));
            else if (!Enum.IsDefined(palette.Harmony.Value))
                errors.Add(new("invalid-palette-harmony", "palette.harmony", "Palette harmony must be a supported value."));
            if (palette.LockedAnchors is null ||
                palette.LockedAnchors.Any(role => !Enum.IsDefined(role)) ||
                palette.LockedAnchors.Distinct().Count() != palette.LockedAnchors.Count)
                errors.Add(new("invalid-locked-anchor", "palette.lockedAnchors", "Locked anchors must be unique supported roles."));
        }
        else if (palette.Anchors is not null || palette.Harmony is not null || palette.LockedAnchors is not null)
        {
            errors.Add(new("unexpected-palette-v2-field", "palette", "Version-two palette fields are not allowed on materialized or version-one recipes."));
        }
    }

    private static void ValidateTypography(ShadcnThemeDocument document, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (document.Typography is null)
        {
            errors.Add(new("required-typography", "typography", "Typography scale is required."));
            return;
        }

        ValidateFont(document.Typography.Body, "typography.body", errors);
        ValidateFont(document.Typography.ThaiFallback, "typography.thaiFallback", errors);
        ValidateFont(document.Typography.Code, "typography.code", errors);
        if (document.Theme is not null &&
            !string.Equals(document.Typography.Body.Family, document.Theme.Metrics.FontFamily, StringComparison.Ordinal))
            errors.Add(new("incompatible-font", "typography.body.family", "Body family must match theme.metrics.fontFamily."));
        if (document.Theme is not null &&
            !string.Equals(document.Typography.Code.Family, document.Theme.Metrics.MonospaceFontFamily, StringComparison.Ordinal))
            errors.Add(new("incompatible-font", "typography.code.family", "Code family must match theme.metrics.monospaceFontFamily."));
        if (document.Typography.Roles is null)
        {
            errors.Add(new("required-typography-roles", "typography.roles", "Typography roles are required."));
            return;
        }

        foreach (var requiredRole in Enum.GetValues<ShadcnTypographyRole>())
        {
            if (!document.Typography.Roles.ContainsKey(requiredRole))
            {
                errors.Add(new(
                    "required-typography-role",
                    RolePath(requiredRole),
                    $"Typography role {requiredRole} is required."));
            }
        }

        foreach (var (role, style) in document.Typography.Roles)
        {
            var path = RolePath(role);
            if (!Enum.IsDefined(role) || style is null || style.Weight is < 100 or > 900 || style.Weight % 100 != 0 ||
                !double.IsFinite(style.Scale) || style.Scale is < 0.625 or > 4 ||
                !double.IsFinite(style.LineHeight) || style.LineHeight is < 1 or > 2.5 ||
                !double.IsFinite(style.LetterSpacingEm) || style.LetterSpacingEm is < -0.1 or > 0.2)
                errors.Add(new("invalid-typography-role", path, "Typography role values are outside supported bounds."));
        }
    }

    private static string RolePath(ShadcnTypographyRole role)
    {
        var name = role.ToString();
        return $"typography.roles.{char.ToLowerInvariant(name[0])}{name[1..]}";
    }

    private static void ValidateFont(ShadcnFontSelection? font, string path, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (font is null || !SafeText(font.Family, 256) || !SafeText(font.Fallback, 256) ||
            font.GoogleFontsId is { } id && !SafeText(id, 100))
            errors.Add(new("invalid-font-selection", path, "Font selections contain unsupported or unsafe text."));
    }

    private static bool SafeText(string? value, int maximumLength) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maximumLength && !value.Any(char.IsControl) &&
        value.IndexOfAny([';', '{', '}', '<', '>']) < 0 && !value.Contains("url(", StringComparison.OrdinalIgnoreCase);

    private static void ValidateIdentifier(string? value, string path, ICollection<ShadcnThemeValidationMessage> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 100 ||
            value.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.')))
            errors.Add(new("invalid-identifier", path, "Value must be a safe portable identifier."));
    }

    private static ShadcnThemeValidationMessage Prefix(string prefix, ShadcnThemeValidationMessage message) =>
        message with { Path = $"{prefix}.{message.Path}" };
}
