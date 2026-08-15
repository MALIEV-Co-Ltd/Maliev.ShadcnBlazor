using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ComponentDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task SemanticDossierUpdatesRealPreviewCopiesSourceAndListsPublicApi()
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            Permissions = ["clipboard-read", "clipboard-write"]
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/aspect-ratio").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var ratio = page.GetByTestId("control-aspect-ratio");
        await ratio.SelectOptionAsync("1:1");
        await Assertions.Expect(page.Locator("[data-slot='aspect-ratio']")).ToHaveAttributeAsync("style", new Regex("aspect-ratio: 1(?:;|$)"));

        var previewSource = page.Locator("#preview").GetByTestId("copy-source");
        await previewSource.ClickAsync();
        await Assertions.Expect(page.Locator("#preview .component-code__announcement")).ToHaveTextAsync("Source copied to clipboard.");
        var copied = await page.EvaluateAsync<string>("navigator.clipboard.readText()");
        Assert.Contains("<ShadcnAspectRatio", copied, StringComparison.Ordinal);

        var api = page.GetByTestId("component-api");
        Assert.True(await api.Locator("table.component-api__table[data-slot='table']").CountAsync() >= 1);
        await Assertions.Expect(api.GetByTestId("api-row")).ToHaveCountAsync(5);
        await Assertions.Expect(api.Locator("[data-parameter='Ratio']")).ToContainTextAsync("Must be positive and finite.");
        var evidenceRows = page.GetByTestId("evidence-row");
        await Assertions.Expect(evidenceRows).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='true']")).ToHaveCountAsync(6);
        await Assertions.Expect(page.Locator("[data-evidence='integration']")).ToHaveAttributeAsync("data-complete", "false");
        Assert.Empty(errors);
    }

    [Fact]
    public async Task SemanticDossierControlsDriveRealAccessibleDomState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/direction").ToString());
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "rtl");
        await page.GetByTestId("control-direction").SelectOptionAsync("LeftToRight");
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "ltr");
        await page.GetByTestId("control-direction").SelectOptionAsync("Inherited");
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "rtl");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/field").ToString());
        var input = page.Locator("#dossier-field-input");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "dossier-field-help dossier-field-error");
        await page.GetByTestId("control-field-invalid").UncheckAsync();
        await Assertions.Expect(input).Not.ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "dossier-field-help");
        await page.GetByTestId("control-field-disabled").CheckAsync();
        await Assertions.Expect(input).ToBeDisabledAsync();
        await Assertions.Expect(page.Locator("[data-slot='field-set']")).ToHaveAttributeAsync("disabled", string.Empty);
        await page.GetByTestId("control-field-legend-variant").SelectOptionAsync("Label");
        await Assertions.Expect(page.Locator("[data-slot='field-legend']")).ToHaveAttributeAsync("data-variant", "label");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/empty").ToString());
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Create project" })).ToBeVisibleAsync();
        await page.GetByTestId("control-empty-media-variant").SelectOptionAsync("Default");
        await Assertions.Expect(page.Locator("[data-slot='empty-icon']")).ToHaveAttributeAsync("data-variant", "default");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/typography").ToString());
        await page.GetByTestId("control-typeset-tag").SelectOptionAsync("article");
        await page.GetByTestId("control-typography-variant").SelectOptionAsync("H1");
        await Assertions.Expect(page.Locator("article[data-slot='typeset'] h1[data-slot='typography']")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task ShowcasePreviewsExposeWorkingStatefulInteractionAndPolishedLayouts()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/progress").ToString());
        var progress = page.Locator("#preview [data-slot='progress']");
        await Assertions.Expect(progress).ToHaveAttributeAsync("aria-valuenow", "64");
        await page.GetByTestId("control-progress-value").FillAsync("31");
        await page.GetByTestId("control-progress-value").PressAsync("Tab");
        await Assertions.Expect(progress).ToHaveAttributeAsync("aria-valuenow", "31");
        await Assertions.Expect(progress.Locator("[data-slot='progress-value']")).ToHaveTextAsync("31%");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/calendar").ToString());
        var calendar = page.Locator("#preview [data-slot='calendar']");
        await calendar.Locator("[data-day='2026-08-20']").ClickAsync();
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-20']")).ToHaveAttributeAsync("data-selected-single", "true");
        await Assertions.Expect(calendar).ToHaveAttributeAsync("data-selected-date", "2026-08-20");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/button").ToString());
        await Assertions.Expect(page.GetByTestId("button-dossier-preview").Locator("[data-testid^='button-variant-']")).ToHaveCountAsync(6);
        await Assertions.Expect(page.GetByTestId("button-dossier-preview").Locator(".showcase-button-dossier__sizes [data-slot='button']")).ToHaveCountAsync(4);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/checkbox").ToString());
        var checkbox = page.GetByTestId("action-checkbox");
        await checkbox.CheckAsync();
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
        await checkbox.UncheckAsync();
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/switch").ToString());
        var switchInput = page.GetByTestId("action-switch");
        await switchInput.CheckAsync();
        await Assertions.Expect(switchInput).ToHaveAttributeAsync("data-state", "checked");
        await switchInput.UncheckAsync();
        await Assertions.Expect(switchInput).ToHaveAttributeAsync("data-state", "unchecked");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/slider").ToString());
        var sliderThumb = page.GetByTestId("action-slider").Locator("[data-slot='slider-thumb']").First;
        await sliderThumb.FillAsync("35");
        await Assertions.Expect(sliderThumb).ToHaveAttributeAsync("aria-valuenow", "35");
        await Assertions.Expect(page.GetByTestId("slider-dossier-preview").Locator("output")).ToContainTextAsync("35");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/combobox").ToString());
        var combobox = page.GetByTestId("forms-dossier-combobox");
        await combobox.FocusAsync();
        await Assertions.Expect(combobox).ToHaveAttributeAsync("aria-expanded", "true");
        await page.Locator("#preview [data-slot='combobox-item'][data-value='peek']").ClickAsync();
        await Assertions.Expect(combobox).ToHaveValueAsync("PEEK");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/date-picker").ToString());
        var datePicker = page.GetByTestId("forms-dossier-date-picker");
        await datePicker.ClickAsync();
        await datePicker.ClickAsync();
        var datePickerRoot = datePicker.Locator("xpath=ancestor-or-self::*[@data-slot='date-picker'][1]");
        await Assertions.Expect(datePickerRoot.Locator("[data-slot='date-picker-content']")).ToBeVisibleAsync();
        await datePickerRoot.Locator("[data-day='2026-08-20']").ClickAsync();
        await Assertions.Expect(datePicker).ToContainTextAsync("20");
        await datePickerRoot.Locator("[data-slot='date-picker-clear']").ClickAsync();
        await Assertions.Expect(datePickerRoot.Locator("[data-slot='date-picker-clear']")).ToHaveCountAsync(0);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/skeleton").ToString());
        await Assertions.Expect(page.GetByTestId("skeleton-dossier-preview").Locator("[data-slot='skeleton']")).ToHaveCountAsync(12);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/avatar").ToString());
        var avatarDemo = page.GetByTestId("avatar-dossier-preview");
        await Assertions.Expect(avatarDemo.Locator("[data-slot='avatar']")).ToHaveCountAsync(1);
        await Assertions.Expect(avatarDemo.Locator("img")).ToHaveAttributeAsync("src", new System.Text.RegularExpressions.Regex("operator-thai\\.png"));
        await page.GetByTestId("control-avatar-group").CheckAsync();
        await Assertions.Expect(avatarDemo.Locator("[data-slot='avatar']")).ToHaveCountAsync(3);
        var groupedAvatarSources = await avatarDemo.Locator("img").EvaluateAllAsync<string[]>("elements => elements.map(element => element.getAttribute('src'))");
        Assert.Equal(3, groupedAvatarSources.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(groupedAvatarSources, source => source?.Contains("reviewer-thai.png", StringComparison.Ordinal) == true);
        Assert.Contains(groupedAvatarSources, source => source?.Contains("coordinator-thai.png", StringComparison.Ordinal) == true);
        await page.GetByTestId("control-avatar-group").UncheckAsync();
        await page.GetByTestId("control-avatar-failed").CheckAsync();
        await Assertions.Expect(avatarDemo.Locator("[data-slot='avatar-fallback']")).ToHaveAttributeAsync("data-state", "visible");
        Assert.Equal("32px", await avatarDemo.Locator("[data-slot='avatar']").EvaluateAsync<string>("element => getComputedStyle(element).width"));

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/carousel").ToString());
        await Assertions.Expect(page.GetByTestId("control-carousel-reduced")).Not.ToBeCheckedAsync();
        await Assertions.Expect(page.Locator("#preview .showcase-carousel-slide").First).ToContainTextAsync("Laser cell");
        await page.Locator("#preview [data-slot='carousel-next']").ClickAsync();
        await Assertions.Expect(page.Locator("#preview [data-slot='carousel-item'][data-selected='true']")).ToContainTextAsync("First-pass yield");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/toast").ToString());
        var showToast = page.GetByRole(AriaRole.Button, new() { Name = "Show localized toast" });
        await showToast.ClickAsync();
        await showToast.ClickAsync();
        var toasts = page.Locator("#preview [data-slot='toast']");
        await Assertions.Expect(toasts).ToHaveCountAsync(2);
        var firstToast = await toasts.Nth(0).BoundingBoxAsync();
        var secondToast = await toasts.Nth(1).BoundingBoxAsync();
        Assert.NotNull(firstToast);
        Assert.NotNull(secondToast);
        Assert.True(Math.Abs(firstToast!.Y - secondToast!.Y) >= 40, "Toast items should remain visibly stacked in the showcase.");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        var message = page.Locator("#preview [data-slot='message']").First;
        var avatar = message.Locator("[data-slot='message-avatar']");
        Assert.Equal("flex-start", await avatar.EvaluateAsync<string>("element => getComputedStyle(element).alignSelf"));
        await Assertions.Expect(message.Locator("img[data-avatar='operator']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#preview [data-slot='message']").Nth(1).Locator("[data-slot='message-avatar']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#preview img[data-avatar='assistant']")).ToHaveCountAsync(1);
        var footer = message.Locator("[data-slot='message-footer']");
        Assert.Equal("0", await footer.EvaluateAsync<string>("element => getComputedStyle(element).opacity"));
        await message.HoverAsync();
        await Assertions.Expect(footer).ToHaveCSSAsync("opacity", "1");
        await Assertions.Expect(footer.Locator("button")).ToBeVisibleAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message-scroller").ToString());
        var scrollerAvatars = page.Locator("#preview .showcase-scroller-frame img.showcase-message-avatar-image");
        await Assertions.Expect(scrollerAvatars).ToHaveCountAsync(3);
        var scrollerAvatarSources = await scrollerAvatars.EvaluateAllAsync<string[]>("elements => elements.map(element => element.getAttribute('src'))");
        Assert.Equal(3, scrollerAvatarSources.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(scrollerAvatarSources, source => source?.Contains("operator-thai.png", StringComparison.Ordinal) == true);
        Assert.Contains(scrollerAvatarSources, source => source?.Contains("assistant-thai.png", StringComparison.Ordinal) == true);
        Assert.Contains(scrollerAvatarSources, source => source?.Contains("coordinator-thai.png", StringComparison.Ordinal) == true);

        await page.SetViewportSizeAsync(390, 844);
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/data-table").ToString());
        var dataTable = page.Locator("#preview .showcase-data-table");
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);
        await Assertions.Expect(dataTable.Locator(".shadcn-data-table-frame")).ToBeVisibleAsync();
        Assert.Equal("grid", await dataTable.Locator("[data-slot='data-table-toolbar']").EvaluateAsync<string>("element => getComputedStyle(element).display"));
    }

    [Theory]
    [InlineData("input")]
    [InlineData("select")]
    public async Task CertifiedFormsDossiersExposePreviewSourceApiAndCompleteEvidence(string slug)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{slug}").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("planned-component-notice")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByTestId("component-preview")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("copy-source")).ToHaveCountAsync(3);
        await Assertions.Expect(page.GetByTestId("component-api")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("evidence-row")).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='true']")).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='false']")).ToHaveCountAsync(0);
    }
}
