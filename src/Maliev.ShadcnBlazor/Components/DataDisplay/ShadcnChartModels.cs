using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Components.DataDisplay;

/// <summary>Defines chart geometries rendered by the package engine.</summary>
public enum ShadcnChartType { Bar, Line, Area, Pie, Donut }
/// <summary>Defines chart tooltip indicator presentation.</summary>
public enum ShadcnChartIndicator { Dot, Line, Dashed }
/// <summary>Defines where the chart legend is composed relative to the plot.</summary>
public enum ShadcnChartLegendPlacement { Top, Bottom }

/// <summary>Provides typed content to a custom chart tooltip.</summary>
public sealed record ShadcnChartTooltipContext(string Category, int PointIndex, IReadOnlyList<ShadcnChartSeries> Series);

/// <summary>Defines theme-specific colors for one chart series.</summary>
public sealed record ShadcnChartTheme(string Light, string Dark);

/// <summary>Defines the human-readable and visual configuration of one chart series.</summary>
public sealed record ShadcnChartItemConfig(string Label)
{
    /// <summary>Gets a single color used in every theme.</summary>
    public string? Color { get; init; }
    /// <summary>Gets theme-specific light and dark colors.</summary>
    public ShadcnChartTheme? Theme { get; init; }
    /// <summary>Gets optional custom icon content.</summary>
    public RenderFragment? Icon { get; init; }
}

/// <summary>Maps stable series keys to labels, icons, and scoped colors.</summary>
public sealed class ShadcnChartConfig : Dictionary<string, ShadcnChartItemConfig>
{
    private static readonly Regex SafeKey = new("^[A-Za-z][A-Za-z0-9_-]*$", RegexOptions.CultureInvariant);
    private static readonly Regex SafeColor = new("^(?:#[0-9a-fA-F]{3,8}|(?:rgb|rgba|hsl|hsla|oklch|oklab|lab|lch|color)\\([^;{}]+\\)|var\\(--[A-Za-z0-9_-]+(?:\\s*,[^;{}]+)?\\)|[A-Za-z]+)$", RegexOptions.CultureInvariant);

    /// <summary>Validates keys, labels, and mutually exclusive safe color modes.</summary>
    public void Validate()
    {
        if (Count == 0) throw new ArgumentException("Chart config requires at least one series.");
        foreach (var (key, item) in this)
        {
            if (!SafeKey.IsMatch(key)) throw new ArgumentException($"Chart key '{key}' is not a safe CSS identifier.");
            if (string.IsNullOrWhiteSpace(item.Label)) throw new ArgumentException($"Chart label for '{key}' is required.");
            if (item.Color is not null && item.Theme is not null) throw new ArgumentException($"Chart series '{key}' cannot define both Color and Theme.");
            if (item.Color is not null) ValidateColor(item.Color, key);
            if (item.Theme is not null) { ValidateColor(item.Theme.Light, key); ValidateColor(item.Theme.Dark, key); }
        }
    }

    /// <summary>Creates scoped chart color variables for light and dark themes.</summary>
    public string ToScopedCss(string chartId)
    {
        Validate();
        if (!SafeKey.IsMatch(chartId)) throw new ArgumentException("Chart id is not a safe CSS identifier.", nameof(chartId));
        var light = new StringBuilder($"[data-chart=\"{chartId}\"] {{\n");
        var dark = new StringBuilder($".dark [data-chart=\"{chartId}\"], [data-shadcn-theme=\"dark\"] [data-chart=\"{chartId}\"] {{\n");
        foreach (var (key, item) in this)
        {
            var lightColor = item.Theme?.Light ?? item.Color;
            var darkColor = item.Theme?.Dark ?? item.Color;
            if (lightColor is not null) light.Append("  --color-").Append(key).Append(": ").Append(lightColor).AppendLine(";");
            if (darkColor is not null) dark.Append("  --color-").Append(key).Append(": ").Append(darkColor).AppendLine(";");
        }
        return light.AppendLine("}").Append(dark).AppendLine("}").ToString();
    }

    private static void ValidateColor(string color, string key)
    {
        if (!SafeColor.IsMatch(color.Trim())) throw new ArgumentException($"Chart color for '{key}' is not a safe CSS color.");
    }
}

/// <summary>Contains ordered values for one chart series.</summary>
public sealed record ShadcnChartSeries(string Key, IReadOnlyList<double?> Values)
{
    /// <summary>Gets whether this series is initially visible.</summary>
    public bool Visible { get; init; } = true;
    /// <summary>Gets optional per-series metadata used by tooltip and legend name-key resolution.</summary>
    public IReadOnlyDictionary<string, string> Names { get; init; } = new Dictionary<string, string>();
}

/// <summary>Contains one SVG shape and its finite numeric attributes.</summary>
public sealed record ShadcnChartShape(string Kind, string SeriesKey, int PointIndex, IReadOnlyList<double> Values);

