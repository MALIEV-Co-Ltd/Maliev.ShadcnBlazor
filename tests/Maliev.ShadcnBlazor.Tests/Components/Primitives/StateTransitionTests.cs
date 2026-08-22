using Bunit;
using Maliev.ShadcnBlazor.Components.Primitives;

namespace Maliev.ShadcnBlazor.Tests.Components.Primitives;

public sealed class StateTransitionTests : BunitContext
{
    [Fact]
    public void KeepsBothStatesMountedAndExposesOnlyTheActiveState()
    {
        var cut = Render<ShadcnStateTransition>(parameters => parameters
            .Add(component => component.Active, true)
            .Add(component => component.ActiveContent, "Preparing")
            .Add(component => component.InactiveContent, "Paused"));

        var root = cut.Find("[data-slot='state-transition']");
        Assert.Equal("active", root.GetAttribute("data-state"));
        Assert.Equal("false", cut.Find("[data-slot='state-transition-active']").GetAttribute("aria-hidden"));
        Assert.Equal("true", cut.Find("[data-slot='state-transition-inactive']").GetAttribute("aria-hidden"));
        Assert.True(cut.Find("[data-slot='state-transition-inactive']").HasAttribute("inert"));
        Assert.Contains("Preparing", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Paused", cut.Markup, StringComparison.Ordinal);

        cut.Render(parameters => parameters
            .Add(component => component.Active, false)
            .Add(component => component.ActiveContent, "Preparing")
            .Add(component => component.InactiveContent, "Paused"));

        Assert.Equal("inactive", root.GetAttribute("data-state"));
        Assert.True(cut.Find("[data-slot='state-transition-active']").HasAttribute("inert"));
        Assert.Equal("false", cut.Find("[data-slot='state-transition-inactive']").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SupportsDisabledAndExplicitReducedMotionModes()
    {
        var cut = Render<ShadcnStateTransition>(parameters => parameters
            .Add(component => component.Enabled, false)
            .Add(component => component.ReducedMotion, true)
            .Add(component => component.ActiveContent, "Preparing")
            .Add(component => component.InactiveContent, "Paused"));

        var root = cut.Find("[data-slot='state-transition']");
        Assert.Equal("false", root.GetAttribute("data-motion"));
        Assert.Equal("true", root.GetAttribute("data-reduced-motion"));
    }

    [Fact]
    public void PackageCssUsesMountedGridLayersAndThemeMotionTokens()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-base.css"));

        Assert.Contains(".shadcn-state-transition", css, StringComparison.Ordinal);
        Assert.Contains("grid-area: 1 / 1", css, StringComparison.Ordinal);
        Assert.Contains("var(--shadcn-motion-duration)", css, StringComparison.Ordinal);
        Assert.Contains("data-reduced-motion=\"true\"", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
