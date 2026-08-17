using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class InputOtpShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task VerificationCardSupportsPasteKeyboardValidationAndRepeatedActions()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        await context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/input-otp").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var card = page.GetByTestId("input-otp-dossier-preview");
        var input = page.GetByTestId("forms-dossier-input-otp");
        var slots = card.Locator("[data-slot='input-otp-slot']");
        await Assertions.Expect(card.Locator("[data-slot='input-otp-group']")).ToHaveCountAsync(2);
        await Assertions.Expect(slots).ToHaveCountAsync(6);
        await Assertions.Expect(card.Locator("[data-slot='input-otp-separator']")).ToHaveCountAsync(1);

        await page.EvaluateAsync("text => navigator.clipboard.writeText(text)", "12 a3-4567");
        await input.FocusAsync();
        await page.Keyboard.PressAsync("Control+V");
        await Assertions.Expect(input).ToHaveValueAsync("123456");
        await Assertions.Expect(card.Locator("[data-slot='input-otp-root']")).ToHaveAttributeAsync("data-complete", "true");

        await input.PressAsync("ArrowLeft");
        await Assertions.Expect(slots.Nth(5)).ToHaveAttributeAsync("data-active", "true");
        await input.PressAsync("ArrowLeft");
        await Assertions.Expect(slots.Nth(4)).ToHaveAttributeAsync("data-active", "true");
        await input.PressAsync("ArrowRight");
        await Assertions.Expect(slots.Nth(5)).ToHaveAttributeAsync("data-active", "true");

        var verify = page.GetByTestId("input-otp-verify");
        await Assertions.Expect(verify).ToBeEnabledAsync();
        await verify.ClickAsync();
        await Assertions.Expect(page.GetByTestId("input-otp-status")).ToContainTextAsync("Email verified");

        await page.GetByTestId("input-otp-resend").ClickAsync();
        await Assertions.Expect(input).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(page.GetByTestId("input-otp-status")).ToContainTextAsync("A new code was sent");
        await Assertions.Expect(verify).ToBeDisabledAsync();

        await page.GetByTestId("control-input-otp-invalid").CheckAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(page.Locator("#preview .component-code code")).ToContainTextAsync("Invalid=\"true\"");

        await page.GetByTestId("control-input-otp-numeric").UncheckAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("inputmode", "text");
        await Assertions.Expect(input).Not.ToHaveAttributeAsync("data-pattern", "[0-9]");
        await Assertions.Expect(page.Locator("#preview .component-code code")).Not.ToContainTextAsync("Pattern=");
    }

    [Fact]
    public async Task VerificationCardRemainsCenteredResponsiveRtlAndForcedColorLegible()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/input-otp").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var card = page.GetByTestId("input-otp-dossier-preview");
        var input = page.GetByTestId("forms-dossier-input-otp");
        await input.FocusAsync();

        await Assertions.Expect(page.Locator("[data-shadcn-scope]")).ToHaveAttributeAsync("dir", "rtl");
        var canvasBox = await canvas.BoundingBoxAsync();
        var cardBox = await card.BoundingBoxAsync();
        Assert.NotNull(canvasBox);
        Assert.NotNull(cardBox);
        Assert.InRange(Math.Abs((cardBox!.X + cardBox.Width / 2) - (canvasBox!.X + canvasBox.Width / 2)), 0, 2);
        Assert.True(cardBox.Width <= canvasBox.Width);
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);

        var activeSlot = card.Locator("[data-slot='input-otp-slot'][data-active='true']");
        await Assertions.Expect(activeSlot).ToHaveCountAsync(1);
        Assert.Equal("solid", await activeSlot.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
        Assert.True(await page.EvaluateAsync<bool>("matchMedia('(prefers-reduced-motion: reduce)').matches"));
        Assert.InRange(
            await card.Locator("[data-slot='input-otp-root']").EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).transitionDuration)"),
            0,
            .00001);
    }
}
