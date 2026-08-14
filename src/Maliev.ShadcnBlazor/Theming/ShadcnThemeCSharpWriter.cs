using System.Globalization;
using System.Text;

namespace Maliev.ShadcnBlazor.Theming;

public static class ShadcnThemeCSharpWriter
{
    public static string Write(ShadcnTheme theme)
    {
        ShadcnThemeCssWriter.EnsureValid(theme);
        var builder = new StringBuilder();
        builder.Append("new ShadcnTheme\n{\n")
            .Append("    SchemaVersion = ShadcnTheme.CurrentSchemaVersion,\n")
            .Append("    Name = \"").Append(Escape(theme.Name)).Append("\",\n")
            .Append("    Light = ");
        AppendScheme(builder, theme.Light, 4);
        builder.Append(",\n    Dark = ");
        AppendScheme(builder, theme.Dark, 4);
        builder.Append(",\n    Metrics = new ShadcnThemeMetrics\n    {\n")
            .Append("        FontFamily = \"").Append(Escape(theme.Metrics.FontFamily)).Append("\",\n")
            .Append("        MonospaceFontFamily = \"").Append(Escape(theme.Metrics.MonospaceFontFamily)).Append("\",\n")
            .Append("        RadiusRem = ").Append(Format(theme.Metrics.RadiusRem)).Append(",\n")
            .Append("        RadiusSmallScale = ").Append(Format(theme.Metrics.RadiusSmallScale)).Append(",\n")
            .Append("        RadiusMediumScale = ").Append(Format(theme.Metrics.RadiusMediumScale)).Append(",\n")
            .Append("        RadiusLargeScale = ").Append(Format(theme.Metrics.RadiusLargeScale)).Append(",\n")
            .Append("        RadiusExtraLargeScale = ").Append(Format(theme.Metrics.RadiusExtraLargeScale)).Append(",\n")
            .Append("        Radius2ExtraLargeScale = ").Append(Format(theme.Metrics.Radius2ExtraLargeScale)).Append(",\n")
            .Append("        Radius3ExtraLargeScale = ").Append(Format(theme.Metrics.Radius3ExtraLargeScale)).Append(",\n")
            .Append("        Radius4ExtraLargeScale = ").Append(Format(theme.Metrics.Radius4ExtraLargeScale)).Append(",\n")
            .Append("        ControlHeightRem = ").Append(Format(theme.Metrics.ControlHeightRem)).Append(",\n")
            .Append("        ControlHeightSmallRem = ").Append(Format(theme.Metrics.ControlHeightSmallRem)).Append(",\n")
            .Append("        ControlHeightLargeRem = ").Append(Format(theme.Metrics.ControlHeightLargeRem)).Append(",\n")
            .Append("        SpacingScaleMultiplier = ").Append(Format(theme.Metrics.SpacingScaleMultiplier)).Append(",\n")
            .Append("        FocusRingWidthPx = ").Append(Format(theme.Metrics.FocusRingWidthPx)).Append(",\n")
            .Append("        FocusRingOffsetPx = ").Append(Format(theme.Metrics.FocusRingOffsetPx)).Append(",\n")
            .Append("        MotionDurationMilliseconds = ").Append(theme.Metrics.MotionDurationMilliseconds.ToString(CultureInfo.InvariantCulture)).Append(",\n")
            .Append("        MotionEasing = \"").Append(Escape(theme.Metrics.MotionEasing)).Append("\",\n")
            .Append("        ReducedMotionBehavior = ShadcnReducedMotionBehavior.").Append(theme.Metrics.ReducedMotionBehavior).Append("\n")
            .Append("    }\n};\n");
        return builder.ToString();
    }

    private static void AppendScheme(StringBuilder builder, ShadcnColorScheme scheme, int indent)
    {
        var outer = new string(' ', indent);
        var inner = new string(' ', indent + 4);
        builder.Append("new ShadcnColorScheme\n").Append(outer).Append("{\n");
        var properties = new (string Name, string Value)[]
        {
            (nameof(scheme.Background), scheme.Background),
            (nameof(scheme.Foreground), scheme.Foreground),
            (nameof(scheme.Card), scheme.Card),
            (nameof(scheme.CardForeground), scheme.CardForeground),
            (nameof(scheme.Popover), scheme.Popover),
            (nameof(scheme.PopoverForeground), scheme.PopoverForeground),
            (nameof(scheme.Primary), scheme.Primary),
            (nameof(scheme.PrimaryForeground), scheme.PrimaryForeground),
            (nameof(scheme.Secondary), scheme.Secondary),
            (nameof(scheme.SecondaryForeground), scheme.SecondaryForeground),
            (nameof(scheme.Muted), scheme.Muted),
            (nameof(scheme.MutedForeground), scheme.MutedForeground),
            (nameof(scheme.Accent), scheme.Accent),
            (nameof(scheme.AccentForeground), scheme.AccentForeground),
            (nameof(scheme.Destructive), scheme.Destructive),
            (nameof(scheme.DestructiveForeground), scheme.DestructiveForeground),
            (nameof(scheme.Border), scheme.Border),
            (nameof(scheme.Input), scheme.Input),
            (nameof(scheme.Ring), scheme.Ring),
            (nameof(scheme.Chart1), scheme.Chart1),
            (nameof(scheme.Chart2), scheme.Chart2),
            (nameof(scheme.Chart3), scheme.Chart3),
            (nameof(scheme.Chart4), scheme.Chart4),
            (nameof(scheme.Chart5), scheme.Chart5),
            (nameof(scheme.Sidebar), scheme.Sidebar),
            (nameof(scheme.SidebarForeground), scheme.SidebarForeground),
            (nameof(scheme.SidebarPrimary), scheme.SidebarPrimary),
            (nameof(scheme.SidebarPrimaryForeground), scheme.SidebarPrimaryForeground),
            (nameof(scheme.SidebarAccent), scheme.SidebarAccent),
            (nameof(scheme.SidebarAccentForeground), scheme.SidebarAccentForeground),
            (nameof(scheme.SidebarBorder), scheme.SidebarBorder),
            (nameof(scheme.SidebarRing), scheme.SidebarRing),
            (nameof(scheme.ShadowExtraSmall), scheme.ShadowExtraSmall),
            (nameof(scheme.ShadowSmall), scheme.ShadowSmall),
            (nameof(scheme.ShadowMedium), scheme.ShadowMedium)
        };

        foreach (var (name, value) in properties)
            builder.Append(inner).Append(name).Append(" = \"").Append(Escape(value)).Append("\",\n");
        builder.Length -= 2;
        builder.Append('\n').Append(outer).Append('}');
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(character) || char.IsSurrogate(character) ||
                        character is '\u2028' or '\u2029' or '\ufeff')
                        builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    else
                        builder.Append(character);
                    break;
            }
        }

        return builder.ToString();
    }

    private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
