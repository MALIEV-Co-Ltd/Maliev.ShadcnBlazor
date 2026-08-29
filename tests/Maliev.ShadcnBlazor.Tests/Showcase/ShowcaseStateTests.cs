using Maliev.ShadcnBlazor.Showcase;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ShowcaseStateTests
{
    [Fact]
    public void ToggleTheme_TransitionsBetweenLightAndDarkAndRaisesChanged()
    {
        var state = new ShowcaseState();
        var changedCount = 0;
        state.Changed += (_, _) => changedCount++;

        Assert.False(state.IsDarkMode);

        state.ToggleTheme();

        Assert.True(state.IsDarkMode);
        Assert.Equal(1, changedCount);

        state.ToggleTheme();

        Assert.False(state.IsDarkMode);
        Assert.Equal(2, changedCount);
    }

    [Fact]
    public void ToggleDirection_TransitionsBetweenLtrAndRtlAndRaisesChanged()
    {
        var state = new ShowcaseState();
        var changedCount = 0;
        state.Changed += (_, _) => changedCount++;

        Assert.Equal(ShadcnDirection.LeftToRight, state.Direction);

        state.ToggleDirection();

        Assert.Equal(ShadcnDirection.RightToLeft, state.Direction);
        Assert.Equal(1, changedCount);

        state.ToggleDirection();

        Assert.Equal(ShadcnDirection.LeftToRight, state.Direction);
        Assert.Equal(2, changedCount);
    }

    [Fact]
    public void ExplicitFixtureStateChangesOnlyWhenValueChanges()
    {
        var state = new ShowcaseState();
        var changedCount = 0;
        state.Changed += (_, _) => changedCount++;

        state.SetTheme(true);
        state.SetTheme(true);
        state.SetDirection(ShadcnDirection.RightToLeft);
        state.SetDirection(ShadcnDirection.RightToLeft);

        Assert.True(state.IsDarkMode);
        Assert.Equal(ShadcnDirection.RightToLeft, state.Direction);
        Assert.Equal(2, changedCount);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find repository root.");
    }

    private static string ReadSource(params string[] segments) => File.ReadAllText(Path.Combine([FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", .. segments]));
}
