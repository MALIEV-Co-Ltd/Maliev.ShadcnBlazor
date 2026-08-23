using Maliev.ShadcnBlazor.Showcase.Theming.Runway;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeRunwayStateTests
{
    [Fact]
    public void RegistryHasTwelveFixedBilingualRealisticCardsSplitEvenly()
    {
        var cards = new ThemeUseCaseRegistry().All;
        Assert.Equal(12, cards.Count);
        Assert.Equal(Enumerable.Range(1, 12), cards.Select(card => card.Order));
        Assert.Equal(6, cards.Count(card => card.Track == ThemeRunwayTrack.Left));
        Assert.Equal(6, cards.Count(card => card.Track == ThemeRunwayTrack.Right));
        Assert.Equal(12, cards.Select(card => card.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.All(cards, card =>
        {
            Assert.NotEqual(card.EnglishTitle, card.ThaiTitle);
            Assert.True(card.ComponentTypes.Count >= 3);
        });
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
