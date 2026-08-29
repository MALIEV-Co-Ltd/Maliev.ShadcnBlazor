using Maliev.ShadcnBlazor.Components.Icons;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Runway;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemePreviewAnimationStateTests
{
    [Fact]
    public void RegistryHasOrderedBilingualRealisticBentoCardsIncludingOverlays()
    {
        var cards = new ThemeUseCaseRegistry().All;
        Assert.Equal(45, cards.Count);
        Assert.Equal(Enumerable.Range(1, 45), cards.Select(card => card.Order));
        Assert.Contains(cards, card => card.Size == ThemeBentoSize.Wide);
        Assert.Equal(45, cards.Select(card => card.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.All(cards, card =>
        {
            Assert.NotEqual(card.EnglishTitle, card.ThaiTitle);
            Assert.NotEmpty(card.ComponentTypes);
        });
        Assert.Contains(cards, card => card.ComponentTypes.Contains("ShadcnDialog", StringComparer.Ordinal));
        Assert.Contains(cards, card => card.ComponentTypes.Contains("ShadcnContextMenu", StringComparer.Ordinal));
        Assert.True(cards.Count(card => card.ComponentTypes.Contains("ShadcnChart", StringComparer.Ordinal) && !card.Id.EndsWith("-suite", StringComparison.Ordinal)) >= 3);
        Assert.True(cards.Count(card => card.ComponentTypes.Contains("ShadcnSecretInput", StringComparer.Ordinal)) >= 3);
        Assert.True(cards.Count(card => card.ComponentTypes.Contains("ShadcnAspectRatio", StringComparer.Ordinal)) >= 3);
    }

    [Fact]
    public void ClockPausesWithoutJumpAndReducedMotionUsesStableRepresentativeFrame()
    {
        var state = new ThemeRunwayState();
        state.AdvanceForTest();
        var first = state.Frame;
        state.SetInteractionPaused(true);
        state.AdvanceForTest();
        Assert.Equal(first, state.Frame);
        state.SetInteractionPaused(false);
        state.AdvanceForTest();
        Assert.Equal(first.Tick + 1, state.Frame.Tick);
        state.SetReducedMotion(true);
        var reduced = state.Frame;
        state.AdvanceForTest();
        Assert.Equal(reduced, state.Frame);
    }

    [Fact]
    public void IconCatalogCacheCreatesOnlyTheSelectedLibrary()
    {
        var created = new List<ThemeStudioIconLibrary>();
        var factories = Enum.GetValues<ThemeStudioIconLibrary>()
            .Where(library => library != ThemeStudioIconLibrary.Custom)
            .ToDictionary(
                library => library,
                library => (Func<IShadcnIconCatalog>)(() =>
                {
                    created.Add(library);
                    return new StubIconCatalog(library.ToString().ToLowerInvariant());
                }));
        var cache = new ThemeStudioIconCatalogCache(factories);

        var lucide = cache.Get(ThemeStudioIconLibrary.Lucide);
        var lucideAgain = cache.Get(ThemeStudioIconLibrary.Lucide);

        Assert.Same(lucide, lucideAgain);
        Assert.Equal([ThemeStudioIconLibrary.Lucide], created);
        Assert.Equal([ThemeStudioIconLibrary.Lucide], cache.LoadedLibraries);

        _ = cache.Get(ThemeStudioIconLibrary.Phosphor);

        Assert.Equal([ThemeStudioIconLibrary.Lucide, ThemeStudioIconLibrary.Phosphor], created);
        Assert.Equal([ThemeStudioIconLibrary.Lucide, ThemeStudioIconLibrary.Phosphor], cache.LoadedLibraries);
    }

    private sealed class StubIconCatalog(string library) : IShadcnIconCatalog
    {
        public string Library { get; } = library;
        public IReadOnlyList<string> Names { get; } = [];
        public bool TryGet(string name, out ShadcnIconData? icon) { icon = null; return false; }
        public ShadcnIconData Get(string name) => throw new KeyNotFoundException(name);
    }
}
