using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class CommandBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task CommandDossierFiltersLocalizedKeywordsAndSupportsKeyboardAndPointerSelection()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/command").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var preview = page.GetByTestId("command-dossier-preview");
        var input = preview.Locator("[data-slot='command-input']");
        var items = preview.Locator("[data-slot='command-item']");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-label", "Workspace commands");
        await Assertions.Expect(items).ToHaveCountAsync(5);

        await input.FillAsync("ใบสั่งซื้อ");
        var visibleItems = preview.Locator("[data-slot='command-item']:visible");
        await Assertions.Expect(visibleItems).ToHaveCountAsync(1);
        await Assertions.Expect(visibleItems).ToContainTextAsync("Orders");
        await input.PressAsync("Enter");
        await Assertions.Expect(preview.Locator(".showcase-command-dossier__status strong")).ToHaveTextAsync("Orders");

        await input.FillAsync("no matching command");
        await Assertions.Expect(preview.Locator("[data-slot='command-empty']")).ToBeVisibleAsync();
        await Assertions.Expect(preview.Locator("[data-slot='command-group']").First).ToBeHiddenAsync();

        await input.FillAsync(string.Empty);
        var upload = items.Filter(new() { HasText = "Upload drawing" });
        await upload.HoverAsync();
        await Assertions.Expect(upload).ToHaveAttributeAsync("data-selected", "true");
        await upload.ClickAsync();
        await Assertions.Expect(preview.Locator(".showcase-command-dossier__status strong")).ToHaveTextAsync("Upload drawing");

        var axe = await preview.RunAxe();
        var violations = axe.Violations ?? [];
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations.Select(violation =>
            $"{violation.Id}: {violation.Help} [{string.Join(", ", violation.Nodes.Select(node => node.Target.ToString()))}]")));

        await page.GetByTestId("control-command-disabled").CheckAsync();
        var disabledCreate = items.Filter(new() { HasText = "Create quotation" });
        await Assertions.Expect(disabledCreate).ToHaveAttributeAsync("aria-disabled", "true");
        await input.FillAsync("create quotation");
        await Assertions.Expect(preview.Locator("[data-slot='command-item']:visible")).ToHaveCountAsync(1);
        await Assertions.Expect(disabledCreate).ToBeVisibleAsync();
        await Assertions.Expect(preview.Locator("[data-slot='command-empty']")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task CommandDossierRemainsContainedAccessibleAndFocusedOnMobileRtlForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/command?theme=dark&dir=rtl").ToString());
        await page.EvaluateAsync("document.documentElement.dir='rtl'; document.querySelector('.shadcn-scope')?.setAttribute('dir','rtl')");

        var preview = page.GetByTestId("command-dossier-preview");
        await preview.WaitForAsync();
        var input = preview.Locator("[data-slot='command-input']");
        await input.FocusAsync();
        await Assertions.Expect(input).ToBeFocusedAsync();
        await input.PressAsync("End");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-activedescendant", await preview.Locator("[data-slot='command-item']").Last.GetAttributeAsync("id") ?? string.Empty);

        var overflow = await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth-document.documentElement.clientWidth, document.body.scrollWidth-document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
        var transitionDuration = await preview.Locator("[data-slot='command-input-composition']").EvaluateAsync<double>("element => Number.parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(transitionDuration, 0, 0.001);

    }
}
