using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.Build;

internal static class ThemeDocumentBuildValidator
{
    private const int MaxDocumentBytes = 1_048_576;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly string[] RequiredRoot = ["name", "theme", "application", "palette", "typography"];
    private static readonly string[] RequiredScheme =
    [
        "background", "foreground", "card", "cardForeground", "popover", "popoverForeground",
        "primary", "primaryForeground", "secondary", "secondaryForeground", "muted", "mutedForeground",
        "accent", "accentForeground", "destructive", "destructiveForeground", "border", "input", "ring",
        "chart1", "chart2", "chart3", "chart4", "chart5", "sidebar", "sidebarForeground", "sidebarPrimary",
        "sidebarPrimaryForeground", "sidebarAccent", "sidebarAccentForeground", "sidebarBorder", "sidebarRing"
    ];
    private static readonly Regex HexPattern = new("^#(?:[0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.CultureInvariant);
    private static readonly Regex OklchPattern = new(
        "^oklch\\(\\s*(?<l>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<c>(?:\\d+(?:\\.\\d+)?|\\.\\d+))\\s+(?<h>(?:\\d+(?:\\.\\d+)?|\\.\\d+))(?:\\s*/\\s*(?<a>(?:\\d+(?:\\.\\d+)?|\\.\\d+)%?))?\\s*\\)$",
        RegexOptions.CultureInvariant);

    internal static IReadOnlyList<ThemeBuildDiagnostic> Validate(string file)
    {
        byte[] bytes;
        try
        {
            using var stream = File.OpenRead(file);
            if (stream.Length > MaxDocumentBytes)
                return [Failure("MSHCN001", "$", $"Theme document exceeds the {MaxDocumentBytes} byte limit.", 1, 1)];
            bytes = new byte[stream.Length];
            stream.ReadExactly(bytes);
            _ = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return [Failure("MSHCN001", "$", "Theme document must contain strict UTF-8 text.", 1, 1)];
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return [Failure("MSHCN001", "$", $"Theme document could not be read: {exception.Message}", 1, 1)];
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(bytes, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Disallow, MaxDepth = 32 });
        }
        catch (JsonException exception)
        {
            return [Failure("MSHCN001", "$", "Theme document is not valid canonical JSON.",
                checked((int)(exception.LineNumber ?? 0) + 1), checked((int)(exception.BytePositionInLine ?? 0) + 1))];
        }

        using (document)
        {
            var source = new SourceMap(bytes);
            var diagnostics = new List<ThemeBuildDiagnostic>();
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(Failure("MSHCN001", "$", "Theme document root must be an object.", 1, 1));
                return diagnostics;
            }

            var root = document.RootElement;
            if (!root.TryGetProperty("schemaVersion", out var schema) || !schema.TryGetInt32(out var schemaVersion))
            {
                Add(diagnostics, source, "MSHCN002", "schemaVersion", "A numeric schemaVersion is required.");
                return diagnostics;
            }
            if (schemaVersion is 0 or 1)
            {
                Add(diagnostics, source, "MSHCN103", "schemaVersion", "Legacy theme schema requires migration before canonical validation.", true);
                return diagnostics;
            }
            if (schemaVersion != 2)
            {
                Add(diagnostics, source, "MSHCN002", "schemaVersion", "Unsupported theme document schema version.");
                return diagnostics;
            }

            foreach (var property in RequiredRoot)
                Require(root, property, property, diagnostics, source);

            if (root.TryGetProperty("theme", out var theme) && theme.ValueKind == JsonValueKind.Object)
            {
                Require(theme, "light", "theme.light", diagnostics, source);
                Require(theme, "dark", "theme.dark", diagnostics, source);
                Require(theme, "metrics", "theme.metrics", diagnostics, source);
                ValidateScheme(theme, "light", diagnostics, source);
                ValidateScheme(theme, "dark", diagnostics, source);
            }

            ValidatePalette(root, diagnostics, source);
            ValidateTypography(root, diagnostics, source);
            return diagnostics;
        }
    }

    private static void ValidateScheme(JsonElement theme, string name, ICollection<ThemeBuildDiagnostic> diagnostics, SourceMap source)
    {
        if (!theme.TryGetProperty(name, out var scheme) || scheme.ValueKind != JsonValueKind.Object)
            return;
        foreach (var token in RequiredScheme)
        {
            var path = $"theme.{name}.{token}";
            if (!Require(scheme, token, path, diagnostics, source))
                continue;
            var value = scheme.GetProperty(token);
            if (value.ValueKind != JsonValueKind.String || !TryParseColor(value.GetString(), out _))
                Add(diagnostics, source, "MSHCN004", path, "Color must use safe hexadecimal or canonical oklch() syntax.");
        }

        if (TryColor(scheme, "foreground", out var foreground) && TryColor(scheme, "background", out var background) &&
            Contrast(foreground, background) < 4.5)
        {
            Add(diagnostics, source, "MSHCN101", $"theme.{name}.foreground", "Text contrast against background is below WCAG AA 4.5:1.", true);
        }
    }

    private static void ValidatePalette(JsonElement root, ICollection<ThemeBuildDiagnostic> diagnostics, SourceMap source)
    {
        if (!root.TryGetProperty("palette", out var palette) || palette.ValueKind != JsonValueKind.Object)
            return;
        if (palette.TryGetProperty("algorithmVersion", out var algorithm) &&
            (!algorithm.TryGetInt32(out var version) || version is not 0 and not 1))
            Add(diagnostics, source, "MSHCN004", "palette.algorithmVersion", "Palette algorithm version must be 0 or 1.");
        if (palette.TryGetProperty("lockedTokens", out var locks) && locks.ValueKind != JsonValueKind.Array)
            Add(diagnostics, source, "MSHCN004", "palette.lockedTokens", "Locked tokens must be an array of semantic paths.");
    }

    private static void ValidateTypography(JsonElement root, ICollection<ThemeBuildDiagnostic> diagnostics, SourceMap source)
    {
        if (!root.TryGetProperty("typography", out var typography) || typography.ValueKind != JsonValueKind.Object)
            return;
        foreach (var fontName in new[] { "body", "thaiFallback", "code" })
        {
            if (!typography.TryGetProperty(fontName, out var font) || font.ValueKind != JsonValueKind.Object)
                continue;
            if (font.TryGetProperty("googleFontsId", out var id) && id.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(id.GetString()))
                Add(diagnostics, source, "MSHCN102", $"typography.{fontName}.googleFontsId", "Remote font availability cannot be guaranteed; configure a local fallback.", true);
        }
    }

    private static bool Require(JsonElement parent, string property, string path,
        ICollection<ThemeBuildDiagnostic> diagnostics, SourceMap source)
    {
        if (parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null)
            return true;
        Add(diagnostics, source, "MSHCN003", path, "Required theme value is missing.");
        return false;
    }

    private static void Add(ICollection<ThemeBuildDiagnostic> diagnostics, SourceMap source,
        string code, string path, string message, bool warning = false)
    {
        var (line, column) = source.Find(path);
        diagnostics.Add(new(code, path, message, line, column, warning));
    }

    private static ThemeBuildDiagnostic Failure(string code, string path, string message, int line, int column) =>
        new(code, path, message, line, column, false);

    private static bool TryColor(JsonElement scheme, string name, out Rgba color)
    {
        color = default;
        return scheme.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String &&
               TryParseColor(value.GetString(), out color);
    }

    private static bool TryParseColor(string? value, out Rgba color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.IndexOfAny([';', '{', '}', '<', '>']) >= 0 ||
            value.Contains("url(", StringComparison.OrdinalIgnoreCase))
            return false;
        if (HexPattern.IsMatch(value))
        {
            var hex = value[1..];
            if (hex.Length is 3 or 4)
                hex = string.Concat(hex.Select(character => new string(character, 2)));
            color = new(ToLinear(byte.Parse(hex.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d),
                ToLinear(byte.Parse(hex.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d),
                ToLinear(byte.Parse(hex.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d));
            return true;
        }

        var match = OklchPattern.Match(value);
        if (!match.Success || !TryNumber(match.Groups["l"].Value, out var lightness) ||
            !TryNumber(match.Groups["c"].Value, out var chroma) || !TryNumber(match.Groups["h"].Value, out var hue) ||
            lightness is < 0 or > 1 || chroma is < 0 or > 0.4 || hue is < 0 or > 360)
            return false;
        var alphaText = match.Groups["a"].Value;
        if (alphaText.Length > 0)
        {
            var percent = alphaText.EndsWith('%');
            if (!TryNumber(percent ? alphaText[..^1] : alphaText, out var alpha) ||
                alpha < 0 || alpha > (percent ? 100 : 1))
                return false;
        }

        var radians = hue * Math.PI / 180d;
        var a = chroma * Math.Cos(radians);
        var b = chroma * Math.Sin(radians);
        var l = lightness + (0.3963377774 * a) + (0.2158037573 * b);
        var m = lightness - (0.1055613458 * a) - (0.0638541728 * b);
        var s = lightness - (0.0894841775 * a) - (1.291485548 * b);
        l *= l * l;
        m *= m * m;
        s *= s * s;
        color = new(Math.Clamp((4.0767416621 * l) - (3.3077115913 * m) + (0.2309699292 * s), 0, 1),
            Math.Clamp((-1.2684380046 * l) + (2.6097574011 * m) - (0.3413193965 * s), 0, 1),
            Math.Clamp((-0.0041960863 * l) - (0.7034186147 * m) + (1.707614701 * s), 0, 1));
        return true;
    }

    private static bool TryNumber(string value, out double result) =>
        double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result) && double.IsFinite(result);

    private static double Contrast(Rgba first, Rgba second)
    {
        var firstLuminance = (0.2126 * first.Red) + (0.7152 * first.Green) + (0.0722 * first.Blue);
        var secondLuminance = (0.2126 * second.Red) + (0.7152 * second.Green) + (0.0722 * second.Blue);
        return (Math.Max(firstLuminance, secondLuminance) + 0.05) / (Math.Min(firstLuminance, secondLuminance) + 0.05);
    }

    private static double ToLinear(double srgb) => srgb <= 0.04045 ? srgb / 12.92 : Math.Pow((srgb + 0.055) / 1.055, 2.4);

    private readonly record struct Rgba(double Red, double Green, double Blue);

    private sealed class SourceMap
    {
        private readonly byte[] _bytes;
        private readonly Dictionary<string, long> _offsets = new(StringComparer.Ordinal);

        internal SourceMap(byte[] bytes)
        {
            _bytes = bytes;
            var reader = new Utf8JsonReader(bytes);
            var objects = new Stack<string>();
            var currentPath = string.Empty;
            string? pending = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    pending = reader.GetString();
                    currentPath = string.IsNullOrEmpty(objects.TryPeek(out var parent) ? parent : null)
                        ? pending!
                        : $"{parent}.{pending}";
                    _offsets[currentPath] = reader.TokenStartIndex;
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    objects.Push(currentPath);
                    pending = null;
                }
                else if (reader.TokenType == JsonTokenType.EndObject && objects.Count > 0)
                {
                    objects.Pop();
                    pending = null;
                }
                else
                {
                    pending = null;
                }
            }
        }

        internal (int Line, int Column) Find(string path)
        {
            var candidate = path;
            long offset;
            while (!_offsets.TryGetValue(candidate, out offset))
            {
                var separator = candidate.LastIndexOf('.');
                if (separator < 0)
                    return (1, 1);
                candidate = candidate[..separator];
            }

            var line = 1;
            var column = 1;
            for (var index = 0; index < offset; index++)
            {
                if (_bytes[index] == (byte)'\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
            return (line, column);
        }
    }
}