/// <summary>Contains deterministic chart geometry used by SSR and browser rendering.</summary>
public sealed record ShadcnChartGeometry(double Width, double Height, IReadOnlyList<ShadcnChartShape> Shapes, double Minimum, double Maximum)
{
    /// <summary>Builds finite SVG geometry for supported charts.</summary>
    public static ShadcnChartGeometry Create(ShadcnChartType type, IReadOnlyList<string> categories, IReadOnlyList<ShadcnChartSeries> series, double width, double height, bool stacked = false)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        if (!double.IsFinite(width) || width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (!double.IsFinite(height) || height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (categories.Count == 0) throw new ArgumentException("Chart categories are required.", nameof(categories));
        if (series.Count == 0) throw new ArgumentException("Chart series are required.", nameof(series));
        var duplicate = series.GroupBy(item => item.Key, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null) throw new ArgumentException($"Duplicate chart series key '{duplicate.Key}'.", nameof(series));
        foreach (var item in series)
        {
            if (string.IsNullOrWhiteSpace(item.Key)) throw new ArgumentException("Chart series keys are required.", nameof(series));
            if (item.Values.Count != categories.Count) throw new ArgumentException($"Chart series '{item.Key}' has {item.Values.Count} values for {categories.Count} categories.", nameof(series));
            if (item.Values.Any(value => value.HasValue && !double.IsFinite(value.Value))) throw new ArgumentException($"Chart series '{item.Key}' contains a non-finite value.", nameof(series));
        }
        var values = series.SelectMany(item => item.Values).Where(value => value.HasValue).Select(value => value!.Value).ToList();
        var minimum = Math.Min(0, values.DefaultIfEmpty(0).Min());
        var maximum = Math.Max(0, values.DefaultIfEmpty(0).Max());
        if (stacked)
        {
            maximum = Math.Max(maximum, Enumerable.Range(0, categories.Count).Max(index => series.Sum(item => Math.Max(0, item.Values[index] ?? 0))));
            minimum = Math.Min(minimum, Enumerable.Range(0, categories.Count).Min(index => series.Sum(item => Math.Min(0, item.Values[index] ?? 0))));
        }
        if (maximum == minimum) maximum = minimum + 1;
        var shapes = type is ShadcnChartType.Pie or ShadcnChartType.Donut
            ? CreatePie(type, series, width, height)
            : CreateCartesian(type, categories, series, width, height, minimum, maximum, stacked);
        return new(width, height, shapes, minimum, maximum);
    }

    private static List<ShadcnChartShape> CreateCartesian(ShadcnChartType type, IReadOnlyList<string> categories, IReadOnlyList<ShadcnChartSeries> series, double width, double height, double min, double max, bool stacked)
    {
        const double left = 36, top = 12, right = 12, bottom = 28;
        var plotWidth = Math.Max(1, width - left - right);
        var plotHeight = Math.Max(1, height - top - bottom);
        var baseline = top + max / (max - min) * plotHeight;
        var result = new List<ShadcnChartShape>();
        var positive = new double[categories.Count]; var negative = new double[categories.Count];
        for (var seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
        {
            var item = series[seriesIndex];
            var upper = new List<double>();
            var lower = new List<double>();
            void FlushSegment()
            {
                if (upper.Count == 0) return;
                if (type == ShadcnChartType.Line) result.Add(new("polyline", item.Key, -1, upper.ToArray()));
                else if (type == ShadcnChartType.Area)
                {
                    var polygon = upper.Concat(Enumerable.Range(0, lower.Count / 2).Reverse().SelectMany(point => new[] { lower[point * 2], lower[point * 2 + 1] })).ToArray();
                    result.Add(new("area", item.Key, -1, polygon));
                }
                upper.Clear(); lower.Clear();
            }
            for (var index = 0; index < categories.Count; index++)
            {
                var value = item.Values[index];
                if (!value.HasValue) { FlushSegment(); continue; }
                var start = stacked ? (value >= 0 ? positive[index] : negative[index]) : 0;
                var end = start + value.Value;
                if (stacked) { if (value >= 0) positive[index] = end; else negative[index] = end; }
                var xCenter = type == ShadcnChartType.Bar
                    ? left + (index + 0.5) * plotWidth / categories.Count
                    : left + index * plotWidth / Math.Max(1, categories.Count - 1);
                var y = top + (max - end) / (max - min) * plotHeight;
                if (type == ShadcnChartType.Bar)
                {
                    var band = plotWidth / categories.Count;
                    var barWidth = stacked ? band * 0.65 : band * 0.65 / series.Count;
                    var x = stacked ? xCenter - barWidth / 2 : xCenter - band * 0.325 + seriesIndex * barWidth;
                    var y0 = top + (max - start) / (max - min) * plotHeight;
                    result.Add(new("rect", item.Key, index, [x, Math.Min(y, y0), Math.Max(1, barWidth), Math.Abs(y0 - y)]));
                }
                else
                {
                    var y0 = top + (max - start) / (max - min) * plotHeight;
                    upper.AddRange([xCenter, y]);
                    lower.AddRange([xCenter, y0]);
                }
            }
            FlushSegment();
        }
        result.Add(new("baseline", string.Empty, -1, [left, baseline, width - right, baseline]));
        return result;
    }

    private static List<ShadcnChartShape> CreatePie(ShadcnChartType type, IReadOnlyList<ShadcnChartSeries> series, double width, double height)
    {
        var pointCount = series.Count == 0 ? 0 : series.Max(item => item.Values.Count);
        var values = Enumerable.Range(0, pointCount).SelectMany(pointIndex => series.Where(item => pointIndex < item.Values.Count).Select(item => (item, pointIndex, value: Math.Max(0, item.Values[pointIndex] ?? 0)))).Where(entry => entry.value > 0).ToList();
        var total = values.Sum(entry => entry.value);
        var radius = Math.Max(1, Math.Min(width, height) * 0.4); var inner = type == ShadcnChartType.Donut ? radius * 0.58 : 0; var angle = -Math.PI / 2;
        var result = new List<ShadcnChartShape>();
        foreach (var entry in values)
        {
            var next = angle + entry.value / total * Math.PI * 2;
            result.Add(new("arc", entry.item.Key, entry.pointIndex, [width / 2, height / 2, radius, inner, angle, next]));
            angle = next;
        }
        return result;
    }
}
