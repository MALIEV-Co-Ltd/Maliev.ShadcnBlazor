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
        state.ThemeDockOpen = true;

        Assert.False(state.CatalogOpen);
        Assert.True(state.ThemeDockOpen);
        Assert.True(state.CloseDrawers());
        Assert.False(state.ThemeDockOpen);
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
        Assert.Single(cut.FindAll("aside#documentation-theme"));

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
        var cut = RenderDocumentationLayout();
        var navigation = cut.FindComponent<DocumentationHeader>().Instance.State;

        cut.Find("[data-testid='catalog-trigger']").Click();
        var catalogFocusCalls = CountFocusCalls();
        cut.Find("[aria-label='Close component catalog']").Click();

        Assert.False(navigation.CatalogOpen);
        Assert.True(CountFocusCalls() > catalogFocusCalls);

        cut.Find("[data-testid='theme-dock-trigger']").Click();
        var themeFocusCalls = CountFocusCalls();
        cut.Find("[aria-label='Close theme studio']").Click();

        Assert.False(navigation.ThemeDockOpen);
        Assert.True(CountFocusCalls() > themeFocusCalls);
    }

    [Fact]
    public void NavigationInteractionsNeverMutateThemeState()
    {
        var theme = Services.GetRequiredService<ShowcaseState>();
        var state = new DocumentationNavigationState();
        var cut = Render<DocumentationHeader>(parameters => parameters.Add(x => x.State, state));

        cut.Find("[data-testid='catalog-trigger']").Click();
        cut.Find("[data-testid='theme-dock-trigger']").Click();

        Assert.False(theme.IsDarkMode);
        Assert.Equal(ShadcnDirection.LeftToRight, theme.Direction);
        Assert.False(state.CatalogOpen);
        Assert.True(state.ThemeDockOpen);
    }

    [Fact]
    public void HeaderActionLabelIsAppliedToAValidGroupRole()
    {
        var cut = Render<DocumentationHeader>(parameters => parameters.Add(component => component.State, new DocumentationNavigationState()));
        var actions = cut.Find(".documentation-header__actions");
        Assert.Equal("group", actions.GetAttribute("role"));
        Assert.Equal("Documentation panels", actions.GetAttribute("aria-label"));
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
