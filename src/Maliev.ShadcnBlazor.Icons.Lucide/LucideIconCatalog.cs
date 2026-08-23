using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Components.Icons;

namespace Maliev.ShadcnBlazor.Icons.Lucide;

/// <summary>
/// Resolves icons from the pinned complete Lucide free catalog.
/// </summary>
public sealed class LucideIconCatalog : IShadcnIconCatalog
{
    private const string ResourceName = "Maliev.ShadcnBlazor.Icons.Lucide.icons.json";
    private static readonly Lazy<LucideIconCatalog> Shared = new(() => new LucideIconCatalog());
    private readonly IReadOnlyDictionary<string, ShadcnIconData> icons;

    private LucideIconCatalog()
    {
        icons = Load(ResourceName, "lucide");
        Names = Array.AsReadOnly(icons.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    /// <summary>Gets the shared immutable catalog instance.</summary>
    public static LucideIconCatalog Instance => Shared.Value;

    /// <inheritdoc />
    public string Library => "lucide";

    /// <inheritdoc />
    public IReadOnlyList<string> Names { get; }

    /// <inheritdoc />
    public bool TryGet(string name, out ShadcnIconData? icon) => icons.TryGetValue(name, out icon);

    /// <inheritdoc />
    public ShadcnIconData Get(string name) => icons.TryGetValue(name, out var icon)
        ? icon
        : throw new KeyNotFoundException($"Lucide icon '{name}' was not found.");

    private static IReadOnlyDictionary<string, ShadcnIconData> Load(string resourceName, string expectedLibrary)
    {
        using var stream = typeof(LucideIconCatalog).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded icon catalog '{resourceName}' was not found.");
        var envelope = JsonSerializer.Deserialize<CatalogEnvelope>(stream, SerializerOptions)
            ?? throw new InvalidOperationException("Lucide icon catalog is empty.");
        if (!string.Equals(envelope.Library, expectedLibrary, StringComparison.Ordinal))
            throw new InvalidOperationException("Lucide icon catalog library identifier is invalid.");
        var dictionary = envelope.Icons.ToDictionary(
            icon => icon.Name,
            icon => new ShadcnIconData(envelope.Library, icon.Name, icon.ViewBox, icon.SvgContent),
            StringComparer.Ordinal);
        return dictionary;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };

    private sealed record CatalogEnvelope(int SchemaVersion, string Library, string Version, string Commit, CatalogIcon[] Icons);
    private sealed record CatalogIcon(string Library, string Name, string ViewBox, string SvgContent);
}
