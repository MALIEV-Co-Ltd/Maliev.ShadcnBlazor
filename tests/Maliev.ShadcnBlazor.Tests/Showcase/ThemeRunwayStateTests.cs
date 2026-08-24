using Maliev.ShadcnBlazor.Showcase.Theming.Runway;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemePreviewAnimationStateTests
{
    [Fact]
    public void RegistryHasOrderedBilingualRealisticBentoCardsIncludingOverlays()
    {
        var cards = new ThemeUseCaseRegistry().All;
        Assert.Equal(19, cards.Count);
        Assert.Equal(Enumerable.Range(1, 19), cards.Select(card => card.Order));
        Assert.Contains(cards, card => card.Size == ThemeBentoSize.Wide);
        Assert.Equal(19, cards.Select(card => card.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.All(cards, card =>
        {
            Assert.NotEqual(card.EnglishTitle, card.ThaiTitle);
            Assert.NotEmpty(card.ComponentTypes);
        });
        Assert.Contains(cards, card => card.ComponentTypes.Contains("ShadcnDialog", StringComparer.Ordinal));
        Assert.Contains(cards, card => card.ComponentTypes.Contains("ShadcnContextMenu", StringComparer.Ordinal));
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
}
