using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Components.Icons;

namespace Maliev.ShadcnBlazor.Icons.Hugeicons;

/// <summary>Resolves icons from the pinned complete Hugeicons Free stroke-rounded catalog.</summary>
public sealed class HugeiconsIconCatalog : IShadcnIconCatalog
{
    private static readonly Lazy<HugeiconsIconCatalog> Shared = new(() => new HugeiconsIconCatalog());
    private readonly IReadOnlyDictionary<string, ShadcnIconData> icons;
    private HugeiconsIconCatalog()
    {
        icons = Load();
        Names = Array.AsReadOnly(icons.Keys.Order(StringComparer.Ordinal).ToArray());
    }
    /// <summary>Gets the shared immutable catalog instance.</summary>
    public static HugeiconsIconCatalog Instance => Shared.Value;
    /// <inheritdoc />
    public string Library => "hugeicons";
    /// <inheritdoc />
    public IReadOnlyList<string> Names { get; }
    /// <inheritdoc />
    public bool TryGet(string name, out ShadcnIconData? icon) => icons.TryGetValue(name, out icon);
    /// <inheritdoc />
    public ShadcnIconData Get(string name) => icons.TryGetValue(name, out var icon) ? icon : throw new KeyNotFoundException($"Hugeicons icon '{name}' was not found.");
    private static IReadOnlyDictionary<string, ShadcnIconData> Load()
    {
        using var stream = typeof(HugeiconsIconCatalog).Assembly.GetManifestResourceStream("Maliev.ShadcnBlazor.Icons.Hugeicons.icons.json") ?? throw new InvalidOperationException("Embedded Hugeicons Free catalog was not found.");
        var envelope = JsonSerializer.Deserialize<CatalogEnvelope>(stream, SerializerOptions) ?? throw new InvalidOperationException("Hugeicons Free catalog is empty.");
        if (envelope.Library != "hugeicons") throw new InvalidOperationException("Hugeicons Free catalog library identifier is invalid.");
        var result = envelope.Icons.ToDictionary(icon => icon.Name, icon => new ShadcnIconData(envelope.Library, icon.Name, icon.ViewBox, icon.SvgContent), StringComparer.Ordinal);
        return result;
    }
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    private sealed record CatalogEnvelope(int SchemaVersion, string Library, string Version, string Commit, CatalogIcon[] Icons);
    private sealed record CatalogIcon(string Library, string Name, string ViewBox, string SvgContent);
}
