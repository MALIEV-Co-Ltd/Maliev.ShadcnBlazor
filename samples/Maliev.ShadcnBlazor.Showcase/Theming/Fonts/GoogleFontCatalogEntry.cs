using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Fonts;

internal sealed record GoogleFontAxis(string Tag, double Minimum, double Maximum);

internal sealed record GoogleFontCatalogEntry
{
    [JsonConstructor]
    public GoogleFontCatalogEntry(
        string id,
        string family,
        string category,
        IReadOnlyList<string> subsets,
        IReadOnlyList<int> weights,
        IReadOnlyList<GoogleFontAxis> axes,
        string css2FamilyQuery,
        bool isBundled = false)
    {
        Id = id;
        Family = family;
        Category = category;
        Subsets = new ReadOnlyCollection<string>(subsets.ToArray());
        Weights = new ReadOnlyCollection<int>(weights.ToArray());
        Axes = new ReadOnlyCollection<GoogleFontAxis>(axes.ToArray());
        Css2FamilyQuery = css2FamilyQuery;
        IsBundled = isBundled;
    }

    public string Id { get; }
    public string Family { get; }
    public string Category { get; }
    public IReadOnlyList<string> Subsets { get; }
    public IReadOnlyList<int> Weights { get; }
    public IReadOnlyList<GoogleFontAxis> Axes { get; }
    public string Css2FamilyQuery { get; }
    public bool IsBundled { get; }

    public ShadcnFontSelection ToSelection(ThemeStudioFontSlot slot)
    {
        var quotedFamily = $"'{Family.Replace("'", "\\'", StringComparison.Ordinal)}'";
        return slot switch
        {
            ThemeStudioFontSlot.Body when Id == "geist" => new(
                "'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
                "ui-sans-serif, system-ui, sans-serif",
                null),
            ThemeStudioFontSlot.Body => new(
                $"{quotedFamily}, ui-sans-serif, system-ui, sans-serif",
                "ui-sans-serif, system-ui, sans-serif",
                IsBundled ? null : Id),
            ThemeStudioFontSlot.ThaiFallback when Id == "noto-sans-thai" => new(
                "'Noto Sans Thai', sans-serif",
                "sans-serif",
                null),
            ThemeStudioFontSlot.ThaiFallback => new(
                $"{quotedFamily}, 'Noto Sans Thai', sans-serif",
                "'Noto Sans Thai', sans-serif",
                IsBundled ? null : Id),
            ThemeStudioFontSlot.Code when Id == "jetbrains-mono" => new(
                "'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
                "ui-monospace, monospace",
                null),
            ThemeStudioFontSlot.Code => new(
                $"{quotedFamily}, ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
                "ui-monospace, monospace",
                IsBundled ? null : Id),
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown typography font slot.")
        };
    }
}
