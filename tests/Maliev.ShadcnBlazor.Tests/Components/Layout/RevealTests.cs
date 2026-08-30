using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Components.Layout;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Components.Layout;

public sealed class RevealTests : BunitContext
{
    public RevealTests() => Services.AddMalievShadcn();

    [Fact]
    public void RevealGroupRendersSemanticBoundaryAndConfiguredMotionContract()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<ShadcnRevealGroup>(parameters => parameters
            .Add(component => component.Tag, "section")
            .Add(component => component.Threshold, 0.2)
            .Add(component => component.RootMargin, "48px 0px")
            .Add(component => component.Stagger, 90)
            .Add(component => component.Paused, true)
            .AddChildContent("workflow"));

        var root = cut.Find("[data-slot='motion-reveal']");

        Assert.Equal("SECTION", root.TagName);
        Assert.Equal("0.2", root.GetAttribute("data-reveal-threshold"));
        Assert.Equal("48px 0px", root.GetAttribute("data-reveal-root-margin"));
        Assert.Equal("90", root.GetAttribute("data-reveal-stagger"));
        Assert.Equal("true", root.GetAttribute("data-reveal-paused"));
        Assert.Equal("workflow", root.TextContent);
    }

    [Fact]
    public void RevealRendersTypedEffectAndTimingMetadata()
    {
        var cut = Render<ShadcnReveal>(parameters => parameters
            .Add(component => component.Tag, "article")
            .Add(component => component.Effect, ShadcnRevealEffect.Clip)
            .Add(component => component.Delay, 120)
            .Add(component => component.Duration, 540)
            .Add(component => component.Cascade, true)
            .AddChildContent("inspection"));

        var root = cut.Find("[data-slot='reveal']");

        Assert.Equal("ARTICLE", root.TagName);
        Assert.Equal("clip", root.GetAttribute("data-reveal-effect"));
        Assert.Equal("true", root.GetAttribute("data-reveal-cascade"));
        Assert.Contains("--shadcn-reveal-delay: 120ms", root.GetAttribute("style"));
        Assert.Contains("--shadcn-reveal-duration: 540ms", root.GetAttribute("style"));
    }

    [Fact]
    public void ExplicitReducedMotionKeepsTheGroupVisibleWithoutRuntimeAttachment()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<ShadcnRevealGroup>(parameters => parameters
            .Add(component => component.ReducedMotion, true)
            .AddChildContent<ShadcnReveal>(item => item.AddChildContent("ready")));

        Assert.Equal("true", cut.Find("[data-slot='motion-reveal']").GetAttribute("data-reveal-reduced-motion"));
        Assert.DoesNotContain(JSInterop.Invocations, invocation => invocation.Identifier == "import");
    }

    [Fact]
    public void DisabledRevealDoesNotExposeAnimationMetadata()
    {
        var cut = Render<ShadcnReveal>(parameters => parameters
            .Add(component => component.Disabled, true)
            .Add(component => component.Cascade, true)
            .AddChildContent("static"));

        var root = cut.Find("[data-slot='reveal']");

        Assert.Equal("true", root.GetAttribute("data-reveal-disabled"));
        Assert.Null(root.GetAttribute("data-reveal-effect"));
        Assert.Null(root.GetAttribute("data-reveal-cascade"));
    }

    [Fact]
    public void RevealRejectsUnsafeSemanticTags()
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnReveal>(parameters => parameters
            .Add(component => component.Tag, "script")));

        Assert.Contains("tag", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RevealGroupImportsAndAttachesTheSharedRuntime()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        _ = Render<ShadcnRevealGroup>(parameters => parameters
            .AddChildContent<ShadcnReveal>(item => item.AddChildContent("ready")));

        Assert.Contains(JSInterop.Invocations, invocation =>
            invocation.Identifier == "import" &&
            invocation.Arguments.Any(argument => string.Equals(
                argument?.ToString(),
                "./_content/Maliev.ShadcnBlazor/js/shadcn-reveal.js",
                StringComparison.Ordinal)));
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "attachRevealGroup");
    }

    [Fact]
    public void SharedRuntimeOwnsObservationMutationAndReducedMotionBehavior()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
        var script = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-reveal.js"));
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-layout.css"));

        Assert.Contains("IntersectionObserver", script, StringComparison.Ordinal);
        Assert.Contains("MutationObserver", script, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", script, StringComparison.Ordinal);
        Assert.Contains("data-reveal-state", script, StringComparison.Ordinal);
        Assert.DoesNotContain("immediatelyVisible", script, StringComparison.Ordinal);
        Assert.Contains(".shadcn-reveal[data-reveal-state='pending']", css, StringComparison.Ordinal);
        Assert.Contains("visibility: hidden", css, StringComparison.Ordinal);
        Assert.Contains("visibility: visible", css, StringComparison.Ordinal);
        Assert.Contains("[data-reveal-cascade='true'] .shadcn-chart", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains("visibility: visible !important", css, StringComparison.Ordinal);
    }
}
