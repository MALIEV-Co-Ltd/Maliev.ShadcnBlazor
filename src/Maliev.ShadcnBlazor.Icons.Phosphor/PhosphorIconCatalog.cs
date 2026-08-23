using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Components.Icons;

namespace Maliev.ShadcnBlazor.Icons.Phosphor;

/// <summary>Resolves icons from the pinned complete Phosphor regular catalog.</summary>
public sealed class PhosphorIconCatalog : IShadcnIconCatalog
{
    private static readonly Lazy<PhosphorIconCatalog> Shared = new(() => new PhosphorIconCatalog());
    private readonly IReadOnlyDictionary<string, ShadcnIconData> icons;
    private PhosphorIconCatalog()
    {
        icons = Load();
        Names = Array.AsReadOnly(icons.Keys.Order(StringComparer.Ordinal).ToArray());
    }
    /// <summary>Gets the shared immutable catalog instance.</summary>
    public static PhosphorIconCatalog Instance => Shared.Value;
    /// <inheritdoc />
    public string Library => "phosphor";
    /// <inheritdoc />
    public IReadOnlyList<string> Names { get; }
    /// <inheritdoc />
    public bool TryGet(string name, out ShadcnIconData? icon) => icons.TryGetValue(name, out icon);
    /// <inheritdoc />
    public ShadcnIconData Get(string name) => icons.TryGetValue(name, out var icon) ? icon : throw new KeyNotFoundException($"Phosphor icon '{name}' was not found.");
    private static IReadOnlyDictionary<string, ShadcnIconData> Load()
    {
        using var stream = typeof(PhosphorIconCatalog).Assembly.GetManifestResourceStream("Maliev.ShadcnBlazor.Icons.Phosphor.icons.json") ?? throw new InvalidOperationException("Embedded Phosphor icon catalog was not found.");
        var envelope = JsonSerializer.Deserialize<CatalogEnvelope>(stream, SerializerOptions) ?? throw new InvalidOperationException("Phosphor icon catalog is empty.");
        if (envelope.Library != "phosphor") throw new InvalidOperationException("Phosphor icon catalog library identifier is invalid.");
        var result = envelope.Icons.ToDictionary(icon => icon.Name, icon => new ShadcnIconData(envelope.Library, icon.Name, icon.ViewBox, icon.SvgContent), StringComparer.Ordinal);
        return result;
    }
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
    private sealed record CatalogEnvelope(int SchemaVersion, string Library, string Version, string Commit, CatalogIcon[] Icons);
    private sealed record CatalogIcon(string Library, string Name, string ViewBox, string SvgContent);
}
