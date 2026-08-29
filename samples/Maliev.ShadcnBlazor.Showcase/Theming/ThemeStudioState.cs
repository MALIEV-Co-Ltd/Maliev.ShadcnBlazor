using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Maliev.ShadcnBlazor.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Presets;
using Maliev.ShadcnBlazor.Components.Styling;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public enum ThemeStudioScheme { Light, Dark }
public enum ThemeStudioMode { Light, Dark, System }
public enum ThemeStudioLocale { English, Thai }
public enum ThemeStudioMockup { OperationsDashboard, ManufacturingRequest, CustomerWorkspace }
public enum ThemeStudioGroup { Colors, Typography, Geometry, Shadows, Focus, Motion }
public enum ThemeStudioFontSlot { Body, ThaiFallback, Code }
public enum ThemeStudioFontLoadState { Bundled, Loading, Loaded, Failed, TimedOut }

public sealed record ThemeStudioViewport(string Id, string DisplayName, int Width)
{
    public static ThemeStudioViewport Desktop { get; } = new("desktop", "Desktop", 1280);
    public static ThemeStudioViewport Tablet { get; } = new("tablet", "Tablet", 768);
    public static ThemeStudioViewport Mobile { get; } = new("mobile", "Mobile", 390);
    public static IReadOnlyList<ThemeStudioViewport> All { get; } = [Desktop, Tablet, Mobile];
}

public sealed record ThemeStudioFontPreset(
    string Id,
    string DisplayName,
    string CssStack,
    string GoogleFontsFamily,
    bool IsBundled)
{
    public static ThemeStudioFontPreset GeistSans { get; } = new(
        "geist-sans",
        "Geist + Noto Sans Thai",
        "'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
        "Geist:wght@400;500;600;700",
        IsBundled: true);

    public static ThemeStudioFontPreset DmSans { get; } = new(
        "dm-sans",
        "DM Sans",
        "'DM Sans', ui-sans-serif, system-ui, sans-serif",
        "DM+Sans:wght@400;500;600;700",
        IsBundled: false);

    public static ThemeStudioFontPreset PlusJakartaSans { get; } = new(
        "plus-jakarta-sans",
        "Plus Jakarta Sans",
        "'Plus Jakarta Sans', ui-sans-serif, system-ui, sans-serif",
        "Plus+Jakarta+Sans:wght@400;500;600;700",
        IsBundled: false);

    public static ThemeStudioFontPreset NotoSansThai { get; } = new(
        "noto-sans-thai",
        "Noto Sans Thai",
        "'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
        "Noto+Sans+Thai:wght@400;500;600;700",
        IsBundled: true);

    public static ThemeStudioFontPreset JetBrainsMono { get; } = new(
        "jetbrains-mono",
        "JetBrains Mono",
        "'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
        "JetBrains+Mono:wght@400;500;600;700",
        IsBundled: true);

    public static IReadOnlyList<ThemeStudioFontPreset> All { get; } = [GeistSans, DmSans, PlusJakartaSans, NotoSansThai];
    public static IReadOnlyList<ThemeStudioFontPreset> MonospaceAll { get; } = [JetBrainsMono];
}

public sealed record ThemeStudioTokenDescriptor(string Name, string Label, ThemeStudioGroup Group, PropertyInfo Property);
public sealed record ThemeStudioMetricDescriptor(string Name, string Label, ThemeStudioGroup Group, PropertyInfo Property);

public static partial class ThemeStudioMetadata
{
    public static IReadOnlyList<ThemeStudioTokenDescriptor> Tokens { get; } = typeof(ShadcnColorScheme)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .OrderBy(property => property.MetadataToken)
        .Select(property => new ThemeStudioTokenDescriptor(
            CamelCase(property.Name),
            Humanize(property.Name),
            property.Name.StartsWith("Shadow", StringComparison.Ordinal) ? ThemeStudioGroup.Shadows : ThemeStudioGroup.Colors,
            property))
        .ToArray();

    public static IReadOnlyList<ThemeStudioMetricDescriptor> Metrics { get; } = typeof(ShadcnThemeMetrics)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .OrderBy(property => property.MetadataToken)
        .Select(property => new ThemeStudioMetricDescriptor(
            CamelCase(property.Name),
            Humanize(property.Name),
            MetricGroup(property.Name),
            property))
        .ToArray();

    private static ThemeStudioGroup MetricGroup(string name) => name switch
    {
        nameof(ShadcnThemeMetrics.FontFamily) or nameof(ShadcnThemeMetrics.MonospaceFontFamily) => ThemeStudioGroup.Typography,
        nameof(ShadcnThemeMetrics.FocusRingWidthPx) or nameof(ShadcnThemeMetrics.FocusRingOffsetPx) => ThemeStudioGroup.Focus,
        nameof(ShadcnThemeMetrics.MotionDurationMilliseconds) or nameof(ShadcnThemeMetrics.MotionEasing) or nameof(ShadcnThemeMetrics.ReducedMotionBehavior) => ThemeStudioGroup.Motion,
        _ => ThemeStudioGroup.Geometry
    };

    private static string CamelCase(string value) => char.ToLowerInvariant(value[0]) + value[1..];
    private static string Humanize(string value) => WordBoundary().Replace(value, " $1").Trim();

    [GeneratedRegex("([A-Z0-9]+)", RegexOptions.CultureInvariant)]
    private static partial Regex WordBoundary();
}

