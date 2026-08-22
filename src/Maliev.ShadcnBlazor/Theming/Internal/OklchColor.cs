using System.Globalization;

namespace Maliev.ShadcnBlazor.Theming.Internal;

internal readonly record struct OklchColor(double Lightness, double Chroma, double Hue)
{
    internal OklchColor FitToSrgb()
    {
        if (IsInSrgbGamut())
            return this;

        var low = 0d;
        var high = Chroma;
        for (var index = 0; index < 20; index++)
        {
            var candidate = this with { Chroma = (low + high) / 2d };
            if (candidate.IsInSrgbGamut())
                low = candidate.Chroma;
            else
                high = candidate.Chroma;
        }

        return this with { Chroma = Math.Floor(low * 10000d) / 10000d };
    }

    internal string ToCss()
    {
        var fitted = FitToSrgb();
        return FormattableString.Invariant(
            $"oklch({fitted.Lightness:F4} {fitted.Chroma:F4} {NormalizeHue(fitted.Hue):F2})");
    }

    private bool IsInSrgbGamut()
    {
        var radians = NormalizeHue(Hue) * Math.PI / 180d;
        var a = Chroma * Math.Cos(radians);
        var b = Chroma * Math.Sin(radians);
        var l = Lightness + (0.3963377774 * a) + (0.2158037573 * b);
        var m = Lightness - (0.1055613458 * a) - (0.0638541728 * b);
        var s = Lightness - (0.0894841775 * a) - (1.291485548 * b);
        l *= l * l;
        m *= m * m;
        s *= s * s;
        var red = (4.0767416621 * l) - (3.3077115913 * m) + (0.2309699292 * s);
        var green = (-1.2684380046 * l) + (2.6097574011 * m) - (0.3413193965 * s);
        var blue = (-0.0041960863 * l) - (0.7034186147 * m) + (1.707614701 * s);
        const double epsilon = 0.0000001;
        return red is >= -epsilon and <= 1.0000001 &&
               green is >= -epsilon and <= 1.0000001 &&
               blue is >= -epsilon and <= 1.0000001;
    }

    internal static double NormalizeHue(double hue)
    {
        var normalized = hue % 360d;
        return normalized < 0 ? normalized + 360d : normalized;
    }
}
