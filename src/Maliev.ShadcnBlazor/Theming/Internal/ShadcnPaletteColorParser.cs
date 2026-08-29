using System.Globalization;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.Theming.Internal;

internal static partial class ShadcnPaletteColorParser
{
    internal static bool TryNormalize(string? value, out OklchColor color, out string normalized)
    {
        color = default;
        normalized = string.Empty;
        if (string.IsNullOrEmpty(value))
            return false;

        var hex = HexPattern().Match(value);
        if (hex.Success)
        {
            var digits = hex.Groups["value"].Value;
            if (digits.Length == 3)
                digits = string.Concat(digits.Select(character => new string(character, 2)));

            var red = byte.Parse(digits.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
            var green = byte.Parse(digits.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
            var blue = byte.Parse(digits.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
            color = FromSrgb(red, green, blue).FitToSrgb();
            normalized = color.ToCss();
            return true;
        }

        var oklch = OklchPattern().Match(value);
        if (!oklch.Success ||
            !TryNumber(oklch.Groups["lightness"].Value, out var lightness) ||
            !TryNumber(oklch.Groups["chroma"].Value, out var chroma) ||
            !TryNumber(oklch.Groups["hue"].Value, out var hue))
        {
            return false;
        }

        color = new OklchColor(
            Math.Clamp(lightness, 0d, 1d),
            chroma,
            OklchColor.NormalizeHue(hue)).FitToSrgb();
        normalized = color.ToCss();
        return true;
    }

    private static OklchColor FromSrgb(double red, double green, double blue)
    {
        red = ToLinear(red);
        green = ToLinear(green);
        blue = ToLinear(blue);

        var l = Math.Cbrt((0.4122214708 * red) + (0.5363325363 * green) + (0.0514459929 * blue));
        var m = Math.Cbrt((0.2119034982 * red) + (0.6806995451 * green) + (0.1073969566 * blue));
        var s = Math.Cbrt((0.0883024619 * red) + (0.2817188376 * green) + (0.6299787005 * blue));
        var lightness = (0.2104542553 * l) + (0.793617785 * m) - (0.0040720468 * s);
        var a = (1.9779984951 * l) - (2.428592205 * m) + (0.4505937099 * s);
        var b = (0.0259040371 * l) + (0.7827717662 * m) - (0.808675766 * s);
        var chroma = Math.Sqrt((a * a) + (b * b));
        var hue = chroma < 0.0000001
            ? 0d
            : OklchColor.NormalizeHue(Math.Atan2(b, a) * 180d / Math.PI);
        return new(lightness, chroma, hue);
    }

    private static double ToLinear(double value) => value <= 0.04045
        ? value / 12.92
        : Math.Pow((value + 0.055) / 1.055, 2.4);

    private static bool TryNumber(string value, out double result) =>
        double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result) &&
        double.IsFinite(result);

    [GeneratedRegex("^#(?<value>[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.CultureInvariant)]
    private static partial Regex HexPattern();

    [GeneratedRegex("^oklch\\(\\s*(?<lightness>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<chroma>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<hue>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s*\\)$", RegexOptions.CultureInvariant)]
    private static partial Regex OklchPattern();
}
