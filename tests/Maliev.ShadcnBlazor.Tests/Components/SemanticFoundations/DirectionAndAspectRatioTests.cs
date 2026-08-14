using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Components;
using Maliev.ShadcnBlazor.Components.Direction;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.SemanticFoundations;

public sealed class DirectionAndAspectRatioTests : BunitContext
{
    public DirectionAndAspectRatioTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void DirectionProviderDefaultsToNearestContextAndCascadesIt()
    {
        ShadcnContext? observed = null;
        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(x => x.IsDarkMode, true)
            .Add(x => x.Direction, ShadcnDirection.RightToLeft)
            .AddChildContent<ShadcnDirectionProvider>(direction => direction
                .Add(x => x.Class, "nested-direction")
                .AddChildContent<CaptureContext>(capture => capture
                    .Add(x => x.OnCaptured, value => observed = value))));

        var provider = cut.Find("[data-slot='direction']");
        Assert.Equal("rtl", provider.GetAttribute("dir"));
        Assert.Contains("nested-direction", provider.ClassList);
        Assert.Equal(new ShadcnContext(true, ShadcnDirection.RightToLeft), observed);
    }

    [Fact]
    public void DirectionProviderOverridesOnlyDirectionAndProtectsOwnedAttributes()
    {
        ShadcnContext? observed = null;
        var cut = Render<ShadcnThemeProvider>(parameters => parameters
            .Add(x => x.IsDarkMode, true)
            .Add(x => x.Direction, ShadcnDirection.RightToLeft)
            .AddChildContent<ShadcnDirectionProvider>(direction => direction
                .Add(x => x.Direction, ShadcnDirection.LeftToRight)
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
                {
                    ["dir"] = "auto",
                    ["data-slot"] = "wrong",
                    ["aria-label"] = "left-to-right example"
                })
                .AddChildContent<CaptureContext>(capture => capture
                    .Add(x => x.OnCaptured, value => observed = value))));

        var provider = cut.Find("[data-slot='direction']");
        Assert.Equal("ltr", provider.GetAttribute("dir"));
        Assert.Equal("direction", provider.GetAttribute("data-slot"));
        Assert.Equal("left-to-right example", provider.GetAttribute("aria-label"));
        Assert.Equal(new ShadcnContext(true, ShadcnDirection.LeftToRight), observed);
    }

    [Fact]
    public void DirectionProviderRejectsUnknownDirection()
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnDirectionProvider>(parameters => parameters
            .Add(x => x.Direction, (ShadcnDirection)999)));

        Assert.Contains("direction", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(16d / 9d, "1.7777777777777777")]
    [InlineData(1d, "1")]
    [InlineData(9d / 16d, "0.5625")]
    public void AspectRatioRendersRequestedGeometry(double ratio, string serializedRatio)
    {
        var cut = Render<ShadcnAspectRatio>(parameters => parameters
            .Add(x => x.Ratio, ratio)
            .Add(x => x.Class, "media-frame")
            .Add(x => x.Style, "max-width: 40rem")
            .Add(x => x.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-slot"] = "wrong",
                ["data-test-id"] = "ratio-fixture"
            })
            .AddChildContent("media"));

        var root = cut.Find("[data-slot='aspect-ratio']");
        Assert.Contains("shadcn-aspect-ratio", root.ClassList);
        Assert.Contains("media-frame", root.ClassList);
        Assert.Equal("ratio-fixture", root.GetAttribute("data-test-id"));
        Assert.Equal(
            $"position: relative; width: 100%; aspect-ratio: {serializedRatio}; max-width: 40rem",
            root.GetAttribute("style"));
        Assert.Equal("media", root.QuerySelector("[data-slot='aspect-ratio-content']")!.TextContent);
    }

    [Fact]
    public void AspectRatioUsesInvariantCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            var cut = Render<ShadcnAspectRatio>(parameters => parameters.Add(x => x.Ratio, 1.5));

            Assert.Contains("aspect-ratio: 1.5", cut.Find("[data-slot='aspect-ratio']").GetAttribute("style"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AspectRatioRejectsNonPositiveOrNonFiniteValues(double ratio)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<ShadcnAspectRatio>(parameters =>
            parameters.Add(x => x.Ratio, ratio)));

        Assert.Contains("ratio", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CaptureContext : ComponentBase
    {
        [CascadingParameter]
        public ShadcnContext Context { get; set; }

        [Parameter]
        public Action<ShadcnContext>? OnCaptured { get; set; }

        protected override void OnParametersSet() => OnCaptured?.Invoke(Context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult();

        base.Dispose(disposing);
    }
}
