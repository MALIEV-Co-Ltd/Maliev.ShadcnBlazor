using System.Globalization;
using System.Xml;

namespace Maliev.ShadcnBlazor.Components.Icons;

/// <summary>
/// Describes one sanitized, dependency-free SVG icon.
/// </summary>
public sealed record ShadcnIconData
{
    private static readonly HashSet<string> AllowedElements = new(StringComparer.Ordinal)
    {
        "circle", "ellipse", "g", "line", "path", "polygon", "polyline", "rect"
    };

    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.Ordinal)
    {
        "clip-rule", "cx", "cy", "d", "fill", "fill-rule", "height", "opacity", "points", "r", "rx", "ry",
        "stroke", "stroke-dasharray", "stroke-dashoffset", "stroke-linecap", "stroke-linejoin", "stroke-width",
        "transform", "width", "x", "x1", "x2", "y", "y1", "y2"
    };

    /// <summary>
    /// Initializes a sanitized icon definition.
    /// </summary>
    /// <param name="library">Stable icon-library identifier.</param>
    /// <param name="name">Stable icon name within the library.</param>
    /// <param name="viewBox">Four-number SVG view box.</param>
    /// <param name="svgContent">Sanitized inner SVG markup.</param>
    public ShadcnIconData(string library, string name, string viewBox, string svgContent)
    {
        Library = RequiredIdentifier(library, nameof(library));
        Name = RequiredIdentifier(name, nameof(name));
        ViewBox = ValidViewBox(viewBox);
        SvgContent = ValidSvgContent(svgContent);
    }

    /// <summary>
    /// Gets the stable icon-library identifier.
    /// </summary>
    public string Library { get; }

    /// <summary>
    /// Gets the stable icon name within the library.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the validated SVG view box.
    /// </summary>
    public string ViewBox { get; }

    /// <summary>
    /// Gets the sanitized inner SVG markup.
    /// </summary>
    public string SvgContent { get; }

    private static string RequiredIdentifier(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    private static string ValidViewBox(string viewBox)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(viewBox);
        var parts = viewBox.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 ||
            !parts.Select(part => double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && double.IsFinite(number)).All(valid => valid) ||
            !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width) || width <= 0 ||
            !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height) || height <= 0)
        {
            throw new ArgumentException("Icon viewBox must contain four finite numbers with positive width and height.", nameof(viewBox));
        }

        return string.Join(' ', parts);
    }

    private static string ValidSvgContent(string svgContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(svgContent);
        var settings = new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Document,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };

        try
        {
            using var textReader = new StringReader($"<root>{svgContent}</root>");
            using var reader = XmlReader.Create(textReader, settings);
            while (reader.Read())
            {
                if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA && !string.IsNullOrWhiteSpace(reader.Value))
                    throw new ArgumentException("SVG content may contain geometry only.", nameof(svgContent));

                if (reader.NodeType != XmlNodeType.Element || reader.Name == "root")
                    continue;

                if (!AllowedElements.Contains(reader.Name))
                    throw new ArgumentException($"SVG element '{reader.Name}' is not allowed.", nameof(svgContent));

                if (!reader.HasAttributes)
                    continue;

                while (reader.MoveToNextAttribute())
                {
                    if (!AllowedAttributes.Contains(reader.Name) ||
                        reader.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                        reader.Value.Contains("url(", StringComparison.OrdinalIgnoreCase) ||
                        reader.Value.Contains("javascript:", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"SVG attribute '{reader.Name}' is not allowed.", nameof(svgContent));
                    }
                }

                reader.MoveToElement();
            }
        }
        catch (XmlException exception)
        {
            throw new ArgumentException("SVG content must be well-formed XML.", nameof(svgContent), exception);
        }

        return svgContent.Trim();
    }
}

/// <summary>
/// Resolves named icons from one immutable catalog.
/// </summary>
public interface IShadcnIconCatalog
{
    /// <summary>
    /// Gets the stable library identifier.
    /// </summary>
    string Library { get; }

    /// <summary>
    /// Gets the available icon names in ordinal order.
    /// </summary>
    IReadOnlyList<string> Names { get; }

    /// <summary>
    /// Tries to resolve an icon by name.
    /// </summary>
    /// <param name="name">Icon name.</param>
    /// <param name="icon">Resolved icon when found.</param>
    /// <returns><see langword="true"/> when the icon exists.</returns>
    bool TryGet(string name, out ShadcnIconData? icon);

    /// <summary>
    /// Resolves an icon by name.
    /// </summary>
    /// <param name="name">Icon name.</param>
    /// <returns>The resolved icon.</returns>
    ShadcnIconData Get(string name);
}
