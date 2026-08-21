using Bunit;
using Maliev.ShadcnBlazor.Showcase.Pages;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DocumentationControlTests : BunitContext
{
    public DocumentationControlTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void DisclosureDocumentationUsesPackageSelectsForItsExampleControls()
    {
        var cut = Render<DisclosureAndNavigation>();

        Assert.Empty(cut.FindAll("[data-testid='disclosure-navigation-fixture'] select"));
        Assert.Equal(2, cut.FindAll("[data-testid='disclosure-navigation-fixture'] [data-slot='select']").Count);

        var tabsControl = cut.Find("[data-testid='disclosure-tabs-control']");
        Assert.Equal("Selected tab", tabsControl.GetAttribute("aria-label"));
        tabsControl.Click();
        cut.Find("[role='option'][data-value='history']").Click();

        Assert.Equal("true", cut.Find("[role='tab'][data-value='history']").GetAttribute("aria-selected"));
        Assert.Contains("Project history.", cut.Markup, StringComparison.Ordinal);
    }
}
