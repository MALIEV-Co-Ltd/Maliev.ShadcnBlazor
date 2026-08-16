using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ButtonShowcaseTests : BunitContext
{
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

        var status = cut.Find("[data-testid='button-last-action']");
        Assert.Equal("status", status.GetAttribute("role"));
        Assert.Equal("polite", status.GetAttribute("aria-live"));
        Assert.Contains("Choose an enabled action", status.TextContent, StringComparison.Ordinal);
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
        Assert.All(disabledCut.FindAll("[data-slot='button']"), button => Assert.True(button.HasAttribute("disabled")));
    }
}
