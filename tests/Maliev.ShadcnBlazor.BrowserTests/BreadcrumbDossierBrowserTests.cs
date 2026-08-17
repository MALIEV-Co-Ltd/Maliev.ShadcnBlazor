using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class BreadcrumbDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData(1280, 900, "light", "ltr", false)]
    [InlineData(390, 844, "dark", "rtl", false)]
    [InlineData(800, 800, "light", "ltr", true)]
    public async Task BreadcrumbDossierKeepsResponsiveCurrentPageFocusAndSourceParity(
        int width,
        int height,
        string theme,
        string direction,
        bool forcedColors)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ColorScheme = theme == "dark" ? ColorScheme.Dark : ColorScheme.Light,
            ForcedColors = forcedColors ? ForcedColors.Active : ForcedColors.None,
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/breadcrumb?theme={theme}&dir={direction}").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.EvaluateAsync(
            "direction => { document.documentElement.dir = direction; document.querySelector('.shadcn-scope')?.setAttribute('dir', direction); }",
            direction);

        var preview = page.Locator("#preview .component-preview__canvas");
        var breadcrumb = preview.Locator("[data-slot='breadcrumb']");
        var list = breadcrumb.Locator("[data-slot='breadcrumb-list']");
        var current = breadcrumb.Locator("[data-slot='breadcrumb-page']");
        await Assertions.Expect(breadcrumb).ToHaveAttributeAsync("aria-label", "Quotation workspace breadcrumb");
        await Assertions.Expect(current).ToHaveAttributeAsync("role", "link");
        await Assertions.Expect(current).ToHaveAttributeAsync("aria-disabled", "true");
        await Assertions.Expect(current).ToHaveAttributeAsync("aria-current", "page");
        await Assertions.Expect(list.Locator(":scope > :not(li)")).ToHaveCountAsync(0);
        await Assertions.Expect(breadcrumb.Locator("[data-slot='breadcrumb-ellipsis']")).ToHaveCountAsync(1);

        var firstLink = breadcrumb.Locator("[data-slot='breadcrumb-link']").First;
        await firstLink.FocusAsync();
        await Assertions.Expect(firstLink).ToBeFocusedAsync();
        var focusIndicator = await firstLink.EvaluateAsync<string[]>(
            "element => [getComputedStyle(element).boxShadow, getComputedStyle(element).outlineStyle]");
        Assert.True(focusIndicator[0] != "none" || focusIndicator[1] != "none");
        Assert.True(await preview.EvaluateAsync<bool>("element => element.scrollWidth <= element.clientWidth + 1"));
        Assert.True(await breadcrumb.EvaluateAsync<bool>("element => element.scrollWidth <= element.clientWidth + 1"));
        Assert.True(await firstLink.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).transitionDuration) || 0") < 0.01);

        await page.GetByTestId("control-breadcrumb-ellipsis").UncheckAsync();
        await Assertions.Expect(breadcrumb.Locator("[data-slot='breadcrumb-ellipsis']")).ToHaveCountAsync(0);
        await Assertions.Expect(breadcrumb).ToContainTextAsync("Aster Precision");
        await Assertions.Expect(breadcrumb).ToContainTextAsync("Quotations");
        await Assertions.Expect(list.Locator(":scope > [data-slot='breadcrumb-item']")).ToHaveCountAsync(5);
        await Assertions.Expect(page.Locator("#preview .component-code pre")).ToContainTextAsync("/projects/aster-precision/quotations");
        await Assertions.Expect(page.Locator("#preview .component-code pre")).Not.ToContainTextAsync("ShadcnBreadcrumbEllipsis");
        Assert.True(await preview.EvaluateAsync<bool>("element => element.scrollWidth <= element.clientWidth + 1"));

        if (direction == "rtl")
        {
            var separator = breadcrumb.Locator("[data-slot='breadcrumb-separator'] svg").First;
            Assert.Contains("-1", await separator.EvaluateAsync<string>("element => getComputedStyle(element).scale"), StringComparison.Ordinal);
        }

        var axe = await preview.RunAxe();
        Assert.True(
            axe.Violations is null || !axe.Violations.Any(),
            $"Breadcrumb axe violations: {string.Join("; ", axe.Violations?.Select(violation => violation.Id) ?? [])}");
    }
}
