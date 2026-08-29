namespace Maliev.ShadcnBlazor.Theming.Internal;

internal static class ShadcnPaletteSemanticMapper
{
    internal static ShadcnTheme Map(
        ShadcnTheme source,
        ShadcnPaletteAnchors anchors,
        double neutralHue,
        double neutralChroma)
    {
        _ = ShadcnPaletteColorParser.TryNormalize(anchors.Brand, out var brand, out var brandCss);
        _ = ShadcnPaletteColorParser.TryNormalize(anchors.Support, out var support, out var supportCss);
        _ = ShadcnPaletteColorParser.TryNormalize(anchors.Highlight, out var highlight, out var highlightCss);
        _ = ShadcnPaletteColorParser.TryNormalize(anchors.DataA, out _, out var dataACss);
        _ = ShadcnPaletteColorParser.TryNormalize(anchors.DataB, out _, out var dataBCss);
        return source with
        {
            Light = CreateScheme(source.Light, false, brand, support, highlight, brandCss, supportCss, highlightCss, dataACss, dataBCss,
                neutralHue, neutralChroma),
            Dark = CreateScheme(source.Dark, true, brand, support, highlight, brandCss, supportCss, highlightCss, dataACss, dataBCss,
                neutralHue, neutralChroma),
            Metrics = source.Metrics with { }
        };
    }

    private static ShadcnColorScheme CreateScheme(
        ShadcnColorScheme source,
        bool dark,
        OklchColor brand,
        OklchColor support,
        OklchColor highlight,
        string brandCss,
        string supportCss,
        string highlightCss,
        string dataACss,
        string dataBCss,
        double neutralHue,
        double neutralChroma)
    {
        string Neutral(double lightness, double chromaScale) =>
            new OklchColor(lightness, Math.Min(0.025d, neutralChroma * chromaScale), neutralHue).ToCss();
        string Brand(double lightness, double chroma) =>
            new OklchColor(lightness, chroma, brand.Hue).ToCss();
        string Surface(OklchColor anchor) => new OklchColor(
            dark ? 0.72d : 0.45d,
            Math.Clamp(anchor.Chroma, 0.08d, 0.18d),
            anchor.Hue).ToCss();

        var low = dark ? 0.145d : 0.985d;
        var high = dark ? 0.985d : 0.145d;
        var actionForeground = dark ? 0.145d : 0.985d;
        return new()
        {
            Background = Neutral(low, 0.035d),
            Foreground = Neutral(high, 0.045d),
            Card = Neutral(dark ? 0.19d : 0.995d, 0.035d),
            CardForeground = Neutral(high, 0.045d),
            Popover = Neutral(dark ? 0.19d : 0.995d, 0.035d),
            PopoverForeground = Neutral(high, 0.045d),
            Primary = brandCss,
            PrimaryForeground = Neutral(actionForeground, 0.02d),
            Secondary = Surface(support),
            SecondaryForeground = Neutral(actionForeground, 0.02d),
            Muted = Neutral(dark ? 0.25d : 0.94d, 0.10d),
            MutedForeground = Neutral(dark ? 0.75d : 0.35d, 0.06d),
            Accent = Surface(highlight),
            AccentForeground = Neutral(actionForeground, 0.02d),
            Destructive = new OklchColor(dark ? 0.68d : 0.48d, 0.19d, 25d).ToCss(),
            DestructiveForeground = Neutral(actionForeground, 0.02d),
            Border = Neutral(dark ? 0.65d : 0.55d, 0.10d),
            Input = Neutral(dark ? 0.65d : 0.55d, 0.10d),
            Ring = Brand(dark ? 0.90d : 0.18d, Math.Min(0.05d, brand.Chroma)),
            Chart1 = brandCss,
            Chart2 = supportCss,
            Chart3 = highlightCss,
            Chart4 = dataACss,
            Chart5 = dataBCss,
            Sidebar = Neutral(dark ? 0.175d : 0.97d, 0.035d),
            SidebarForeground = Neutral(high, 0.045d),
            SidebarPrimary = brandCss,
            SidebarPrimaryForeground = Neutral(actionForeground, 0.02d),
            SidebarAccent = Surface(highlight),
            SidebarAccentForeground = Neutral(actionForeground, 0.02d),
            SidebarBorder = Neutral(dark ? 0.65d : 0.55d, 0.10d),
            SidebarRing = Brand(dark ? 0.90d : 0.18d, Math.Min(0.05d, brand.Chroma)),
            ShadowExtraSmall = source.ShadowExtraSmall,
            ShadowSmall = source.ShadowSmall,
            ShadowMedium = source.ShadowMedium
        };
    }
}
