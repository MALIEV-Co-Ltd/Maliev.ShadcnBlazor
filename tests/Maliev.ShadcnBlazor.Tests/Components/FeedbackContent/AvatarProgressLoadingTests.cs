using Bunit;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Feedback;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.FeedbackContent;

public sealed class AvatarProgressLoadingTests : BunitContext
{
    [Fact]
    public void AvatarRingUsesPinnedBorderAndBlendTreatment()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));
        Assert.Contains(".shadcn-avatar::after", css, StringComparison.Ordinal);
        Assert.Contains("border: 1px solid var(--shadcn-border)", css, StringComparison.Ordinal);
        Assert.Contains("mix-blend-mode: darken", css, StringComparison.Ordinal);
        Assert.Contains("mix-blend-mode: lighten", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--shadcn-success", css, StringComparison.Ordinal);
    }
    [Theory]
    [InlineData(ShadcnAvatarSize.Small, "sm")]
    [InlineData(ShadcnAvatarSize.Default, "default")]
    [InlineData(ShadcnAvatarSize.Large, "lg")]
    public void AvatarRendersEveryPinnedSizeAndComposition(ShadcnAvatarSize size, string expected)
    {
        var cut = Render<ShadcnAvatar>(parameters => parameters
            .Add(component => component.Size, size)
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["aria-label"] = "Operator" })
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnAvatarImage>(0);
                builder.AddAttribute(1, nameof(ShadcnAvatarImage.Source), "/operator.webp");
                builder.AddAttribute(2, nameof(ShadcnAvatarImage.Alt), "Thai CNC operator");
                builder.CloseComponent();
                builder.OpenComponent<ShadcnAvatarFallback>(3);
                builder.AddAttribute(4, nameof(ShadcnAvatarFallback.ChildContent), Text("NO"));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnAvatarBadge>(5);
                builder.AddAttribute(6, nameof(ShadcnAvatarBadge.ChildContent), Text("Online"));
                builder.CloseComponent();
            }));

        var root = cut.Find("[data-slot='avatar']");
        Assert.Equal(expected, root.GetAttribute("data-size"));
        Assert.Equal("Operator", root.GetAttribute("aria-label"));
        Assert.Equal("Thai CNC operator", cut.Find("[data-slot='avatar-image']").GetAttribute("alt"));
        Assert.Equal("NO", cut.Find("[data-slot='avatar-fallback']").TextContent);
        Assert.Equal("Online", cut.Find("[data-slot='avatar-badge']").TextContent);
    }

    [Fact]
    public void AvatarFallbackTracksImageLoadAndErrorWithoutLayoutRemoval()
    {
        var loaded = 0;
        var failed = 0;
        var cut = Render<ShadcnAvatar>(parameters => parameters.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnAvatarImage>(0);
            builder.AddAttribute(1, nameof(ShadcnAvatarImage.Source), "/avatar.webp");
            builder.AddAttribute(2, nameof(ShadcnAvatarImage.Alt), "Operator");
            builder.AddAttribute(3, nameof(ShadcnAvatarImage.OnLoad), EventCallback.Factory.Create<EventArgs>(this, () => loaded++));
            builder.AddAttribute(4, nameof(ShadcnAvatarImage.OnError), EventCallback.Factory.Create<EventArgs>(this, () => failed++));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAvatarFallback>(5);
            builder.AddAttribute(6, nameof(ShadcnAvatarFallback.ChildContent), Text("OP"));
            builder.CloseComponent();
        }));

        Assert.Equal("visible", cut.Find("[data-slot='avatar-fallback']").GetAttribute("data-state"));
        cut.Find("img").TriggerEvent("onload", new ProgressEventArgs());
        Assert.Equal(1, loaded);
        Assert.Equal("hidden", cut.Find("[data-slot='avatar-fallback']").GetAttribute("data-state"));
        cut.Find("img").TriggerEvent("onerror", new Microsoft.AspNetCore.Components.Web.ErrorEventArgs());
        Assert.Equal(1, failed);
        Assert.Equal("visible", cut.Find("[data-slot='avatar-fallback']").GetAttribute("data-state"));
        Assert.Equal("error", cut.Find("[data-slot='avatar-image']").GetAttribute("data-state"));
        Assert.NotNull(cut.Find("[data-slot='avatar-image']"));
    }

    [Fact]
    public void AvatarImageContractFillsTheCircularFrameAndPresenceUsesSuccessColor()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));

        Assert.Contains(".shadcn-avatar-image { display: block;", css, StringComparison.Ordinal);
        Assert.Contains("object-fit: cover", css, StringComparison.Ordinal);
        Assert.Contains("object-position: center", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--shadcn-success", css, StringComparison.Ordinal);
        Assert.Contains("inset-inline-end: 0", css, StringComparison.Ordinal);
        Assert.Contains("margin-inline-start: calc(-1 * var(--shadcn-avatar-overlap))", css, StringComparison.Ordinal);

        var showcaseCss = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));
        Assert.Contains("@media (prefers-reduced-motion: reduce)", showcaseCss, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 36rem)", showcaseCss, StringComparison.Ordinal);
        Assert.Contains(".showcase-avatar-gallery { grid-template-columns: 1fr; }", showcaseCss, StringComparison.Ordinal);
    }

    [Fact]
    public void AvatarSourceChangeReturnsToFallbackBeforeNewImageLoads()
    {
        var source = "/first.webp";
        RenderFragment content = builder =>
        {
            builder.OpenComponent<ShadcnAvatarImage>(0);
            builder.AddAttribute(1, nameof(ShadcnAvatarImage.Source), source);
            builder.AddAttribute(2, nameof(ShadcnAvatarImage.Alt), "Operator");
            builder.CloseComponent();
            builder.OpenComponent<ShadcnAvatarFallback>(3);
            builder.AddAttribute(4, nameof(ShadcnAvatarFallback.ChildContent), Text("OP"));
            builder.CloseComponent();
        };
        var cut = Render<ShadcnAvatar>(parameters => parameters.AddChildContent(content));
        cut.Find("img").TriggerEvent("onload", new ProgressEventArgs());
        Assert.Equal("hidden", cut.Find("[data-slot='avatar-fallback']").GetAttribute("data-state"));

        source = "/second.webp";
        cut.Render(parameters => parameters.AddChildContent(content));

        Assert.Equal("visible", cut.Find("[data-slot='avatar-fallback']").GetAttribute("data-state"));
        Assert.Equal("loading", cut.Find("img").GetAttribute("data-state"));
    }

    [Fact]
    public void AvatarGroupAndCountExposePinnedSlotsAndRtlLogicalStacking()
    {
        var cut = Render<ShadcnAvatarGroup>(parameters => parameters
            .Add(component => component.Overlap, "0.5rem")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnAvatarGroupCount>(0);
                builder.AddAttribute(1, nameof(ShadcnAvatarGroupCount.ChildContent), Text("+4"));
                builder.AddAttribute(2, nameof(ShadcnAvatarGroupCount.Size), ShadcnAvatarSize.Large);
                builder.CloseComponent();
            }));

        var group = cut.Find("[data-slot='avatar-group']");
        Assert.Contains("--shadcn-avatar-overlap: 0.5rem", group.GetAttribute("style"));
        Assert.Equal("+4", cut.Find("[data-slot='avatar-group-count']").TextContent);
        Assert.Equal("lg", cut.Find("[data-slot='avatar-group-count']").GetAttribute("data-size"));
    }

    [Fact]
    public void ProgressLabelParameterFallsBackWhenCompositionHasNoLabelAndExposesValueText()
    {
        var cut = Render<ShadcnProgress>(parameters => parameters
            .Add(component => component.Label, "งานที่เสร็จ")
            .Add(component => component.Value, 50)
            .Add(component => component.ValueFormatter, value => $"{value:0} จาก 100")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnProgressTrack>(0);
                builder.AddAttribute(1, nameof(ShadcnProgressTrack.ChildContent), (RenderFragment)(track => { track.OpenComponent<ShadcnProgressIndicator>(0); track.CloseComponent(); }));
                builder.CloseComponent();
            }));
        var root = cut.Find("[data-slot='progress']");
        Assert.Equal("งานที่เสร็จ", root.GetAttribute("aria-label"));
        Assert.Null(root.GetAttribute("aria-labelledby"));
        Assert.Equal("50 จาก 100", root.GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void UnlabelledCompositionDoesNotEmitDanglingLabelReference()
    {
        var cut = Render<ShadcnProgress>(parameters => parameters.Add(component => component.Value, 25).AddChildContent<ShadcnProgressTrack>());
        Assert.Null(cut.Find("[data-slot='progress']").GetAttribute("aria-labelledby"));
    }

    [Theory]
    [InlineData(0, 100, 25, "25", "25%")]
    [InlineData(10, 20, 25, "20", "100%")]
    [InlineData(10, 20, 5, "10", "0%")]
    public void ProgressClampsDeterminateSemanticsAndIndicator(
        double minimum,
        double maximum,
        double value,
        string expectedValue,
        string expectedPercent)
    {
        var cut = Render<ShadcnProgress>(parameters => parameters
            .Add(component => component.Minimum, minimum)
            .Add(component => component.Maximum, maximum)
            .Add(component => component.Value, value)
            .Add(component => component.Label, "Upload")
            .Add(component => component.ShowValue, true));

        var root = cut.Find("[data-slot='progress']");
        Assert.Equal("progressbar", root.GetAttribute("role"));
        Assert.Equal(minimum.ToString(System.Globalization.CultureInfo.InvariantCulture), root.GetAttribute("aria-valuemin"));
        Assert.Equal(maximum.ToString(System.Globalization.CultureInfo.InvariantCulture), root.GetAttribute("aria-valuemax"));
        Assert.Equal(expectedValue, root.GetAttribute("aria-valuenow"));
        Assert.Equal("Upload", root.GetAttribute("aria-label"));
        var indicatorStyle = cut.Find("[data-slot='progress-indicator']").GetAttribute("style");
        Assert.Contains($"--shadcn-progress-percent: {expectedPercent}", indicatorStyle);
        Assert.Contains("--shadcn-progress-ratio:", indicatorStyle);
        Assert.NotEmpty(cut.Find("[data-slot='progress-value']").TextContent);
    }

    [Fact]
    public void ProgressNullValueIsIndeterminateAndHasNoValueNow()
    {
        var cut = Render<ShadcnProgress>(parameters => parameters
            .Add(component => component.Value, (double?)null)
            .Add(component => component.Label, "กำลังอัปโหลด"));

        var root = cut.Find("[data-slot='progress']");
        Assert.Equal("indeterminate", root.GetAttribute("data-state"));
        Assert.Null(root.GetAttribute("aria-valuenow"));
        Assert.Equal("กำลังอัปโหลด", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void ComposedProgressNamesRootAndSuppliesFormattedValueToEmptyValueSlot()
    {
        var cut = Render<ShadcnProgress>(parameters => parameters
            .Add(component => component.Minimum, 10)
            .Add(component => component.Maximum, 20)
            .Add(component => component.Value, 25)
            .Add(component => component.ValueFormatter, value => $"{value:0} งาน")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnProgressLabel>(0);
                builder.AddAttribute(1, nameof(ShadcnProgressLabel.ChildContent), Text("อัปโหลด"));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnProgressValue>(2);
                builder.CloseComponent();
                builder.OpenComponent<ShadcnProgressTrack>(3);
                builder.AddAttribute(4, nameof(ShadcnProgressTrack.ChildContent), (RenderFragment)(track =>
                {
                    track.OpenComponent<ShadcnProgressIndicator>(0);
                    track.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var root = cut.Find("[data-slot='progress']");
        var label = cut.Find("[data-slot='progress-label']");
        Assert.Equal(label.Id, root.GetAttribute("aria-labelledby"));
        Assert.Null(root.GetAttribute("aria-label"));
        Assert.Equal("20 งาน", cut.Find("[data-slot='progress-value']").TextContent);
        var indicatorStyle = cut.Find("[data-slot='progress-indicator']").GetAttribute("style");
        Assert.Contains("--shadcn-progress-percent: 100%", indicatorStyle);
        Assert.Contains("--shadcn-progress-ratio: 1", indicatorStyle);
    }

    [Fact]
    public void ProgressIndicatorUsesTransformRatioForLiveValueUpdates()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-feedback-content.css"));
        Assert.Contains("transform: scaleX(var(--shadcn-progress-ratio, 0))", css, StringComparison.Ordinal);
        Assert.Contains("transition: transform", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-progress-indicator { transition: none; }", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-progress-indicator[data-state=\"indeterminate\"] { animation: none;", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ProgressRerendersComposedIndicatorAndValueWhenValueChanges()
    {
        var cut = Render<ShadcnProgress>(parameters => parameters
            .Add(component => component.Value, 64)
            .Add(component => component.Label, "Upload")
            .Add(component => component.ShowValue, true));

        cut.Render(parameters => parameters
            .Add(component => component.Value, 5)
            .Add(component => component.Label, "Upload")
            .Add(component => component.ShowValue, true));

        var root = cut.Find("[data-slot='progress']");
        Assert.Equal("5", root.GetAttribute("aria-valuenow"));
        Assert.Equal("5%", cut.Find("[data-slot='progress-value']").TextContent);
        Assert.Contains("--shadcn-progress-percent: 5%", cut.Find("[data-slot='progress-indicator']").GetAttribute("style"));
        Assert.Contains("--shadcn-progress-ratio: 0.05", cut.Find("[data-slot='progress-indicator']").GetAttribute("style"));
    }

    [Theory]
    [InlineData(ShadcnSkeletonShape.Default, ShadcnSkeletonAnimation.Pulse, "default", "pulse")]
    [InlineData(ShadcnSkeletonShape.Circle, ShadcnSkeletonAnimation.None, "circle", "none")]
    public void SkeletonExposesShapeMotionAndDecorativeSemantics(
        ShadcnSkeletonShape shape,
        ShadcnSkeletonAnimation animation,
        string expectedShape,
        string expectedAnimation)
    {
        var cut = Render<ShadcnSkeleton>(parameters => parameters
            .Add(component => component.Shape, shape)
            .Add(component => component.Animation, animation)
            .Add(component => component.Decorative, true));

        var skeleton = cut.Find("[data-slot='skeleton']");
        Assert.Equal(expectedShape, skeleton.GetAttribute("data-shape"));
        Assert.Equal(expectedAnimation, skeleton.GetAttribute("data-animation"));
        Assert.Equal("true", skeleton.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SpinnerUsesPinnedLoaderPathAndCustomizableLocalizedName()
    {
        var cut = Render<ShadcnSpinner>(parameters => parameters
            .Add(component => component.Label, "กำลังโหลด")
            .Add(component => component.Size, "1.25rem"));

        var spinner = cut.Find("svg[data-slot='spinner']");
        Assert.Equal("status", spinner.GetAttribute("role"));
        Assert.Equal("กำลังโหลด", spinner.GetAttribute("aria-label"));
        Assert.Contains("--shadcn-spinner-size: 1.25rem", spinner.GetAttribute("style"));
        Assert.Equal("M21 12a9 9 0 1 1-6.219-8.56", spinner.QuerySelector("path")!.GetAttribute("d"));
    }

    [Fact]
    public void SpinnerCanBeDecorative()
    {
        var cut = Render<ShadcnSpinner>(parameters => parameters.Add(component => component.Label, null));
        var spinner = cut.Find("svg[data-slot='spinner']");
        Assert.Null(spinner.GetAttribute("role"));
        Assert.Equal("true", spinner.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SpinnerUsesTypedRoleAndRejectsWhitespaceNames()
    {
        var cut = Render<ShadcnSpinner>(parameters => parameters
            .Add(component => component.SpinnerRole, ShadcnSpinnerRole.None)
            .Add(component => component.Label, "Ignored when decorative"));
        Assert.Null(cut.Find("svg").GetAttribute("role"));
        Assert.Equal("true", cut.Find("svg").GetAttribute("aria-hidden"));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSpinner>(parameters => parameters.Add(component => component.Label, "   ")));
    }

    [Fact]
    public void LoadingComponentsRejectUnknownEnumsAndInvalidRanges()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAvatar>(parameters => parameters.Add(component => component.Size, (ShadcnAvatarSize)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnAvatarGroup>(parameters => parameters.Add(component => component.Overlap, "1rem;color:red")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnProgress>(parameters => parameters.Add(component => component.Minimum, 10).Add(component => component.Maximum, 10)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnProgress>(parameters => parameters.Add(component => component.Value, double.NaN)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSkeleton>(parameters => parameters.Add(component => component.Shape, (ShadcnSkeletonShape)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSkeleton>(parameters => parameters.Add(component => component.Animation, (ShadcnSkeletonAnimation)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSpinner>(parameters => parameters.Add(component => component.Size, "1rem;display:none")));
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
}
