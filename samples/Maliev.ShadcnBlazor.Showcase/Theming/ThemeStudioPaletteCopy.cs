using System.Text.RegularExpressions;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public sealed record ThemeStudioPaletteCopy(
    string CustomizePalette,
    string ActivePalette,
    string ContrastReady,
    string NeedsReview,
    string GeneratePalette,
    string MainColor,
    string MainColorHelp,
    string DerivedColors,
    string AdvancedColors,
    string GenerateFromMainColor,
    string DerivingPalette,
    string ReturnToPreview,
    string ClosePaletteEditor,
    string Harmony,
    string HarmonyFree,
    string HarmonyAnalogous,
    string HarmonyComplementary,
    string HarmonyTriadic,
    string AnchorBrand,
    string AnchorSupport,
    string AnchorHighlight,
    string AnchorDataA,
    string AnchorDataB,
    string Lock,
    string Unlock,
    string Copy,
    string CopiedStatus,
    string GeneratedStatus,
    string ValidationSummary,
    string ErrorPrefix)
{
    private static readonly Regex ContrastDiagnosticPattern = new(
        @"^Contrast between (?<first>[A-Za-z0-9.]+) and (?<second>[A-Za-z0-9.]+) is (?<measured>[0-9.]+):1; (?<required>[0-9.]+):1 is required\.$",
        RegexOptions.CultureInvariant);
    private static readonly Regex ValidationContrastDiagnosticPattern = new(
        @"^(?<kind>Text|Boundary|DestructiveAdjacency|Chart|FocusRing) contrast against (?<background>[A-Za-z0-9.]+) is (?<measured>[0-9.]+):1; (?<required>[0-9.]+):1 is required\.$",
        RegexOptions.CultureInvariant);

    public string AnchorValue => ReferenceEquals(this, Thai) ? "ค่าสี" : "Color value";
    public string PaletteIdentity(ShadcnThemeDocument document) => document.Palette.IsVersion2
        ? ReferenceEquals(this, Thai) ? $"ซีด {document.Palette.Seed}" : $"Seed {document.Palette.Seed}"
        : document.Name;
    public string InvalidAnchorValue => ReferenceEquals(this, Thai)
        ? "กรอกค่าสีเป็น #rgb, #rrggbb หรือ oklch(L C H)"
        : "Enter a color as #rgb, #rrggbb, or oklch(L C H).";
    public string GeneratedStatusForSeed(ulong seed) => ReferenceEquals(this, Thai)
        ? $"{GeneratedStatus}: ซีด {seed}"
        : $"{GeneratedStatus}: Seed {seed}";

    public static ThemeStudioPaletteCopy English { get; } = new(
        "Customize palette",
        "Active palette",
        "Contrast ready",
        "Needs review",
        "Generate palette",
        "Main color",
        "Choose the color the rest of the palette should follow.",
        "Derived colors",
        "Advanced colors",
        "Generate from main color",
        "Deriving colors…",
        "Return to preview",
        "Close palette editor",
        "Harmony",
        "Free",
        "Analogous",
        "Complementary",
        "Triadic",
        "Brand",
        "Support",
        "Highlight",
        "Data A",
        "Data B",
        "Lock",
        "Unlock",
        "Copy",
        "Copied",
        "Palette generated",
        "Palette validation summary",
        "Palette error");

    public static ThemeStudioPaletteCopy Thai { get; } = new(
        "ปรับแต่งชุดสี",
        "ชุดสีที่ใช้งาน",
        "คอนทราสต์พร้อมใช้งาน",
        "ต้องตรวจสอบ",
        "สร้างชุดสี",
        "สีหลัก",
        "เลือกสีหลักที่ใช้เป็นพื้นฐานของชุดสีที่เหลือ",
        "สีที่สร้างจากสีหลัก",
        "สีขั้นสูง",
        "สร้างจากสีหลัก",
        "กำลังสร้างสี…",
        "กลับไปยังตัวอย่าง",
        "ปิดตัวแก้ไขชุดสี",
        "ความกลมกลืน",
        "อิสระ",
        "สีข้างเคียง",
        "สีคู่ตรงข้าม",
        "สามสี",
        "แบรนด์",
        "สีสนับสนุน",
        "สีเน้น",
        "ข้อมูล A",
        "ข้อมูล B",
        "ล็อก",
        "ปลดล็อก",
        "คัดลอก",
        "คัดลอกแล้ว",
        "สร้างชุดสีแล้ว",
        "สรุปการตรวจสอบชุดสี",
        "ข้อผิดพลาดของชุดสี");

    public static ThemeStudioPaletteCopy For(ThemeStudioLocale locale) => locale switch
    {
        ThemeStudioLocale.English => English,
        ThemeStudioLocale.Thai => Thai,
        _ => throw new ArgumentOutOfRangeException(nameof(locale), locale, "Unknown Theme Studio locale.")
    };

    public string HarmonyName(ShadcnPaletteHarmony harmony) => harmony switch
    {
        ShadcnPaletteHarmony.Free => HarmonyFree,
        ShadcnPaletteHarmony.Analogous => HarmonyAnalogous,
        ShadcnPaletteHarmony.Complementary => HarmonyComplementary,
        ShadcnPaletteHarmony.Triadic => HarmonyTriadic,
        _ => throw new ArgumentOutOfRangeException(nameof(harmony), harmony, "Unknown palette harmony.")
    };

    public string AnchorName(ShadcnPaletteAnchorRole role) => role switch
    {
        ShadcnPaletteAnchorRole.Brand => AnchorBrand,
        ShadcnPaletteAnchorRole.Support => AnchorSupport,
        ShadcnPaletteAnchorRole.Highlight => AnchorHighlight,
        ShadcnPaletteAnchorRole.DataA => AnchorDataA,
        ShadcnPaletteAnchorRole.DataB => AnchorDataB,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette anchor role.")
    };

    public string DiagnosticMessage(ShadcnThemeValidationMessage diagnostic)
    {
        if (diagnostic.Code == "palette-invalid-anchor")
            return ReferenceEquals(this, Thai) ? InvalidAnchorValue : diagnostic.Message;
        if (diagnostic.Code is "palette-locked-constraint" or "palette-constraint-unsatisfied")
        {
            if (!ReferenceEquals(this, Thai))
                return diagnostic.Message;

            var contrast = ContrastDiagnosticPattern.Match(diagnostic.Message);
            return contrast.Success
                ? $"คอนทราสต์ระหว่าง {contrast.Groups["first"].Value} และ {contrast.Groups["second"].Value} เท่ากับ {contrast.Groups["measured"].Value}:1 โดยต้องมีอย่างน้อย {contrast.Groups["required"].Value}:1"
                : $"{ErrorPrefix}: {diagnostic.Path}";
        }
        if (diagnostic.Code is not ("low-contrast" or "low-boundary-contrast" or
            "low-destructive-adjacency-contrast" or "low-chart-contrast" or "low-focus-ring-contrast"))
            return $"{ErrorPrefix}: {diagnostic.Path}";
        if (!ReferenceEquals(this, Thai))
            return diagnostic.Message;

        var validationContrast = ValidationContrastDiagnosticPattern.Match(diagnostic.Message);
        return validationContrast.Success
            ? $"คอนทราสต์ของ {diagnostic.Path} เทียบกับ {validationContrast.Groups["background"].Value} เท่ากับ {validationContrast.Groups["measured"].Value}:1 โดยต้องมีอย่างน้อย {validationContrast.Groups["required"].Value}:1"
            : $"{ErrorPrefix}: {diagnostic.Path}";
    }
}
