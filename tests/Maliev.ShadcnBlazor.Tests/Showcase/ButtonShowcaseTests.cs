using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ButtonShowcaseTests : BunitContext
{
    [Fact]
    public async Task FocusExampleRequestsFocusThroughTheComponentReference()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Render<ButtonDossierPreview>();

        await cut.Find("[data-testid='button-focus-request']").ClickAsync(new MouseEventArgs());

        var invocation = Assert.Single(
            JSInterop.Invocations,
            candidate => candidate.Identifier == "Blazor._internal.domWrapper.focus");
        var element = Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.False(string.IsNullOrWhiteSpace(element.Id));
        Assert.Equal(true, invocation.Arguments[1]);
    }

    [Fact]
    public void PreviewRendersAllVariantsAndSizesWithAnAccessibleStatusRegion()
    {
        var cut = Render<ButtonDossierPreview>();

        Assert.Equal(6, cut.FindAll("[data-testid^='button-variant-']").Count);
        Assert.Equal(
            ["default", "destructive", "outline", "secondary", "ghost", "link"],
            cut.FindAll("[data-testid^='button-variant-']").Select(button => button.GetAttribute("data-variant")));
        Assert.Equal(4, cut.FindAll(".showcase-button-dossier__sizes [data-slot='button']").Count);
        Assert.Equal(["xs", "sm", "default", "lg"], cut.FindAll(".showcase-button-dossier__sizes [data-slot='button']").Select(button => button.GetAttribute("data-size")));
        Assert.Equal(4, cut.FindAll(".showcase-button-dossier__icon-sizes [data-slot='button']").Count);
        Assert.Equal(["icon-xs", "icon-sm", "icon", "icon-lg"], cut.FindAll(".showcase-button-dossier__icon-sizes [data-slot='button']").Select(button => button.GetAttribute("data-size")));
        Assert.All(cut.FindAll(".showcase-button-dossier__icon-sizes [data-slot='button']"), button => Assert.False(string.IsNullOrWhiteSpace(button.GetAttribute("aria-label"))));
        Assert.Equal("#usage", cut.Find("[data-testid='button-variant-link']").GetAttribute("href"));
        Assert.Empty(cut.FindAll(".showcase-button-dossier__eyebrow"));

        var status = cut.Find("[data-testid='button-last-action']");
        Assert.Equal("status", status.GetAttribute("role"));
        Assert.Equal("polite", status.GetAttribute("aria-live"));
        Assert.Contains("Choose an enabled action", status.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void ExampleSourceMatchesTheCurrentDisabledStateAndCompleteRenderedSurface()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var example = Assert.Single(registry.GetBySlug("button"));

        Assert.Contains("Disabled=\"false\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnButtonSize.IconExtraSmall", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Href=\"#usage\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("PointerCursor=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@ref=\"_primaryAction\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("FocusAsync(preventScroll: true)", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<input type=\"checkbox\"", example.RazorSource, StringComparison.Ordinal);

        Assert.Single(example.Controls).Apply("true");

        Assert.Contains("Disabled=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Disabled=\"false\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void EnabledActionsAnnounceThePressedVariantAndDisabledModeSuppressesIt()
    {
        var cut = Render<ButtonDossierPreview>();
        var defaultButton = cut.Find("[data-testid='button-variant-default']");

        defaultButton.Click();
        Assert.Contains("Save changes pressed", cut.Find("[data-testid='button-last-action']").TextContent, StringComparison.Ordinal);

        var disabledCut = Render<ButtonDossierPreview>(parameters => parameters.Add(component => component.Disabled, true));
        disabledCut.Find("[data-testid='button-variant-destructive']").Click();

        Assert.Contains("Choose an enabled action", disabledCut.Find("[data-testid='button-last-action']").TextContent, StringComparison.Ordinal);
        Assert.All(disabledCut.FindAll("button[data-slot='button']"), button => Assert.True(button.HasAttribute("disabled")));
        Assert.Equal("true", disabledCut.Find("a[data-slot='button']").GetAttribute("aria-disabled"));
        Assert.Null(disabledCut.Find("a[data-slot='button']").GetAttribute("href"));
    }
}
