using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Deque.AxeCore.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DisclosureNavigationBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task EveryDisclosureNavigationComponentPassesNamedAccessibilityRulesInLocalizedRtlState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 900, Height = 900 }, ReducedMotion = ReducedMotion.Reduce, ColorScheme = ColorScheme.Dark });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation?theme=dark&dir=rtl").ToString());
        var root = page.GetByTestId("disclosure-navigation-fixture"); await root.WaitForAsync();
        await page.EvaluateAsync("document.documentElement.dir='rtl'; document.querySelector('.shadcn-scope')?.setAttribute('dir','rtl')");

        await Assertions.Expect(page.Locator("[data-component='accordion'] [data-slot='accordion']")).ToHaveAttributeAsync("aria-label", "Manufacturing questions");
        await Assertions.Expect(page.Locator("[data-component='breadcrumb'] [data-slot='breadcrumb']")).ToHaveAttributeAsync("aria-label", "Project breadcrumb");
        await Assertions.Expect(page.Locator("[data-component='collapsible'] [data-slot='collapsible-trigger']")).ToHaveAttributeAsync("aria-controls", new Regex(".+"));
        await Assertions.Expect(page.Locator("[data-component='navigation-menu'] [data-slot='navigation-menu']")).ToHaveAttributeAsync("aria-label", "Services");
        await Assertions.Expect(page.Locator("[data-component='pagination'] [data-slot='pagination']")).ToHaveAttributeAsync("aria-label", "Quotation pages");
        await Assertions.Expect(page.Locator("[data-component='pagination'] [aria-current='page']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-component='resizable'] [role='separator']")).ToHaveAttributeAsync("aria-label", "Resize queue");
        await Assertions.Expect(page.Locator("[data-component='scroll-area'] [data-slot='scroll-area-viewport']")).ToHaveAttributeAsync("aria-label", "Material catalog");
        await Assertions.Expect(page.Locator("[data-component='sidebar'] [data-slot='sidebar']")).ToHaveAttributeAsync("aria-label", "Workspace");
        await Assertions.Expect(page.Locator("[data-component='tabs'] [role='tablist']")).ToHaveAttributeAsync("aria-label", "Project views");

        var issues = await root.EvaluateAsync<string[]>("""root => { const issues=[]; for(const element of root.querySelectorAll('button,a,input,select,[role=tab],[role=separator]')) { const name=(element.getAttribute('aria-label')||element.textContent||'').trim(); if(!name&&!element.closest('label'))issues.push(`unnamed:${element.dataset.slot||element.tagName}`); if(element.getAttribute('aria-expanded')==='true')for(const id of (element.getAttribute('aria-controls')||'').split(/\s+/).filter(Boolean))if(!document.getElementById(id))issues.push(`missing-control:${id}`); } const ids=[...root.querySelectorAll('[id]')].map(element=>element.id); if(new Set(ids).size!==ids.length)issues.push('duplicate-id'); return issues; }""");
        Assert.Empty(issues);
        await AssertAxeCleanAsync(root, "dark RTL reduced-motion resting state");
    }

    [Theory]
    [InlineData("light", "ltr", false)]
    [InlineData("dark", "rtl", false)]
    [InlineData("light", "ltr", true)]
    public async Task RecognizedAxeEnginePassesOpenClosedSelectedDisabledAndForcedColorStates(string theme, string direction, bool forcedColors)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1000, Height = 900 },
            ColorScheme = theme == "dark" ? ColorScheme.Dark : ColorScheme.Light,
            ForcedColors = forcedColors ? ForcedColors.Active : ForcedColors.None,
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, $"/components/disclosure-and-navigation?theme={theme}&dir={direction}").ToString());
        var root = page.GetByTestId("disclosure-navigation-fixture");
        await root.WaitForAsync();
        await page.EvaluateAsync("direction => { document.documentElement.dir=direction; document.querySelector('.shadcn-scope')?.setAttribute('dir',direction); }", direction);

        await AssertAxeCleanAsync(root, $"{theme} {direction} resting state forced-colors={forcedColors}");
        await page.Locator("[data-component='accordion'] [data-slot='accordion-trigger']").Nth(1).ClickAsync();
        await page.Locator("[data-component='collapsible'] [data-slot='collapsible-trigger']").ClickAsync();
        await page.Locator("[data-component='navigation-menu'] [data-slot='navigation-menu-trigger']").ClickAsync();
        await page.Locator("[data-component='pagination'] input[type='number']").FillAsync("1");
        await page.Locator("[data-component='pagination'] input[type='number']").PressAsync("Tab");
        await page.Locator("[data-component='tabs'] [role='tab']").Nth(1).ClickAsync();
        await AssertAxeCleanAsync(root, $"{theme} {direction} changed disclosure/selection/disabled state forced-colors={forcedColors}");
    }

    [Fact]
    public async Task EveryStateExposesNameRoleValueRelationshipsAndCorrectHiddenDisabledSemantics()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1000, Height = 900 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation").ToString());

        var accordionTrigger = page.Locator("[data-component='accordion'] [data-slot='accordion-trigger']").First;
        var accordionContent = page.Locator("[data-component='accordion'] [data-slot='accordion-content']").First;
        await AssertControlsRelationshipAsync(accordionTrigger, accordionContent, expanded: true);
        await accordionTrigger.ClickAsync();
        await AssertControlsRelationshipAsync(accordionTrigger, accordionContent, expanded: false);

        var collapsibleTrigger = page.Locator("[data-component='collapsible'] [data-slot='collapsible-trigger']");
        var collapsibleContent = page.Locator("[data-component='collapsible'] [data-slot='collapsible-content']");
        await AssertControlsRelationshipAsync(collapsibleTrigger, collapsibleContent, expanded: true);
        await collapsibleTrigger.ClickAsync();
        await AssertControlsRelationshipAsync(collapsibleTrigger, collapsibleContent, expanded: false);

        await Assertions.Expect(page.Locator("[data-component='breadcrumb'] [aria-current='page']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-component='pagination'] [aria-current='page']")).ToHaveCountAsync(1);
        await page.Locator("[data-component='pagination'] input[type='number']").FillAsync("1");
        await page.Locator("[data-component='pagination'] input[type='number']").PressAsync("Tab");
        await Assertions.Expect(page.Locator("[data-component='pagination'] [data-slot='pagination-previous']")).ToBeDisabledAsync();

        var separator = page.Locator("[data-component='resizable'] [role='separator']");
        await Assertions.Expect(separator).ToHaveAttributeAsync("aria-orientation", "vertical");
        Assert.True(double.Parse((await separator.GetAttributeAsync("aria-valuemin"))!, System.Globalization.CultureInfo.InvariantCulture) < double.Parse((await separator.GetAttributeAsync("aria-valuemax"))!, System.Globalization.CultureInfo.InvariantCulture));
        await Assertions.Expect(page.Locator("[data-component='scroll-area'] [data-slot='scroll-area-viewport']")).ToHaveAttributeAsync("tabindex", "0");

        var tabs = page.Locator("[data-component='tabs'] [role='tab']");
        await Assertions.Expect(tabs.First).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "false");
        var inactivePanelId = await tabs.Nth(1).GetAttributeAsync("aria-controls");
        await Assertions.Expect(page.Locator($"#{inactivePanelId}")).ToBeHiddenAsync();
        await tabs.Nth(1).ClickAsync();
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(page.Locator($"#{inactivePanelId}")).ToBeVisibleAsync();

        var menuTrigger = page.Locator("[data-component='navigation-menu'] [data-slot='navigation-menu-trigger']");
        await menuTrigger.ClickAsync();
        await Assertions.Expect(menuTrigger).ToHaveAttributeAsync("aria-expanded", "false");
        await menuTrigger.ClickAsync();
        await Assertions.Expect(menuTrigger).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(menuTrigger).ToBeFocusedAsync();
    }

    [Fact]
    public async Task ComponentsRemainOperableAtTwoHundredPercentZoomWithReducedMotion()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 640, Height = 900 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation").ToString());
        await page.EvaluateAsync("document.documentElement.style.zoom='2'");
        foreach (var slug in new[] { "accordion", "breadcrumb", "collapsible", "navigation-menu", "pagination", "resizable", "scroll-area", "sidebar", "tabs" })
        {
            var component = page.Locator($"[data-component='{slug}']");
            await Assertions.Expect(component).ToBeVisibleAsync();
            Assert.True(await component.EvaluateAsync<bool>("element => element.getBoundingClientRect().width > 0 && element.getBoundingClientRect().height > 0"), slug);
        }
        await page.Locator("[data-component='accordion'] [data-slot='accordion-trigger']").First.ClickAsync();
        await page.Locator("[data-component='tabs'] [role='tab']").Nth(1).ClickAsync();
        await Assertions.Expect(page.Locator("[data-component='tabs'] [role='tab']").Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Fact]
    public async Task EveryDisclosureNavigationComponentHasNamedRealBrowserInteractionOrSemanticJourney()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 900, Height = 900 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation").ToString());
        await page.GetByTestId("disclosure-navigation-fixture").WaitForAsync();

        var collapsible = page.Locator("[data-component='collapsible'] [data-slot='collapsible-trigger']");
        var collapsibleBefore = await collapsible.GetAttributeAsync("aria-expanded");
        await collapsible.ClickAsync(); await Assertions.Expect(collapsible).Not.ToHaveAttributeAsync("aria-expanded", collapsibleBefore!);
        await Assertions.Expect(page.Locator("[data-component='breadcrumb'] [aria-current='page']")).ToHaveTextAsync("Quotation");
        var currentPage = page.Locator("[data-component='pagination'] [aria-current='page']");
        var pageBefore = await currentPage.TextContentAsync();
        await page.Locator("[data-component='pagination'] [data-slot='pagination-next']").ClickAsync();
        await Assertions.Expect(currentPage).Not.ToHaveTextAsync(pageBefore!);
        var accordionTrigger = page.Locator("[data-component='accordion'] [data-slot='accordion-trigger']").First;
        var accordionBefore = await accordionTrigger.GetAttributeAsync("aria-expanded");
        await accordionTrigger.ClickAsync();
        await Assertions.Expect(accordionTrigger).Not.ToHaveAttributeAsync("aria-expanded", accordionBefore!);
        var menuTrigger = page.Locator("[data-component='navigation-menu'] [data-slot='navigation-menu-trigger']");
        var menuBefore = await menuTrigger.GetAttributeAsync("aria-expanded");
        await menuTrigger.ClickAsync();
        await Assertions.Expect(menuTrigger).Not.ToHaveAttributeAsync("aria-expanded", menuBefore!);
        await page.Locator("[data-component='tabs'] [role='tab']").Nth(1).ClickAsync();
        await Assertions.Expect(page.Locator("[data-component='tabs'] [role='tab']").Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Fact]
    public async Task AccordionAndTabsUseNativeActivationWithSelectiveRovingKeyGuards()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 900, Height = 800 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation").ToString());

        var accordion = page.Locator("[data-component='accordion'] [data-slot='accordion-trigger']");
        await accordion.First.FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(accordion.Nth(1)).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(accordion.Nth(1)).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Keyboard.PressAsync("Space");
        await Assertions.Expect(accordion.Nth(1)).ToHaveAttributeAsync("aria-expanded", "false");

        var tabs = page.Locator("[data-component='tabs'] [role='tab']");
        await tabs.First.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(tabs.Nth(1)).ToBeFocusedAsync();
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Fact]
    public async Task AccordionDossierShowsRichContentAndDirectDisclosureInteraction()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/accordion").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var accordion = page.Locator("#preview [data-slot='accordion']");
        var triggers = accordion.Locator("[data-slot='accordion-trigger']");
        var contents = accordion.Locator("[data-slot='accordion-content']");
        await Assertions.Expect(triggers).ToHaveCountAsync(3);
        await Assertions.Expect(contents.First).ToContainTextAsync("Express delivery");
        await Assertions.Expect(contents.First).ToBeVisibleAsync();

        await triggers.First.ClickAsync();
        await Assertions.Expect(contents.First).ToBeHiddenAsync();
        await triggers.Nth(1).ClickAsync();
        await Assertions.Expect(contents.Nth(1)).ToBeVisibleAsync();
        await Assertions.Expect(contents.Nth(1)).ToContainTextAsync("Revision notes");

        await page.GetByTestId("control-accordion-multiple").CheckAsync();
        await Assertions.Expect(contents.First).ToBeVisibleAsync();
        await Assertions.Expect(contents.Nth(1)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PaginationDossierUsesAConfigurableInteractiveWindowAndMatchingSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/pagination").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var numericPages = canvas.Locator("[data-slot='pagination-link'][data-page]");
        await Assertions.Expect(numericPages).ToHaveCountAsync(5);
        await Assertions.Expect(canvas.Locator("[data-slot='pagination-ellipsis']")).ToHaveCountAsync(1);

        var visibleCount = page.GetByTestId("control-pagination-visible");
        await visibleCount.FillAsync("7");
        await visibleCount.PressAsync("Tab");
        await Assertions.Expect(numericPages).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("VisiblePageCount=\"7\"");

        await canvas.Locator("[data-page='6']").ClickAsync();
        await Assertions.Expect(canvas.Locator("[data-page='6']")).ToHaveAttributeAsync("aria-current", "page");
        await Assertions.Expect(canvas.GetByText("Page 6 of 12")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DisclosureTabsAndNavigationMenuHaveRealKeyboardFocusAndState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 900, Height = 800 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation").ToString());
        var accordion = page.Locator("[data-component='accordion'] [data-slot='accordion-trigger']");
        await accordion.First.FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(accordion.Nth(1)).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(accordion.Nth(1)).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Keyboard.PressAsync("Space");
        await Assertions.Expect(accordion.Nth(1)).ToHaveAttributeAsync("aria-expanded", "false");

        var tabs = page.Locator("[data-component='tabs'] [role='tab']");
        await tabs.First.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(tabs.Nth(1)).ToBeFocusedAsync();
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");

        var trigger = page.Locator("[data-component='navigation-menu'] [data-slot='navigation-menu-trigger']");
        await trigger.ClickAsync();
        await Assertions.Expect(page.Locator("[data-component='navigation-menu'] [data-slot='navigation-menu-content']")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task ResizableScrollAreaResponsiveSidebarAndForcedColorsRemainOperable()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, ReducedMotion = ReducedMotion.Reduce, ForcedColors = ForcedColors.Active });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation").ToString());

        var handle = page.Locator("[data-component='resizable'] [data-slot='resizable-handle']");
        await handle.FocusAsync();
        var before = await handle.GetAttributeAsync("aria-valuenow");
        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(handle).Not.ToHaveAttributeAsync("aria-valuenow", before!);
        var focusIndicator = await handle.EvaluateAsync<string[]>("element => [getComputedStyle(element).boxShadow, getComputedStyle(element).outlineStyle]");
        Assert.True(focusIndicator[0] != "none" || focusIndicator[1] != "none");

        var viewport = page.Locator("[data-component='scroll-area'] [data-slot='scroll-area-viewport']");
        await viewport.FocusAsync();
        await viewport.EvaluateAsync("element => element.scrollTop = 120");
        Assert.True(await viewport.EvaluateAsync<double>("element => element.scrollTop") > 0);

        await Assertions.Expect(page.Locator("[data-component='sidebar'] [data-slot='sidebar-wrapper']")).ToHaveAttributeAsync("data-device", "mobile");
        await page.Locator("[data-component='sidebar'] [data-slot='sidebar-trigger']").ClickAsync();
        var mobile = page.Locator("[data-component='sidebar'] aside[data-mobile='true']");
        await Assertions.Expect(mobile).ToHaveAttributeAsync("aria-modal", "true");
        var inset = page.Locator("[data-component='sidebar'] [data-slot='sidebar-inset']");
        Assert.True(await inset.EvaluateAsync<bool>("element => element.inert"));
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(mobile).ToHaveCountAsync(0);
        await page.WaitForFunctionAsync("element => !element.inert", await inset.ElementHandleAsync());
        Assert.False(await inset.EvaluateAsync<bool>("element => element.inert"));
    }

    [Fact]
    public async Task RtlDirectionReversesLogicalTabsAndKeepsThaiAccessibleNames()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 768, Height = 800 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/disclosure-and-navigation?dir=rtl").ToString());
        await page.EvaluateAsync("document.documentElement.dir='rtl'; document.querySelector('.shadcn-scope')?.setAttribute('dir','rtl')");
        var tabs = page.Locator("[data-component='tabs'] [role='tab']");
        await tabs.First.FocusAsync();
        await page.Keyboard.PressAsync("ArrowLeft");
        await Assertions.Expect(tabs.Nth(1)).ToBeFocusedAsync();
        Assert.Contains("ภาษาไทย", await page.Locator("section[dir='rtl']").InnerTextAsync(), StringComparison.Ordinal);
    }

    private static async Task AssertAxeCleanAsync(ILocator locator, string state)
    {
        var axe = await locator.RunAxe();
        var violations = axe.Violations ?? [];
        Assert.True(!violations.Any(), $"Axe violations in {state}: {string.Join("; ", violations.Select(violation => $"{violation.Id} [{string.Join(", ", violation.Nodes.Select(node => node.Target.ToString()))}]"))}");
    }

    private static async Task AssertControlsRelationshipAsync(ILocator trigger, ILocator content, bool expanded)
    {
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", expanded ? "true" : "false");
        var controlledId = await trigger.GetAttributeAsync("aria-controls");
        Assert.False(string.IsNullOrWhiteSpace(controlledId));
        Assert.Equal(controlledId, await content.GetAttributeAsync("id"));
        if (expanded) await Assertions.Expect(content).ToBeVisibleAsync();
        else await Assertions.Expect(content).ToBeHiddenAsync();
    }
}
