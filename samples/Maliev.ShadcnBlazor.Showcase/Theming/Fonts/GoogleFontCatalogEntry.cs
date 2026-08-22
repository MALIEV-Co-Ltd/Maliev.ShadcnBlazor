using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

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
}
