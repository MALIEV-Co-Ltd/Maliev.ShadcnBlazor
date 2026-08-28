using Bunit;
using Maliev.ShadcnBlazor.Showcase;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Layout;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DocumentationNavigationTests : BunitContext
{
    public DocumentationNavigationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        Services.AddSingleton<IComponentDocumentationCatalog>(new ComponentDocumentationCatalog());
        Services.AddScoped<ShowcaseState>();
        Services.AddScoped<DocumentationPageState>();
    }

    [Fact]
    public void State_NormalizesQueryAndFiltersAndRaisesOneChangePerMutation()
    {
        var state = new DocumentationNavigationState();
        var changes = 0;
        state.Changed += (_, _) => changes++;

        state.Query = "  keyboard   shortcut ";
        state.Category = "  Foundation ";
        state.Status = ComponentDocumentationStatus.Complete;

        Assert.Equal("keyboard shortcut", state.Query);
        Assert.Equal("Foundation", state.Category);
        Assert.Equal(ComponentDocumentationStatus.Complete, state.Status);
        Assert.Equal(3, changes);
    }

    [Fact]
    public void State_DrawersAreMutuallyExclusiveAndEscapeClosesTheOpenDrawer()
    {
        var state = new DocumentationNavigationState();

        state.CatalogOpen = true;
        state.OutlineOpen = true;

        Assert.False(state.CatalogOpen);
        Assert.True(state.OutlineOpen);
        Assert.True(state.CloseDrawers());
        Assert.False(state.OutlineOpen);
        Assert.False(state.CloseDrawers());
    }

    [Fact]
    public void CatalogRail_AnnouncesFilteredCountAndMarksTheCurrentRoute()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/docs/components/kbd");
        var state = new DocumentationNavigationState
        {
            Query = "kbd",
            Category = "Foundation",
            Status = ComponentDocumentationStatus.Complete
        };

        var cut = Render<DocumentationCatalogRail>(parameters => parameters.Add(x => x.State, state));

        Assert.Equal("1 component found", cut.Find("[data-testid='documentation-result-count']").TextContent.Trim());
        Assert.Equal("page", cut.Find("a[href='docs/components/kbd']").GetAttribute("aria-current"));
    }

    [Fact]
    public void CatalogRail_GroupsEveryComponentAndOffersRecoveryForEmptySearch()
    {
        var state = new DocumentationNavigationState();
        var cut = Render<DocumentationCatalogRail>(parameters => parameters.Add(component => component.State, state));

        Assert.Equal(69, cut.FindAll(".documentation-component-list a").Count);
        Assert.Equal(
            new[] { "Composition", "Data", "Feedback", "Forms", "Foundation", "Layout", "Overlays" },
            cut.FindAll(".documentation-category h3").Select(heading => heading.TextContent.Trim()));

        state.Query = "no-such-component";

        Assert.Equal("No components found", cut.Find("[role='status']").TextContent.Trim());
        Assert.Equal("Clear search", cut.Find("[data-testid='clear-component-search']").TextContent.Trim());
        cut.Find("[data-testid='clear-component-search']").Click();
        Assert.Equal(69, cut.FindAll(".documentation-component-list a").Count);
    }

    [Fact]
    public void PageState_NormalizesUniqueSectionsAndClearsWithoutDuplicateNotifications()
    {
        var state = new DocumentationPageState();
        var changes = 0;
        state.Changed += (_, _) => changes++;

        state.SetSections([
            new DocumentationSection(" usage ", " Usage "),
            new DocumentationSection("usage", "Duplicate"),
            new DocumentationSection("api-reference", "API Reference")
        ]);
        state.SetSections(state.Sections);

        Assert.Equal(
            [new DocumentationSection("usage", "Usage"), new DocumentationSection("api-reference", "API Reference")],
            state.Sections);
        Assert.Equal(1, changes);

        state.Clear();
        state.Clear();
        Assert.Empty(state.Sections);
        Assert.Equal(2, changes);
    }

    [Fact]
    public void OnThisPage_RendersOnlyPublishedSections()
    {
        var state = new DocumentationPageState();
        state.SetSections([
            new DocumentationSection("overview", "Overview"),
            new DocumentationSection("usage", "Usage")
        ]);

        var cut = Render<DocumentationOnThisPage>(parameters => parameters.Add(component => component.State, state));

        Assert.Equal("On This Page", cut.Find("h2").TextContent.Trim());
        Assert.Equal(new[] { "#overview", "#usage" }, cut.FindAll("a").Select(link => link.GetAttribute("href")));
        state.Clear();
        Assert.Empty(cut.FindAll("nav"));
    }

    [Fact]
    public void Layout_HasLandmarkSkipTargetsAndEscapeClosesDrawersWithoutMutatingTheme()
    {
        var theme = Services.GetRequiredService<ShowcaseState>();
        var cut = Render<DocumentationLayout>(parameters => parameters
            .Add(x => x.Body, (RenderFragment)(builder => builder.AddContent(0, "Documentation body"))));
        var navigation = cut.FindComponent<DocumentationHeader>().Instance.State;

        Assert.Equal("#documentation-catalog", cut.Find("a[href='#documentation-catalog']").GetAttribute("href"));
        Assert.Equal("#documentation-content", cut.Find("a[href='#documentation-content']").GetAttribute("href"));
        Assert.Single(cut.FindAll("header"));
        Assert.Single(cut.FindAll("nav#documentation-catalog"));
        Assert.Single(cut.FindAll("main#documentation-content"));
        Assert.Single(cut.FindAll("aside#documentation-outline"));
        Assert.Empty(cut.FindAll("#documentation-theme"));
        Assert.Single(cut.FindAll("[data-testid='documentation-theme-toggle']"));
        Assert.Single(cut.FindAll("[data-testid='documentation-direction-toggle']"));

        navigation.CatalogOpen = true;
        cut.Find(".documentation-shell").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.False(navigation.CatalogOpen);
        Assert.False(theme.IsDarkMode);
        Assert.Equal(ShadcnDirection.LeftToRight, theme.Direction);
    }

    [Fact]
    public void Layout_CatalogSkipLinkOpensTheDrawerAndMovesFocusToItsNavigationTarget()
    {
        var cut = RenderDocumentationLayout();
        var navigation = cut.FindComponent<DocumentationHeader>().Instance.State;

        cut.Find("a[href='#documentation-catalog']").Click();

        Assert.True(navigation.CatalogOpen);
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier.Contains("focus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Layout_CloseButtonsRestoreFocusThroughTheMatchingHeaderTrigger()
    {
        Services.GetRequiredService<DocumentationPageState>().SetSections([new("overview", "Overview")]);
        var cut = RenderDocumentationLayout();
        var navigation = cut.FindComponent<DocumentationHeader>().Instance.State;

        cut.Find("[data-testid='catalog-trigger']").Click();
        var catalogFocusCalls = CountFocusCalls();
        cut.Find("[aria-label='Close component catalog']").Click();

        Assert.False(navigation.CatalogOpen);
        Assert.True(CountFocusCalls() > catalogFocusCalls);

        cut.Find("[data-testid='outline-trigger']").Click();
        var outlineFocusCalls = CountFocusCalls();
        cut.Find("[aria-label='Close page outline']").Click();

        Assert.False(navigation.OutlineOpen);
        Assert.True(CountFocusCalls() > outlineFocusCalls);
    }

    [Fact]
    public void NavigationInteractionsNeverMutateThemeState()
    {
        var theme = Services.GetRequiredService<ShowcaseState>();
        Services.GetRequiredService<DocumentationPageState>().SetSections([new("overview", "Overview")]);
        var state = new DocumentationNavigationState();
        var cut = Render<DocumentationHeader>(parameters => parameters.Add(x => x.State, state));

        cut.Find("[data-testid='catalog-trigger']").Click();
        cut.Find("[data-testid='outline-trigger']").Click();

        Assert.False(theme.IsDarkMode);
        Assert.Equal(ShadcnDirection.LeftToRight, theme.Direction);
        Assert.False(state.CatalogOpen);
        Assert.True(state.OutlineOpen);

        cut.Find("[data-testid='documentation-theme-toggle']").Click();
        cut.Find("[data-testid='documentation-direction-toggle']").Click();
        Assert.True(theme.IsDarkMode);
        Assert.Equal(ShadcnDirection.RightToLeft, theme.Direction);
    }

    [Fact]
    public void HeaderActionLabelIsAppliedToAValidGroupRole()
    {
        var cut = Render<DocumentationHeader>(parameters => parameters.Add(component => component.State, new DocumentationNavigationState()));
        var actions = cut.Find(".documentation-header__actions");
        Assert.Equal("group", actions.GetAttribute("role"));
        Assert.Equal("Documentation actions", actions.GetAttribute("aria-label"));
    }

    [Fact]
    public void HeaderSponsorLinkUsesCompactOfficialKofiAsset()
    {
        var cut = Render<DocumentationHeader>(parameters => parameters.Add(component => component.State, new DocumentationNavigationState()));
        var sponsor = cut.Find("[data-testid='documentation-kofi-link']");
        var cup = sponsor.QuerySelector(".documentation-kofi__cup");
        var label = sponsor.QuerySelector(".documentation-kofi__label");

        Assert.Equal("https://ko-fi.com/natthapolv", sponsor.GetAttribute("href"));
        Assert.Equal("_blank", sponsor.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", sponsor.GetAttribute("rel"));
        Assert.Equal("Support my work on Ko-fi (opens in a new tab)", sponsor.GetAttribute("aria-label"));
        Assert.NotNull(cup);
        Assert.NotNull(label);
        Assert.Empty(sponsor.QuerySelectorAll(".documentation-kofi__badge"));
        Assert.Contains("images/brand/kofi-cup.png", cup.GetAttribute("src"), StringComparison.Ordinal);
        Assert.Equal("Ko-fi", label.TextContent.Trim());
    }

    [Fact]
    public void HeaderThemeAndDirectionActionsUseAccessibleIconGlyphs()
    {
        var cut = Render<DocumentationHeader>(parameters => parameters.Add(component => component.State, new DocumentationNavigationState()));

        Assert.Equal(2, cut.FindAll(".documentation-icon-action svg[aria-hidden='true']").Count);
        Assert.Equal("Maliev Shadcn Blazor", cut.Find(".documentation-brand").TextContent.Trim());
        Assert.Single(cut.FindAll(".documentation-brand__mark img"));
        Assert.Contains("images/brand/MALIEV_BLACK.svg", cut.Find(".documentation-brand__mark img").GetAttribute("src"), StringComparison.Ordinal);
        Assert.Equal("Use dark theme", cut.Find("[data-testid='documentation-theme-toggle']").GetAttribute("aria-label"));
        Assert.Equal("Use right-to-left direction", cut.Find("[data-testid='documentation-direction-toggle']").GetAttribute("aria-label"));
        var directionIcon = cut.Find("[data-testid='documentation-direction-toggle'] [data-slot='icon']");
        Assert.Equal("tabler", directionIcon.GetAttribute("data-library"));
        Assert.Equal("text-direction-rtl", directionIcon.GetAttribute("data-icon"));
    }

    [Fact]
    public void ThemeStudioMobileActionsUseDistinctLibraryIcons()
    {
        var cut = Render<DocumentationHeader>(parameters => parameters
            .Add(component => component.State, new DocumentationNavigationState())
            .Add(component => component.StudioMode, true));

        var catalogIcon = cut.Find("[data-testid='catalog-trigger'] [data-slot='icon']");
        var settingsIcon = cut.Find("[data-testid='theme-controls-toggle'] [data-slot='icon']");

        Assert.Equal("lucide", catalogIcon.GetAttribute("data-library"));
        Assert.Equal("layout-grid", catalogIcon.GetAttribute("data-icon"));
        Assert.Equal("lucide", settingsIcon.GetAttribute("data-library"));
        Assert.Equal("sliders-horizontal", settingsIcon.GetAttribute("data-icon"));
        Assert.NotEqual(catalogIcon.OuterHtml, settingsIcon.OuterHtml);
    }

    private IRenderedComponent<DocumentationLayout> RenderDocumentationLayout() => Render<DocumentationLayout>(parameters => parameters
        .Add(x => x.Body, (RenderFragment)(builder => builder.AddContent(0, "Documentation body"))));

    private int CountFocusCalls() => JSInterop.Invocations.Count(invocation =>
        invocation.Identifier.Contains("focus", StringComparison.OrdinalIgnoreCase));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DisposeAsyncCore().AsTask().GetAwaiter().GetResult();

        base.Dispose(disposing);
    }
}
