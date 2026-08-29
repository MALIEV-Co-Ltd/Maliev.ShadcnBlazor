using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public sealed record ThemeStudioPaletteCopy(
    string CustomizePalette,
    string ActivePalette,
    string ContrastReady,
    string NeedsReview,
    string GeneratePalette,
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
    public string AnchorValue => ReferenceEquals(this, Thai) ? "ค่าสี" : "Color value";
    public string InvalidAnchorValue => ReferenceEquals(this, Thai)
        ? "กรอกค่าสีเป็น #rgb, #rrggbb หรือ oklch(L C H)"
        : "Enter a color as #rgb, #rrggbb, or oklch(L C H).";

    public static ThemeStudioPaletteCopy English { get; } = new(
        "Customize palette",
        "Active palette",
        "Contrast ready",
        "Needs review",
        "Generate palette",
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
}
