using Maliev.ShadcnBlazor.Components.Icons;
using Maliev.ShadcnBlazor.Icons.Hugeicons;
using Maliev.ShadcnBlazor.Icons.Lucide;
using Maliev.ShadcnBlazor.Icons.Phosphor;
using Maliev.ShadcnBlazor.Icons.Tabler;

namespace Maliev.ShadcnBlazor.Tests.Components.Icons;

public sealed class IconCatalogTests
{
    public static TheoryData<IShadcnIconCatalog, string, int> Catalogs => new()
    {
        { LucideIconCatalog.Instance, "text-align-start", 1700 },
        { TablerIconCatalog.Instance, "text-direction-rtl", 5000 },
        { PhosphorIconCatalog.Instance, "text-align-right", 1200 },
        { HugeiconsIconCatalog.Instance, "right-to-left-list-bullet", 4400 }
    };

    [Theory]
    [MemberData(nameof(Catalogs))]
    public void FullCatalogResolvesSanitizedRepresentativeIcon(IShadcnIconCatalog catalog, string iconName, int minimumCount)
    {
        Assert.True(catalog.Names.Count >= minimumCount, $"{catalog.Library} exposed only {catalog.Names.Count} icons.");
        Assert.Equal(catalog.Names.Order(StringComparer.Ordinal), catalog.Names);
        Assert.Equal(catalog.Names.Count, catalog.Names.Distinct(StringComparer.Ordinal).Count());

        var icon = catalog.Get(iconName);

        Assert.Equal(catalog.Library, icon.Library);
        Assert.Equal(iconName, icon.Name);
        Assert.Contains("currentColor", icon.SvgContent, StringComparison.Ordinal);
        Assert.True(catalog.TryGet(iconName, out var sameIcon));
        Assert.Same(icon, sameIcon);
    }

    [Theory]
    [MemberData(nameof(Catalogs))]
    public void UnknownIconFailsClosed(IShadcnIconCatalog catalog, string knownName, int minimumCount)
    {
        Assert.True(catalog.Names.Count >= minimumCount);
        Assert.Contains(knownName, catalog.Names);
        Assert.False(catalog.TryGet("not-a-real-icon", out var icon));
        Assert.Null(icon);
        Assert.Throws<KeyNotFoundException>(() => catalog.Get("not-a-real-icon"));
    }
}
