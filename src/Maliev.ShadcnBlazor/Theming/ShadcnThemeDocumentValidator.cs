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

        if (palette.AlgorithmVersion < 1)
            errors.Add(new("invalid-palette-algorithm", "palette.algorithmVersion", "Palette algorithm version must be positive."));
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
                    token.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '.' or '-')))
                    errors.Add(new("invalid-locked-token", "palette.lockedTokens", "Locked tokens must be unique semantic token paths."));
            }
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

        foreach (var (role, style) in document.Typography.Roles)
        {
            var path = $"typography.roles.{char.ToLowerInvariant(role.ToString()[0])}{role.ToString()[1..]}";
            if (!Enum.IsDefined(role) || style is null || style.Weight is < 100 or > 900 || style.Weight % 100 != 0 ||
                !double.IsFinite(style.Scale) || style.Scale is < 0.5 or > 8 ||
                !double.IsFinite(style.LineHeight) || style.LineHeight is < 1 or > 3 ||
                !double.IsFinite(style.LetterSpacingEm) || style.LetterSpacingEm is < -0.1 or > 0.5)
                errors.Add(new("invalid-typography-role", path, "Typography role values are outside supported bounds."));
        }
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