public sealed class ThemeStudioState
{
    private const int HistoryLimit = 50;
    private static readonly IReadOnlyList<ShadcnFontSelection> BodyShuffleFonts =
    [
        BodyFont("Geist", bundled: true),
        BodyFont("Inter", "inter"),
        BodyFont("DM Sans", "dm-sans"),
        BodyFont("Manrope", "manrope"),
        BodyFont("Plus Jakarta Sans", "plus-jakarta-sans")
    ];
    private static readonly IReadOnlyList<ShadcnFontSelection> ThaiShuffleFonts =
    [
        ThaiFont("Noto Sans Thai", bundled: true),
        ThaiFont("Prompt", "prompt"),
        ThaiFont("Sarabun", "sarabun")
    ];
    private static readonly IReadOnlyList<ShadcnFontSelection> CodeShuffleFonts =
    [
        CodeFont("JetBrains Mono", bundled: true),
        CodeFont("IBM Plex Mono", "ibm-plex-mono"),
        CodeFont("Source Code Pro", "source-code-pro"),
        CodeFont("Fira Code", "fira-code")
    ];
    private readonly IThemeStudioStorage storage;
    private readonly IThemeStudioPresetCatalog presetCatalog;
    private readonly List<ThemeStudioSnapshot> _undo = [];
    private readonly List<ThemeStudioSnapshot> _redo = [];
    private readonly Dictionary<string, string> _tokenEditorValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _metricEditorValues = new(StringComparer.Ordinal);
    private ShadcnTheme _baseline = Clone(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
    private string _baselineStyleId = "vega";
    private string _baselineBaseColorId = "neutral";
    private ThemeStudioIconLibrary _baselineIconLibrary = ThemeStudioIconLibrary.Lucide;
    private ThemeStudioMenuAccent _baselineMenuAccent = ThemeStudioMenuAccent.Default;
    private ThemeStudioMenuColor _baselineMenuColor = ThemeStudioMenuColor.Default;
    private ShadcnVisualStyle _baselineVisualStyle = ShadcnVisualStyle.Minimal;
    private ShadcnColorTreatment _baselineColorTreatment = ShadcnColorTreatment.Inherit;
    private ShadcnDepthTreatment _baselineDepthTreatment = ShadcnDepthTreatment.Flat;
    private ShadcnMotionTreatment _baselineMotionTreatment = ShadcnMotionTreatment.Calm;
    private ShadcnStyleIntensity _baselineStyleIntensity = ShadcnStyleIntensity.Default;
    private string? _pointerMutationKey;
    private bool _pointerSnapshotCaptured;
    private bool _pointerMutationOccurred;
    private bool _suppressWorkbenchChanged;
    private long _fontLoadRequest;
    private ShadcnThemeDocument _documentTemplate = ShadcnThemeDocumentSerializer.Deserialize(
        ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme()));
    private ShadcnThemeDocument _baselineDocumentTemplate = ShadcnThemeDocumentSerializer.Deserialize(
        ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme()));

    public ThemeStudioState(IThemeStudioStorage storage)
        : this(storage, new ThemeStudioWorkbenchState(), new ThemeStudioPresetCatalog())
    {
    }

    public ThemeStudioState(IThemeStudioStorage storage, ThemeStudioWorkbenchState workbench)
        : this(storage, workbench, new ThemeStudioPresetCatalog())
    {
    }

    public ThemeStudioState(IThemeStudioStorage storage, ThemeStudioWorkbenchState workbench, IThemeStudioPresetCatalog presetCatalog)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        Workbench = workbench ?? throw new ArgumentNullException(nameof(workbench));
        this.presetCatalog = presetCatalog ?? throw new ArgumentNullException(nameof(presetCatalog));
        Workbench.Changed += OnWorkbenchChanged;
    }

    public ThemeStudioWorkbenchState Workbench { get; }
    public ShadcnThemeDocument Document => CreateDocument();
    public ShadcnTypographyScale Typography => _documentTemplate.Typography;
    public ShadcnTheme Draft { get; private set; } = Clone(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
    public ShadcnTheme Applied { get; private set; } = Clone(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
    public ThemeStudioMode Mode => Workbench.Mode;
    public ShadcnDirection Direction => Workbench.Direction;
    public ThemeStudioLocale Locale => Workbench.Locale;
    public ThemeStudioViewport Viewport => Workbench.Viewport;
    public ThemeStudioMockup SelectedMockup { get; private set; } = ThemeStudioMockup.OperationsDashboard;
    public string SelectedPresetId { get; private set; } = ShadcnThemePresets.BaseVegaNeutral.Id;
    public string StyleId { get; private set; } = "vega";
    public string BaseColorId { get; private set; } = "neutral";
    public ThemeStudioIconLibrary IconLibrary { get; private set; } = ThemeStudioIconLibrary.Lucide;
    public ThemeStudioMenuAccent MenuAccent { get; private set; } = ThemeStudioMenuAccent.Default;
    public ThemeStudioMenuColor MenuColor { get; private set; } = ThemeStudioMenuColor.Default;
    public ShadcnVisualStyle VisualStyle { get; private set; } = ShadcnVisualStyle.Minimal;
    public ShadcnColorTreatment ColorTreatment { get; private set; } = ShadcnColorTreatment.Inherit;
    public ShadcnDepthTreatment DepthTreatment { get; private set; } = ShadcnDepthTreatment.Flat;
    public ShadcnMotionTreatment MotionTreatment { get; private set; } = ShadcnMotionTreatment.Calm;
    public ShadcnStyleIntensity StyleIntensity { get; private set; } = ShadcnStyleIntensity.Default;
    public ThemeStudioRadiusPreset RadiusPreset => ThemeStudioGeneratorCatalog.RadiusPreset(Draft.Metrics.RadiusRem);
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public bool IsPointerInteractionActive => _pointerMutationKey is not null;
    public bool IsDirty => _tokenEditorValues.Count > 0 || _metricEditorValues.Count > 0 || Draft != _baseline ||
        StyleId != _baselineStyleId || BaseColorId != _baselineBaseColorId || IconLibrary != _baselineIconLibrary ||
        MenuAccent != _baselineMenuAccent || MenuColor != _baselineMenuColor || VisualStyle != _baselineVisualStyle ||
        ColorTreatment != _baselineColorTreatment || DepthTreatment != _baselineDepthTreatment ||
        MotionTreatment != _baselineMotionTreatment || StyleIntensity != _baselineStyleIntensity ||
        !PaletteEquals(_documentTemplate.Palette, _baselineDocumentTemplate.Palette) ||
        !TypographyEquals(_documentTemplate.Typography, _baselineDocumentTemplate.Typography);
    public ShadcnThemeValidationResult Validation { get; private set; } = ShadcnThemeValidator.Validate(ShadcnThemePresets.BaseVegaNeutral.CreateTheme());
    public IReadOnlyList<ShadcnThemeValidationMessage> PaletteDiagnostics { get; private set; } = [];
    public ShadcnPaletteAnchors PaletteAnchors => _documentTemplate.Palette.Anchors ?? new(
        Applied.Light.Primary,
        Applied.Light.Secondary,
        Applied.Light.Accent,
        Applied.Light.Chart4,
        Applied.Light.Chart5);
    public ShadcnPaletteHarmony PaletteHarmony =>
        _documentTemplate.Palette.Harmony ?? ShadcnPaletteHarmony.Free;
    public string? StorageDiagnostic { get; private set; }
    public string? ImportDiagnostic { get; private set; }
    public bool SystemDarkMode => Workbench.SystemDarkMode;
    public bool EffectiveDarkMode => Workbench.EffectiveDarkMode;
    public ThemeStudioFontLoadState FontLoadState { get; private set; } = ThemeStudioFontLoadState.Bundled;
    public string? FontLoadDiagnostic { get; private set; }

    public event EventHandler? Changed;

    public async ValueTask InitializeAsync()
    {
        var result = await storage.LoadAsync();
        StorageDiagnostic = result.Diagnostic;
        if (result.Succeeded && result.Document is not null)
        {
            var validation = ShadcnThemeDocumentValidator.Validate(result.Document);
            if (validation.IsValid)
            {
                ApplyDocument(result.Document, captureHistory: false);
            }
            else
            {
                StorageDiagnostic = "Stored theme failed validation and was not applied.";
            }
        }

        RaiseChanged();
    }

    public async ValueTask PersistAsync()
    {
        if (!Validation.IsValid || _tokenEditorValues.Count > 0 || _metricEditorValues.Count > 0)
            return;
        var result = await storage.SaveAsync(CreateDocument());
        StorageDiagnostic = result.Diagnostic;
    }

    internal void ReportStorageDiagnostic(string diagnostic) => StorageDiagnostic = diagnostic;

    public void SetToken(ThemeStudioScheme scheme, string token, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        value ??= string.Empty;
        var descriptor = FindToken(token);
        var editorKey = TokenEditorKey(scheme, descriptor.Name);
        if (string.Equals(GetTokenValue(scheme, descriptor.Name), value, StringComparison.Ordinal))
            return;

        CaptureHistory(editorKey);
        var candidate = WithToken(Draft, scheme, descriptor, value);
        if (ShadcnThemeValidator.Validate(candidate).IsValid)
        {
            _tokenEditorValues.Remove(editorKey);
            Draft = candidate;
        }
        else
        {
            _tokenEditorValues[editorKey] = value;
        }
        RevalidateAndApply();
    }

    public void SetMetric(string metric, string editorValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);
        editorValue ??= string.Empty;
        var descriptor = FindMetric(metric);
        if (string.Equals(GetMetricEditorValue(metric), editorValue, StringComparison.Ordinal))
            return;

        CaptureHistory($"metrics.{descriptor.Name}");
        if (!TryConvertMetric(descriptor.Property.PropertyType, editorValue, out var value))
        {
            _metricEditorValues[descriptor.Name] = editorValue;
            RevalidateAndApply();
            return;
        }

        var candidate = WithMetric(Draft, descriptor, value);
        if (ShadcnThemeValidator.Validate(candidate).IsValid)
        {
            _metricEditorValues.Remove(descriptor.Name);
            Draft = candidate;
        }
        else
        {
            _metricEditorValues[descriptor.Name] = editorValue;
        }
        RevalidateAndApply();
    }

    public string GetMetricEditorValue(string metric)
    {
        var descriptor = FindMetric(metric);
        if (_metricEditorValues.TryGetValue(descriptor.Name, out var editorValue))
            return editorValue;
        var value = descriptor.Property.GetValue(Draft.Metrics);
        return value switch
        {
            double number => number.ToString("G17", CultureInfo.InvariantCulture),
            int number => number.ToString(CultureInfo.InvariantCulture),
            ShadcnReducedMotionBehavior behavior => behavior.ToString(),
            _ => value?.ToString() ?? string.Empty
        };
    }

    public string GetTokenValue(ThemeStudioScheme scheme, string token)
    {
        var descriptor = FindToken(token);
        if (_tokenEditorValues.TryGetValue(TokenEditorKey(scheme, descriptor.Name), out var editorValue))
            return editorValue;
        return descriptor.Property.GetValue(GetScheme(Draft, scheme))?.ToString() ?? string.Empty;
    }

    public void ApplyPreset(string presetId)
    {
        ApplyCuratedPreset(presetCatalog.Get(presetId), captureHistory: true);
    }

    public string ShufflePreset()
    {
        var previousTypography = Typography;
        var candidates = presetCatalog.All.Where(item => !string.Equals(item.Id, SelectedPresetId, StringComparison.Ordinal)).ToArray();
        var selected = candidates[RandomNumberGenerator.GetInt32(candidates.Length)];
        ApplyCuratedPreset(selected, captureHistory: true);
        ApplyShuffledTypography(previousTypography);
        return selected.Id;
    }

    private void ApplyShuffledTypography(ShadcnTypographyScale previous)
    {
        var typography = _documentTemplate.Typography with
        {
            Body = SelectDifferentFont(previous.Body, BodyShuffleFonts),
            ThaiFallback = SelectDifferentFont(previous.ThaiFallback, ThaiShuffleFonts),
            Code = SelectDifferentFont(previous.Code, CodeShuffleFonts)
        };
        var metrics = Draft.Metrics with
        {
            FontFamily = typography.Body.Family,
            MonospaceFontFamily = typography.Code.Family
        };
        var theme = Draft with { Metrics = metrics };
        EnsureTypographyValid(theme, typography);

        _documentTemplate = _documentTemplate with { Typography = typography };
        _baselineDocumentTemplate = _baselineDocumentTemplate with { Typography = typography };
        Draft = theme;
        Applied = Clone(theme);
        _baseline = Clone(theme);
        RevalidateAndApply();
    }

    private static ShadcnFontSelection SelectDifferentFont(
        ShadcnFontSelection current,
        IReadOnlyList<ShadcnFontSelection> candidates)
    {
        var available = candidates
            .Where(candidate => !string.Equals(candidate.Family, current.Family, StringComparison.Ordinal))
            .ToArray();
        return available[RandomNumberGenerator.GetInt32(available.Length)];
    }

    private static ShadcnFontSelection BodyFont(string family, string? googleFontsId = null, bool bundled = false) => new(
        $"'{family}', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
        "ui-sans-serif, system-ui, sans-serif",
        bundled ? null : googleFontsId);

    private static ShadcnFontSelection ThaiFont(string family, string? googleFontsId = null, bool bundled = false) => new(
        family == "Noto Sans Thai" ? "'Noto Sans Thai', sans-serif" : $"'{family}', 'Noto Sans Thai', sans-serif",
        "'Noto Sans Thai', sans-serif",
        bundled ? null : googleFontsId);

    private static ShadcnFontSelection CodeFont(string family, string? googleFontsId = null, bool bundled = false) => new(
        $"'{family}', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
        "ui-monospace, monospace",
        bundled ? null : googleFontsId);

    public IReadOnlyList<ThemeStudioPresetDefinition> CuratedPresets => presetCatalog.All;

    private void ApplyCuratedPreset(ThemeStudioPresetDefinition preset, bool captureHistory)
    {
        var document = preset.CreateDocument();
        var validation = ShadcnThemeDocumentValidator.Validate(document);
        if (!validation.IsValid)
            throw new ArgumentException($"Curated preset '{preset.Id}' is invalid.", nameof(preset));
        if (captureHistory)
            CaptureHistory("preset");
        _documentTemplate = document;
        _baselineDocumentTemplate = document;
        Draft = Clone(document.Theme);
        Applied = Clone(document.Theme);
        _baseline = Clone(document.Theme);
        _tokenEditorValues.Clear();
        _metricEditorValues.Clear();
        SelectedPresetId = preset.Id;
        StyleId = preset.Style;
        BaseColorId = preset.BaseColor;
        IconLibrary = preset.IconLibrary;
        MenuAccent = ParseOption<ThemeStudioMenuAccent>(document.Application.MenuAccent);
        MenuColor = ParseOption<ThemeStudioMenuColor>(document.Application.MenuColor);
        VisualStyle = preset.VisualStyle;
        ColorTreatment = preset.ColorTreatment;
        DepthTreatment = preset.DepthTreatment;
        MotionTreatment = preset.MotionTreatment;
        StyleIntensity = preset.StyleIntensity;
        _baselineStyleId = StyleId;
        _baselineBaseColorId = BaseColorId;
        _baselineIconLibrary = IconLibrary;
        _baselineMenuAccent = MenuAccent;
        _baselineMenuColor = MenuColor;
        _baselineVisualStyle = VisualStyle;
        _baselineColorTreatment = ColorTreatment;
        _baselineDepthTreatment = DepthTreatment;
        _baselineMotionTreatment = MotionTreatment;
        _baselineStyleIntensity = StyleIntensity;
        RevalidateAndApply();
    }

    public void ResetToken(ThemeStudioScheme scheme, string token)
    {
        var descriptor = FindToken(token);
        var baselineValue = descriptor.Property.GetValue(GetScheme(_baseline, scheme))?.ToString() ?? string.Empty;
        SetToken(scheme, token, baselineValue);
    }

    public void ResetGroup(ThemeStudioGroup group, ThemeStudioScheme? scheme = null)
    {
        if (group is ThemeStudioGroup.Colors or ThemeStudioGroup.Shadows)
        {
            if (scheme is null)
                throw new ArgumentNullException(nameof(scheme), "A color scheme is required for color and shadow resets.");
            var descriptors = ThemeStudioMetadata.Tokens.Where(item => item.Group == group).ToArray();
            if (descriptors.All(item => Equals(item.Property.GetValue(GetScheme(Draft, scheme.Value)), item.Property.GetValue(GetScheme(_baseline, scheme.Value)))) &&
                descriptors.All(item => !_tokenEditorValues.ContainsKey(TokenEditorKey(scheme.Value, item.Name))))
                return;
            CaptureHistory($"reset.{scheme}.{group}");
            var target = GetScheme(Draft, scheme.Value) with { };
            var baseline = GetScheme(_baseline, scheme.Value);
            foreach (var descriptor in descriptors)
            {
                descriptor.Property.SetValue(target, descriptor.Property.GetValue(baseline));
                _tokenEditorValues.Remove(TokenEditorKey(scheme.Value, descriptor.Name));
            }
            Draft = scheme == ThemeStudioScheme.Light ? Draft with { Light = target } : Draft with { Dark = target };
        }
        else
        {
            var descriptors = ThemeStudioMetadata.Metrics.Where(item => item.Group == group).ToArray();
            if (descriptors.Length == 0)
                return;
            CaptureHistory($"reset.metrics.{group}");
            var target = Draft.Metrics with { };
            foreach (var descriptor in descriptors)
            {
                descriptor.Property.SetValue(target, descriptor.Property.GetValue(_baseline.Metrics));
                _metricEditorValues.Remove(descriptor.Name);
            }
            Draft = Draft with { Metrics = target };
            if (group == ThemeStudioGroup.Typography)
                _documentTemplate = _documentTemplate with { Typography = _baselineDocumentTemplate.Typography };
        }
        RevalidateAndApply();
    }

    public void ResetAll()
    {
        if (!IsDirty)
            return;
        CaptureHistory("reset.all");
        Draft = Clone(_baseline);
        _documentTemplate = _baselineDocumentTemplate;
        _tokenEditorValues.Clear();
        _metricEditorValues.Clear();
        StyleId = _baselineStyleId;
        BaseColorId = _baselineBaseColorId;
        IconLibrary = _baselineIconLibrary;
        MenuAccent = _baselineMenuAccent;
        MenuColor = _baselineMenuColor;
        VisualStyle = _baselineVisualStyle;
        ColorTreatment = _baselineColorTreatment;
        DepthTreatment = _baselineDepthTreatment;
        MotionTreatment = _baselineMotionTreatment;
        StyleIntensity = _baselineStyleIntensity;
        RevalidateAndApply();
    }

    public bool Undo()
    {
        if (_undo.Count == 0)
            return false;
        _redo.Add(CreateSnapshot());
        Restore(Pop(_undo));
        return true;
    }

    public bool Redo()
    {
        if (_redo.Count == 0)
            return false;
        AddBounded(_undo, CreateSnapshot());
        Restore(Pop(_redo));
        return true;
    }

    public bool Import(ShadcnTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        try
        {
            var document = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme));
            return ImportDocument(document);
        }
        catch (Exception exception) when (exception is ArgumentException or JsonException or NotSupportedException)
        {
            ImportDiagnostic = $"Imported theme was not applied: {exception.Message}";
            RaiseChanged();
            return false;
        }
    }

    public ShadcnThemeDocument CreateDocument()
    {
        var theme = Clone(Applied);
        var migrated = ShadcnThemeDocumentSerializer.Deserialize(ShadcnThemeSerializer.Serialize(theme));
        return migrated with
        {
            Name = theme.Name,
            Theme = theme,
            Application = new ShadcnThemeApplication(
                SelectedPresetId,
                StyleId,
                BaseColorId,
                IconLibrary.ToString().ToLowerInvariant(),
                MenuAccent.ToString().ToLowerInvariant(),
                MenuColor.ToString().ToLowerInvariant(),
                Mode == ThemeStudioMode.Dark,
                Direction,
                Locale == ThemeStudioLocale.Thai ? "th" : "en",
                theme.Metrics.ReducedMotionBehavior),
            Palette = _documentTemplate.Palette with { BaseColor = BaseColorId },
            Typography = _documentTemplate.Typography with
            {
                Body = _documentTemplate.Typography.Body with { Family = theme.Metrics.FontFamily },
                Code = _documentTemplate.Typography.Code with { Family = theme.Metrics.MonospaceFontFamily }
            }
        };
    }

    public string SerializeDocument() => ShadcnThemeDocumentSerializer.Serialize(CreateDocument());

    public bool GeneratePalette(ulong seed)
    {
        var recipe = ShadcnPaletteRecipe.CreateV2(
            seed,
            BaseColorId,
            _documentTemplate.Palette.LockedTokens,
            PaletteAnchors,
            PaletteHarmony,
            _documentTemplate.Palette.LockedAnchors ?? []);
        return TryApplyPaletteRecipe(recipe, "palette.generate");
    }

    public bool GeneratePalette(string seedText)
    {
        if (ulong.TryParse(seedText, NumberStyles.None, CultureInfo.InvariantCulture, out var seed))
            return GeneratePalette(seed);
        PaletteDiagnostics = [new("palette-invalid-seed", "palette.seed", "Seed must be an unsigned 64-bit decimal number.")];
        RaiseChanged();
        return false;
    }

    public ulong GenerateNewPalette()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        var seed = BitConverter.ToUInt64(bytes);
        GeneratePalette(seed);
        return seed;
    }

    public bool IsPaletteLocked(ThemeStudioScheme scheme, string token) =>
        _documentTemplate.Palette.LockedTokens.Contains(TokenEditorKey(scheme, token), StringComparer.Ordinal);

    public void SetPaletteLock(ThemeStudioScheme scheme, string token, bool locked)
    {
        _ = FindToken(token);
        var path = TokenEditorKey(scheme, token);
        var locks = _documentTemplate.Palette.LockedTokens.ToHashSet(StringComparer.Ordinal);
        if (locked ? !locks.Add(path) : !locks.Remove(path))
            return;
        CaptureHistory($"palette.lock.{path}");
        var lockedTokens = locks.Order(StringComparer.Ordinal).ToArray();
        var palette = _documentTemplate.Palette.IsVersion2
            ? ShadcnPaletteRecipe.CreateV2(
                _documentTemplate.Palette.Seed,
                BaseColorId,
                lockedTokens,
                PaletteAnchors,
                PaletteHarmony,
                _documentTemplate.Palette.LockedAnchors ?? [])
            : new ShadcnPaletteRecipe(
                _documentTemplate.Palette.AlgorithmVersion,
                _documentTemplate.Palette.Seed,
                BaseColorId,
                lockedTokens);
        _documentTemplate = _documentTemplate with { Palette = palette };
        PaletteDiagnostics = [];
        RaiseChanged();
    }

    public bool SetPaletteAnchor(ShadcnPaletteAnchorRole role, string value)
    {
        ValidateWorkspaceValue(role, nameof(role));
        value ??= string.Empty;
        var locks = (_documentTemplate.Palette.LockedAnchors ?? []).ToHashSet();
        locks.Add(role);
        var recipe = ShadcnPaletteRecipe.CreateV2(
            _documentTemplate.Palette.Seed,
            BaseColorId,
            _documentTemplate.Palette.LockedTokens,
            PaletteAnchors.Set(role, value),
            PaletteHarmony,
            locks);
        return TryApplyPaletteRecipe(recipe, PaletteMutationKey(role));
    }

    public bool SetPaletteAnchorLock(ShadcnPaletteAnchorRole role, bool locked)
    {
        ValidateWorkspaceValue(role, nameof(role));
        var locks = (_documentTemplate.Palette.LockedAnchors ?? []).ToHashSet();
        if (locked ? !locks.Add(role) : !locks.Remove(role))
            return true;
        var recipe = ShadcnPaletteRecipe.CreateV2(
            _documentTemplate.Palette.Seed,
            BaseColorId,
            _documentTemplate.Palette.LockedTokens,
            PaletteAnchors,
            PaletteHarmony,
            locks);
        return TryApplyPaletteRecipe(recipe, $"palette.lock.{PaletteRoleName(role)}");
    }

    public bool SetPaletteHarmony(ShadcnPaletteHarmony harmony)
    {
        ValidateWorkspaceValue(harmony, nameof(harmony));
        if (_documentTemplate.Palette.IsVersion2 && PaletteHarmony == harmony)
            return true;
        var recipe = ShadcnPaletteRecipe.CreateV2(
            _documentTemplate.Palette.Seed,
            BaseColorId,
            _documentTemplate.Palette.LockedTokens,
            PaletteAnchors,
            harmony,
            _documentTemplate.Palette.LockedAnchors ?? []);
        return TryApplyPaletteRecipe(recipe, "palette.harmony");
    }

    public string CreatePaletteShare()
    {
        var bytes = Encoding.UTF8.GetBytes(SerializeDocument());
        return "palette-v1:" + Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public bool ImportPaletteShare(string value)
    {
        try
        {
            return ImportDocument(ThemeStudioPaletteShareCodec.Decode(value));
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or JsonException or NotSupportedException)
        {
            ImportDiagnostic = $"Palette share was not applied: {exception.Message}";
            RaiseChanged();
            return false;
        }
    }

    public bool ImportDocument(ShadcnThemeDocument document) => ApplyDocument(document, captureHistory: true);

    public bool ImportDocument(string json)
    {
        try
        {
            return ImportDocument(ShadcnThemeDocumentSerializer.Deserialize(json));
        }
        catch (Exception exception) when (exception is ArgumentException or JsonException or NotSupportedException)
        {
            ImportDiagnostic = $"Theme document was not applied: {exception.Message}";
            RaiseChanged();
            return false;
        }
    }

    private bool TryApplyPaletteRecipe(ShadcnPaletteRecipe recipe, string mutationKey)
    {
        var result = ShadcnPaletteGenerator.Generate(Applied, recipe);
        if (!result.IsValid)
        {
            PaletteDiagnostics = result.Errors.Concat(result.Warnings).ToArray();
            RaiseChanged();
            return false;
        }

        recipe = NormalizeLockedPaletteAnchors(recipe);
        CaptureHistory(mutationKey);
        Draft = Clone(result.Theme);
        Applied = Clone(result.Theme);
        _documentTemplate = _documentTemplate with { Theme = Clone(result.Theme), Palette = recipe };
        _tokenEditorValues.Clear();
        PaletteDiagnostics = result.Warnings;
        Validation = ShadcnThemeValidator.Validate(Draft);
        if (_pointerMutationKey is not null)
            _pointerMutationOccurred = true;
        RaiseChanged();
        return true;
    }

    private ShadcnPaletteRecipe NormalizeLockedPaletteAnchors(ShadcnPaletteRecipe recipe)
    {
        if (!recipe.IsVersion2 || recipe.LockedAnchors is not { Count: > 0 })
            return recipe;
        var projectionRecipe = ShadcnPaletteRecipe.CreateV2(
            recipe.Seed,
            recipe.BaseColor,
            [],
            recipe.Anchors!,
            recipe.Harmony!.Value,
            recipe.LockedAnchors);
        var projection = ShadcnPaletteGenerator.Generate(Applied, projectionRecipe).Theme.Light;
        var anchors = recipe.Anchors!;
        foreach (var role in recipe.LockedAnchors)
        {
            anchors = anchors.Set(role, role switch
            {
                ShadcnPaletteAnchorRole.Brand => projection.Chart1,
                ShadcnPaletteAnchorRole.Support => projection.Chart2,
                ShadcnPaletteAnchorRole.Highlight => projection.Chart3,
                ShadcnPaletteAnchorRole.DataA => projection.Chart4,
                ShadcnPaletteAnchorRole.DataB => projection.Chart5,
                _ => throw new ArgumentOutOfRangeException(nameof(recipe), role, "Unknown palette anchor role.")
            });
        }
        return ShadcnPaletteRecipe.CreateV2(
            recipe.Seed,
            recipe.BaseColor,
            recipe.LockedTokens,
            anchors,
            recipe.Harmony.Value,
            recipe.LockedAnchors);
    }

    private bool ApplyDocument(ShadcnThemeDocument document, bool captureHistory)
    {
        ArgumentNullException.ThrowIfNull(document);
        var validation = ShadcnThemeDocumentValidator.Validate(document);
        if (!validation.IsValid)
        {
            ImportDiagnostic = "Theme document failed validation; the current theme was not changed.";
            RaiseChanged();
            return false;
        }

        ThemeStudioIconLibrary iconLibrary;
        ThemeStudioMenuAccent menuAccent;
        ThemeStudioMenuColor menuColor;
        if (!ThemeStudioGeneratorCatalog.IsKnownStyle(document.Application.Style) ||
            !ThemeStudioGeneratorCatalog.IsKnownBaseColor(document.Application.BaseColor))
        {
            ImportDiagnostic = "Theme document was not applied: application style or base color is unsupported.";
            RaiseChanged();
            return false;
        }
        try
        {
            iconLibrary = ParseOption<ThemeStudioIconLibrary>(document.Application.IconLibrary);
            menuAccent = ParseOption<ThemeStudioMenuAccent>(document.Application.MenuAccent);
            menuColor = ParseOption<ThemeStudioMenuColor>(document.Application.MenuColor);
        }
        catch (JsonException exception)
        {
            ImportDiagnostic = $"Theme document was not applied: {exception.Message}";
            RaiseChanged();
            return false;
        }

        if (captureHistory)
            CaptureHistory("document.import");
        _documentTemplate = document;
        _baselineDocumentTemplate = document;
        Draft = Clone(document.Theme);
        Applied = Clone(document.Theme);
        _baseline = Clone(document.Theme);
        SelectedPresetId = document.Application.Preset;
        StyleId = document.Application.Style;
        BaseColorId = document.Application.BaseColor;
        IconLibrary = iconLibrary;
        MenuAccent = menuAccent;
        MenuColor = menuColor;
        _suppressWorkbenchChanged = true;
        try
        {
            Workbench.SetDirection(document.Application.DefaultDirection);
            Workbench.SetLocale(string.Equals(document.Application.DefaultLocale, "th", StringComparison.OrdinalIgnoreCase)
                ? ThemeStudioLocale.Thai
                : ThemeStudioLocale.English);
            Workbench.SetMode(document.Application.DefaultDarkMode ? ThemeStudioMode.Dark : ThemeStudioMode.Light);
        }
        finally
        {
            _suppressWorkbenchChanged = false;
        }
        _baselineStyleId = StyleId;
        _baselineBaseColorId = BaseColorId;
        _baselineIconLibrary = IconLibrary;
        _baselineMenuAccent = MenuAccent;
        _baselineMenuColor = MenuColor;
        _tokenEditorValues.Clear();
        _metricEditorValues.Clear();
        ImportDiagnostic = null;
        PaletteDiagnostics = [];
        RevalidateAndApply();
        return true;
    }

    private static T ParseOption<T>(string value) where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new JsonException($"Unsupported Theme Studio option '{value}'.");

    public void BeginPointerInteraction(string mutationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mutationKey);
        _pointerMutationKey = mutationKey.ToLowerInvariant();
        _pointerSnapshotCaptured = false;
        _pointerMutationOccurred = false;
    }

    public void EndPointerInteraction()
    {
        var raiseChanged = _pointerMutationOccurred;
        _pointerMutationKey = null;
        _pointerSnapshotCaptured = false;
        _pointerMutationOccurred = false;
        if (raiseChanged)
            RaiseChanged();
    }

    public void SetMode(ThemeStudioMode mode)
        => Workbench.SetMode(mode);

    public void SetSystemDarkMode(bool isDarkMode)
        => Workbench.SetSystemDarkMode(isDarkMode);

    public void SetDirection(ShadcnDirection direction)
        => Workbench.SetDirection(direction);

    public void SetLocale(ThemeStudioLocale locale)
        => Workbench.SetLocale(locale);

    public void SetSelectedMockup(ThemeStudioMockup mockup)
    {
        ValidateWorkspaceValue(mockup, nameof(mockup));
        if (SelectedMockup == mockup) return;
        SelectedMockup = mockup;
        RaiseChanged();
    }

    public void SetStyle(string styleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(styleId);
        if (!ThemeStudioGeneratorCatalog.IsKnownStyle(styleId))
            throw new ArgumentOutOfRangeException(nameof(styleId), styleId, "Unknown Theme Studio style.");
        if (string.Equals(StyleId, styleId, StringComparison.Ordinal)) return;
        CaptureHistory("generator.style");
        StyleId = styleId;
        RaiseChanged();
    }

    public void SetBaseColor(string baseColorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseColorId);
        if (!ThemeStudioGeneratorCatalog.IsKnownBaseColor(baseColorId))
            throw new ArgumentOutOfRangeException(nameof(baseColorId), baseColorId, "Unknown Theme Studio base color.");
        if (string.Equals(BaseColorId, baseColorId, StringComparison.Ordinal)) return;
        CaptureHistory("generator.base-color");
        BaseColorId = baseColorId;
        RaiseChanged();
    }

    public void SetIconLibrary(ThemeStudioIconLibrary iconLibrary)
    {
        ValidateWorkspaceValue(iconLibrary, nameof(iconLibrary));
        if (IconLibrary == iconLibrary) return;
        CaptureHistory("generator.icon-library");
        IconLibrary = iconLibrary;
        RaiseChanged();
    }

    public void SetMenuAccent(ThemeStudioMenuAccent menuAccent)
    {
        ValidateWorkspaceValue(menuAccent, nameof(menuAccent));
        if (MenuAccent == menuAccent) return;
        CaptureHistory("generator.menu-accent");
        MenuAccent = menuAccent;
        RaiseChanged();
    }

    public void SetMenuColor(ThemeStudioMenuColor menuColor)
    {
        ValidateWorkspaceValue(menuColor, nameof(menuColor));
        if (MenuColor == menuColor) return;
        CaptureHistory("generator.menu-color");
        MenuColor = menuColor;
        RaiseChanged();
    }

    public void SetRadiusPreset(ThemeStudioRadiusPreset radiusPreset)
    {
        ValidateWorkspaceValue(radiusPreset, nameof(radiusPreset));
        SetMetric("radiusRem", ThemeStudioGeneratorCatalog.RadiusRem(radiusPreset).ToString("G17", CultureInfo.InvariantCulture));
    }

    public void SetVisualStyle(ShadcnVisualStyle value) => SetVisualTreatment(value, VisualStyle, "visual-style", next => VisualStyle = next);
    public void SetColorTreatment(ShadcnColorTreatment value) => SetVisualTreatment(value, ColorTreatment, "color-treatment", next => ColorTreatment = next);
    public void SetDepthTreatment(ShadcnDepthTreatment value) => SetVisualTreatment(value, DepthTreatment, "depth-treatment", next => DepthTreatment = next);
    public void SetMotionTreatment(ShadcnMotionTreatment value) => SetVisualTreatment(value, MotionTreatment, "motion-treatment", next => MotionTreatment = next);
    public void SetStyleIntensity(ShadcnStyleIntensity value) => SetVisualTreatment(value, StyleIntensity, "style-intensity", next => StyleIntensity = next);

    private void SetVisualTreatment<T>(T value, T current, string key, Action<T> assign) where T : struct, Enum
    {
        ValidateWorkspaceValue(value, key);
        if (EqualityComparer<T>.Default.Equals(value, current)) return;
        CaptureHistory($"visual.{key}");
        assign(value);
        RaiseChanged();
    }

    public void SetViewport(ThemeStudioViewport viewport)
        => Workbench.SetViewport(viewport);

    public void SetFontFamily(string presetOrCssStack)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presetOrCssStack);
        var preset = ThemeStudioFontPreset.All.FirstOrDefault(item =>
                string.Equals(item.Id, presetOrCssStack, StringComparison.Ordinal) ||
                string.Equals(item.CssStack, presetOrCssStack, StringComparison.Ordinal))
            ?? throw new ArgumentOutOfRangeException(nameof(presetOrCssStack), presetOrCssStack, "Unknown Theme Studio font preset.");
        SetTypographyFont(
            ThemeStudioFontSlot.Body,
            new ShadcnFontSelection(
                preset.CssStack,
                _documentTemplate.Typography.Body.Fallback,
                preset.IsBundled ? null : preset.Id));
    }

    public void SetMonospaceFontFamily(string presetOrCssStack)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presetOrCssStack);
        var preset = ThemeStudioFontPreset.MonospaceAll.FirstOrDefault(item =>
                string.Equals(item.Id, presetOrCssStack, StringComparison.Ordinal) ||
                string.Equals(item.CssStack, presetOrCssStack, StringComparison.Ordinal))
            ?? throw new ArgumentOutOfRangeException(nameof(presetOrCssStack), presetOrCssStack, "Unknown Theme Studio monospace font preset.");
        SetTypographyFont(
            ThemeStudioFontSlot.Code,
            new ShadcnFontSelection(
                preset.CssStack,
                _documentTemplate.Typography.Code.Fallback,
                preset.IsBundled ? null : preset.Id));
    }

    public void SetTypographyFont(ThemeStudioFontSlot slot, ShadcnFontSelection selection)
    {
        ValidateWorkspaceValue(slot, nameof(slot));
        ArgumentNullException.ThrowIfNull(selection);
        var current = slot switch
        {
            ThemeStudioFontSlot.Body => _documentTemplate.Typography.Body,
            ThemeStudioFontSlot.ThaiFallback => _documentTemplate.Typography.ThaiFallback,
            ThemeStudioFontSlot.Code => _documentTemplate.Typography.Code,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown typography font slot.")
        };
        if (current == selection)
            return;

        var typography = slot switch
        {
            ThemeStudioFontSlot.Body => _documentTemplate.Typography with { Body = selection },
            ThemeStudioFontSlot.ThaiFallback => _documentTemplate.Typography with { ThaiFallback = selection },
            ThemeStudioFontSlot.Code => _documentTemplate.Typography with { Code = selection },
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown typography font slot.")
        };
        var metrics = Draft.Metrics with
        {
            FontFamily = slot == ThemeStudioFontSlot.Body ? selection.Family : Draft.Metrics.FontFamily,
            MonospaceFontFamily = slot == ThemeStudioFontSlot.Code ? selection.Family : Draft.Metrics.MonospaceFontFamily
        };
        var theme = Draft with { Metrics = metrics };
        EnsureTypographyValid(theme, typography);

        CaptureHistory($"typography.font.{slot.ToString().ToLowerInvariant()}");
        _documentTemplate = _documentTemplate with { Typography = typography };
        Draft = theme;
        Applied = Clone(theme);
        if (slot == ThemeStudioFontSlot.Body)
            _metricEditorValues.Remove("fontFamily");
        else if (slot == ThemeStudioFontSlot.Code)
            _metricEditorValues.Remove("monospaceFontFamily");
        RevalidateAndApply();
    }

    public void SetTypographyRole(ShadcnTypographyRole role, ShadcnTypographyRoleStyle style)
    {
        ValidateWorkspaceValue(role, nameof(role));
        ArgumentNullException.ThrowIfNull(style);
        if (_documentTemplate.Typography.Roles.TryGetValue(role, out var current) && current == style)
            return;

        var roles = _documentTemplate.Typography.Roles.ToDictionary();
        roles[role] = style;
        var typography = new ShadcnTypographyScale(
            _documentTemplate.Typography.Body,
            _documentTemplate.Typography.ThaiFallback,
            _documentTemplate.Typography.Code,
            roles);
        EnsureTypographyValid(Draft, typography);
        CaptureHistory($"typography.role.{role.ToString().ToLowerInvariant()}");
        _documentTemplate = _documentTemplate with { Typography = typography };
        RaiseChanged();
    }

    public long BeginFontLoad(string stylesheet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stylesheet);
        var request = ++_fontLoadRequest;
        FontLoadState = ThemeStudioFontLoadState.Loading;
        FontLoadDiagnostic = null;
        RaiseChanged();
        return request;
    }

    public bool CompleteFontLoad(long request, ThemeStudioFontLoadState state)
    {
        if (request != _fontLoadRequest || state is ThemeStudioFontLoadState.Bundled or ThemeStudioFontLoadState.Loading)
            return false;
        FontLoadState = state;
        FontLoadDiagnostic = state switch
        {
            ThemeStudioFontLoadState.Failed => "The selected web font could not be loaded; the configured fallback remains active.",
            ThemeStudioFontLoadState.TimedOut => "The selected web font timed out; the configured fallback remains active.",
            _ => null
        };
        RaiseChanged();
        return true;
    }

    public void UseBundledFonts()
    {
        _fontLoadRequest++;
        FontLoadState = ThemeStudioFontLoadState.Bundled;
        FontLoadDiagnostic = null;
        RaiseChanged();
    }

    private static void ValidateWorkspaceValue<T>(T value, string parameterName) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Unknown Theme Studio {parameterName}.");
    }

    private void CaptureHistory(string mutationKey)
    {
        if (_pointerMutationKey is not null && string.Equals(_pointerMutationKey, mutationKey, StringComparison.Ordinal))
        {
            if (_pointerSnapshotCaptured)
                return;
            _pointerSnapshotCaptured = true;
        }
        AddBounded(_undo, CreateSnapshot());
        _redo.Clear();
    }

    private ThemeStudioSnapshot CreateSnapshot() => new(
        Clone(Draft),
        Clone(Applied),
        Clone(_baseline),
        _documentTemplate,
        _baselineDocumentTemplate,
        ImmutableArray.CreateRange(PaletteDiagnostics),
        new Dictionary<string, string>(_tokenEditorValues, StringComparer.Ordinal),
        new Dictionary<string, string>(_metricEditorValues, StringComparer.Ordinal),
        SelectedPresetId,
        StyleId,
        BaseColorId,
        IconLibrary,
        MenuAccent,
        MenuColor,
        VisualStyle,
        ColorTreatment,
        DepthTreatment,
        MotionTreatment,
        StyleIntensity,
        _baselineStyleId,
        _baselineBaseColorId,
        _baselineIconLibrary,
        _baselineMenuAccent,
        _baselineMenuColor,
        _baselineVisualStyle,
        _baselineColorTreatment,
        _baselineDepthTreatment,
        _baselineMotionTreatment,
        _baselineStyleIntensity);

    private void Restore(ThemeStudioSnapshot snapshot)
    {
        Draft = Clone(snapshot.Draft);
        Applied = Clone(snapshot.Applied);
        _baseline = Clone(snapshot.Baseline);
        _documentTemplate = snapshot.DocumentTemplate;
        _baselineDocumentTemplate = snapshot.BaselineDocumentTemplate;
        SelectedPresetId = snapshot.SelectedPresetId;
        StyleId = snapshot.StyleId;
        BaseColorId = snapshot.BaseColorId;
        IconLibrary = snapshot.IconLibrary;
        MenuAccent = snapshot.MenuAccent;
        MenuColor = snapshot.MenuColor;
        VisualStyle = snapshot.VisualStyle;
        ColorTreatment = snapshot.ColorTreatment;
        DepthTreatment = snapshot.DepthTreatment;
        MotionTreatment = snapshot.MotionTreatment;
        StyleIntensity = snapshot.StyleIntensity;
        _baselineStyleId = snapshot.BaselineStyleId;
        _baselineBaseColorId = snapshot.BaselineBaseColorId;
        _baselineIconLibrary = snapshot.BaselineIconLibrary;
        _baselineMenuAccent = snapshot.BaselineMenuAccent;
        _baselineMenuColor = snapshot.BaselineMenuColor;
        _baselineVisualStyle = snapshot.BaselineVisualStyle;
        _baselineColorTreatment = snapshot.BaselineColorTreatment;
        _baselineDepthTreatment = snapshot.BaselineDepthTreatment;
        _baselineMotionTreatment = snapshot.BaselineMotionTreatment;
        _baselineStyleIntensity = snapshot.BaselineStyleIntensity;
        _tokenEditorValues.Clear();
        foreach (var pair in snapshot.TokenEditorValues)
            _tokenEditorValues[pair.Key] = pair.Value;
        _metricEditorValues.Clear();
        foreach (var pair in snapshot.MetricEditorValues)
            _metricEditorValues[pair.Key] = pair.Value;
        PaletteDiagnostics = snapshot.PaletteDiagnostics;
        RevalidateAndApply(applyWhenValid: false);
    }

    private void RevalidateAndApply(bool applyWhenValid = true)
    {
        var validation = ShadcnThemeValidator.Validate(Draft);
        if (_tokenEditorValues.Count > 0 || _metricEditorValues.Count > 0)
        {
            var editorErrors = new List<ShadcnThemeValidationMessage>();
            foreach (var item in _tokenEditorValues)
            {
                var separator = item.Key.IndexOf('.', StringComparison.Ordinal);
                var scheme = item.Key[..separator] == "light" ? ThemeStudioScheme.Light : ThemeStudioScheme.Dark;
                var descriptor = FindToken(item.Key[(separator + 1)..]);
                var candidateValidation = ShadcnThemeValidator.Validate(WithToken(Draft, scheme, descriptor, item.Value));
                if (candidateValidation.Errors.Count > 0)
                    editorErrors.AddRange(candidateValidation.Errors);
                else
                    editorErrors.Add(new ShadcnThemeValidationMessage("invalid-editor-value", item.Key, "Color value is not valid for this field."));
            }
            foreach (var item in _metricEditorValues)
            {
                var descriptor = FindMetric(item.Key);
                if (TryConvertMetric(descriptor.Property.PropertyType, item.Value, out var converted))
                {
                    var candidateValidation = ShadcnThemeValidator.Validate(WithMetric(Draft, descriptor, converted));
                    if (candidateValidation.Errors.Count > 0)
                    {
                        editorErrors.AddRange(candidateValidation.Errors);
                        continue;
                    }
                }
                editorErrors.Add(new ShadcnThemeValidationMessage("invalid-editor-value", $"metrics.{item.Key}", "Metric value is not valid for this field."));
            }
            var errors = validation.Errors.Concat(editorErrors).ToArray();
            validation = validation with { Errors = errors };
        }
        Validation = validation;
        if (applyWhenValid && validation.IsValid)
            Applied = Clone(Draft);
        RaiseChanged();
    }

    private static string TokenEditorKey(ThemeStudioScheme scheme, string token) =>
        $"{scheme.ToString().ToLowerInvariant()}.{token}";

    private static ShadcnTheme WithToken(
        ShadcnTheme theme,
        ThemeStudioScheme scheme,
        ThemeStudioTokenDescriptor descriptor,
        string value)
    {
        var nextScheme = GetScheme(theme, scheme) with { };
        descriptor.Property.SetValue(nextScheme, value);
        return scheme == ThemeStudioScheme.Light
            ? theme with { Light = nextScheme }
            : theme with { Dark = nextScheme };
    }

    private static ShadcnTheme WithMetric(
        ShadcnTheme theme,
        ThemeStudioMetricDescriptor descriptor,
        object? value)
    {
        var metrics = theme.Metrics with { };
        descriptor.Property.SetValue(metrics, value);
        return theme with { Metrics = metrics };
    }

    private static ShadcnColorScheme GetScheme(ShadcnTheme theme, ThemeStudioScheme scheme) => scheme switch
    {
        ThemeStudioScheme.Light => theme.Light,
        ThemeStudioScheme.Dark => theme.Dark,
        _ => throw new ArgumentOutOfRangeException(nameof(scheme), scheme, "Unknown Theme Studio color scheme.")
    };

    private static ThemeStudioTokenDescriptor FindToken(string token) =>
        ThemeStudioMetadata.Tokens.FirstOrDefault(item => string.Equals(item.Name, token, StringComparison.Ordinal))
        ?? throw new ArgumentException($"Unknown theme token '{token}'.", nameof(token));

    private static ThemeStudioMetricDescriptor FindMetric(string metric) =>
        ThemeStudioMetadata.Metrics.FirstOrDefault(item => string.Equals(item.Name, metric, StringComparison.Ordinal))
        ?? throw new ArgumentException($"Unknown theme metric '{metric}'.", nameof(metric));

    private static bool TryConvertMetric(Type type, string value, out object? converted)
    {
        if (type == typeof(string))
        {
            converted = value;
            return true;
        }
        if (type == typeof(double) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            converted = doubleValue;
            return true;
        }
        if (type == typeof(int) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            converted = intValue;
            return true;
        }
        if (type == typeof(ShadcnReducedMotionBehavior) && Enum.TryParse<ShadcnReducedMotionBehavior>(value, false, out var behavior) && Enum.IsDefined(behavior))
        {
            converted = behavior;
            return true;
        }
        converted = null;
        return false;
    }

    private static void AddBounded(List<ThemeStudioSnapshot> history, ThemeStudioSnapshot snapshot)
    {
        history.Add(snapshot);
        if (history.Count > HistoryLimit)
            history.RemoveAt(0);
    }

    private static bool PaletteEquals(ShadcnPaletteRecipe first, ShadcnPaletteRecipe second) =>
        first.AlgorithmVersion == second.AlgorithmVersion && first.Seed == second.Seed &&
        string.Equals(first.BaseColor, second.BaseColor, StringComparison.Ordinal) &&
        first.LockedTokens.SequenceEqual(second.LockedTokens, StringComparer.Ordinal) &&
        first.Anchors == second.Anchors && first.Harmony == second.Harmony &&
        SequenceEqual(first.LockedAnchors, second.LockedAnchors);

    private static bool SequenceEqual<T>(IReadOnlyList<T>? first, IReadOnlyList<T>? second) =>
        first is null ? second is null : second is not null && first.SequenceEqual(second);

    private static string PaletteMutationKey(ShadcnPaletteAnchorRole role) =>
        $"palette.{PaletteRoleName(role)}";

    private static string PaletteRoleName(ShadcnPaletteAnchorRole role) => role switch
    {
        ShadcnPaletteAnchorRole.Brand => "brand",
        ShadcnPaletteAnchorRole.Support => "support",
        ShadcnPaletteAnchorRole.Highlight => "highlight",
        ShadcnPaletteAnchorRole.DataA => "dataa",
        ShadcnPaletteAnchorRole.DataB => "datab",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
    };

    private static bool TypographyEquals(ShadcnTypographyScale first, ShadcnTypographyScale second) =>
        first.Body == second.Body && first.ThaiFallback == second.ThaiFallback && first.Code == second.Code &&
        first.Roles.Count == second.Roles.Count && first.Roles.All(item =>
            second.Roles.TryGetValue(item.Key, out var value) && value == item.Value);

    private void EnsureTypographyValid(ShadcnTheme theme, ShadcnTypographyScale typography)
    {
        var candidate = CreateDocument() with { Theme = theme, Typography = typography };
        var validation = ShadcnThemeDocumentValidator.Validate(candidate);
        if (!validation.IsValid)
            throw new ArgumentException("Typography selection is invalid: " +
                string.Join("; ", validation.Errors.Select(error => $"{error.Path}: {error.Message}")));
    }

    private static ThemeStudioSnapshot Pop(List<ThemeStudioSnapshot> history)
    {
        var index = history.Count - 1;
        var snapshot = history[index];
        history.RemoveAt(index);
        return snapshot;
    }

    private static ShadcnTheme Clone(ShadcnTheme theme) => theme with
    {
        Light = theme.Light with { },
        Dark = theme.Dark with { },
        Metrics = theme.Metrics with { }
    };

    private void OnWorkbenchChanged(object? sender, EventArgs args)
    {
        if (!_suppressWorkbenchChanged)
            RaiseChanged();
    }
    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
