using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Components.Icons;

namespace Maliev.ShadcnBlazor.Icons.Tabler;

/// <summary>Resolves icons from the pinned complete Tabler outline catalog.</summary>
public sealed class TablerIconCatalog : IShadcnIconCatalog
{
    private static readonly Lazy<TablerIconCatalog> Shared = new(() => new TablerIconCatalog());
    private readonly IReadOnlyDictionary<string, ShadcnIconData> icons;
    private TablerIconCatalog()
    {
        icons = Load();
        Names = Array.AsReadOnly(icons.Keys.Order(StringComparer.Ordinal).ToArray());
    }
    /// <summary>Gets the shared immutable catalog instance.</summary>
    public static TablerIconCatalog Instance => Shared.Value;
    /// <inheritdoc />
    public string Library => "tabler";
    /// <inheritdoc />
    public IReadOnlyList<string> Names { get; }
    /// <inheritdoc />
    public bool TryGet(string name, out ShadcnIconData? icon) => icons.TryGetValue(name, out icon);
    /// <inheritdoc />
    public ShadcnIconData Get(string name) => icons.TryGetValue(name, out var icon) ? icon : throw new KeyNotFoundException($"Tabler icon '{name}' was not found.");
    private static IReadOnlyDictionary<string, ShadcnIconData> Load()
    {
        using var stream = typeof(TablerIconCatalog).Assembly.GetManifestResourceStream("Maliev.ShadcnBlazor.Icons.Tabler.icons.json") ?? throw new InvalidOperationException("Embedded Tabler icon catalog was not found.");
        var envelope = JsonSerializer.Deserialize<CatalogEnvelope>(stream, SerializerOptions) ?? throw new InvalidOperationException("Tabler icon catalog is empty.");
        if (envelope.Library != "tabler") throw new InvalidOperationException("Tabler icon catalog library identifier is invalid.");
        var result = envelope.Icons.ToDictionary(icon => icon.Name, icon => new ShadcnIconData(envelope.Library, icon.Name, icon.ViewBox, icon.SvgContent), StringComparer.Ordinal);
        return result;
    }
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    private sealed record CatalogEnvelope(int SchemaVersion, string Library, string Version, string Commit, CatalogIcon[] Icons);
    private sealed record CatalogIcon(string Library, string Name, string ViewBox, string SvgContent);
}
