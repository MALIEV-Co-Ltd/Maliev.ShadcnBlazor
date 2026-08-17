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
    public async Task CheckboxDossierSupportsDirectPointerKeyboardAndAllDocumentedStates()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/checkbox?dir=rtl").ToString());
        var preview = page.GetByTestId("checkbox-dossier-preview");
        await preview.WaitForAsync();
        await Assertions.Expect(preview.Locator("[data-slot='checkbox']")).ToHaveCountAsync(6);

        var terms = page.GetByTestId("action-checkbox");
        await Assertions.Expect(terms).ToHaveAttributeAsync("aria-checked", "false");
        await terms.CheckAsync();
        await Assertions.Expect(terms).ToHaveAttributeAsync("data-state", "checked");
        await terms.UncheckAsync();
        await Assertions.Expect(terms).ToHaveAttributeAsync("data-state", "unchecked");

        var updates = page.GetByTestId("checkbox-updates");
        await updates.FocusAsync();
        await updates.PressAsync("Space");
        await Assertions.Expect(updates).ToHaveAttributeAsync("aria-checked", "false");
        await Assertions.Expect(page.GetByTestId("checkbox-indeterminate")).ToHaveAttributeAsync("aria-checked", "mixed");
        await Assertions.Expect(page.GetByTestId("checkbox-readonly")).ToHaveAttributeAsync("aria-readonly", "true");
        await Assertions.Expect(page.GetByTestId("checkbox-invalid")).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(page.GetByTestId("checkbox-disabled")).ToBeDisabledAsync();

        var bounds = await preview.BoundingBoxAsync();
        Assert.NotNull(bounds);
        Assert.True(bounds!.X >= 0 && bounds.X + bounds.Width <= 390, $"Checkbox dossier overflows mobile viewport: {bounds.X}, {bounds.Width}");
    }

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
        var frame = page.Locator(".showcase-aspect-ratio-demo");
        var landscapeBox = await frame.BoundingBoxAsync();
        Assert.NotNull(landscapeBox);
        await Assertions.Expect(frame.Locator("img[alt='Engineering workspace reference']")).ToBeVisibleAsync();

        await ratio.SelectOptionAsync("1:1");
        await Assertions.Expect(page.Locator("[data-slot='aspect-ratio']")).ToHaveAttributeAsync("style", new Regex("aspect-ratio: 1(?:;|$)"));
        await Assertions.Expect(frame).ToHaveClassAsync(new Regex("showcase-aspect-ratio-demo--1-1"));
        var squareBox = await frame.BoundingBoxAsync();
        Assert.NotNull(squareBox);
        Assert.True(squareBox.Width < landscapeBox.Width, $"Expected the 1:1 frame ({squareBox.Width}px) to narrow from the 16:9 frame ({landscapeBox.Width}px). ");

        await ratio.SelectOptionAsync("4:3");
        await Assertions.Expect(page.Locator("[data-slot='aspect-ratio']")).ToHaveAttributeAsync("style", new Regex("aspect-ratio: 1.3333333333333333(?:;|$)"));

        var previewSource = page.Locator("#preview").GetByTestId("copy-source");
        await previewSource.ClickAsync();
        await Assertions.Expect(page.Locator("#preview .component-code__announcement")).ToHaveTextAsync("Source copied to clipboard.");
        var copied = await page.EvaluateAsync<string>("navigator.clipboard.readText()");
        Assert.Contains("<ShadcnAspectRatio", copied, StringComparison.Ordinal);
        Assert.Contains("Ratio=\"@(4d / 3d)\"", copied, StringComparison.Ordinal);
        Assert.Contains("showcase-aspect-ratio-demo--4-3", copied, StringComparison.Ordinal);

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
        await page.GetByTestId("control-direction").SelectOptionAsync("Left to right (LTR)");
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "ltr");
        await page.GetByTestId("control-direction").SelectOptionAsync("Inherited (RTL)");
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "rtl");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/field").ToString());
        var fieldPreview = page.GetByTestId("field-dossier-preview");
        var input = fieldPreview.Locator("#field-card-number");
        await Assertions.Expect(input).Not.ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "field-card-number-help");
        await page.GetByTestId("control-field-invalid").CheckAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "field-card-number-help field-card-number-error");
        await Assertions.Expect(fieldPreview.GetByRole(AriaRole.Alert)).ToContainTextAsync("Check the card number");
        await page.GetByTestId("control-field-invalid").UncheckAsync();
        await Assertions.Expect(input).Not.ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "field-card-number-help");
        await page.GetByTestId("control-field-disabled").CheckAsync();
        await Assertions.Expect(input).ToBeDisabledAsync();
        await Assertions.Expect(fieldPreview.Locator("[data-slot='field-set']")).ToHaveAttributeAsync("disabled", string.Empty);
        await page.GetByTestId("control-field-legend-variant").SelectOptionAsync("Label");
        await Assertions.Expect(fieldPreview.Locator("[data-slot='field-legend']")).ToHaveAttributeAsync("data-variant", "label");

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
    public async Task DirectionDossierIsInteractiveResponsiveAndForcedColorSafe()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/direction?theme=dark&dir=rtl").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var preview = page.GetByTestId("direction-example");
        await Assertions.Expect(preview).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(preview).ToHaveAttributeAsync("lang", "ar");

        await preview.Locator("#direction-email").FillAsync("qa@example.com");
        await preview.Locator("#direction-workspace").FillAsync("Factory QA");
        await preview.GetByRole(AriaRole.Button).FocusAsync();
        await Assertions.Expect(preview.GetByRole(AriaRole.Button)).ToBeFocusedAsync();

        await page.GetByTestId("control-direction").SelectOptionAsync("Left to right (LTR)");
        await Assertions.Expect(preview).ToHaveAttributeAsync("dir", "ltr");
        await Assertions.Expect(preview).ToHaveAttributeAsync("lang", "en");
        await Assertions.Expect(page.Locator("#preview pre")).ToContainTextAsync("ShadcnDirection.LeftToRight");
        await Assertions.Expect(page.Locator("#preview pre")).ToContainTextAsync("Create a production workspace");

        var hasHorizontalOverflow = await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasHorizontalOverflow);
    }

    [Fact]
    public async Task ItemDossierUsesRealMediaResponsiveCompositionAndSynchronizedSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/item").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var dossier = canvas.Locator(".showcase-item-dossier");
        await Assertions.Expect(dossier.Locator("[data-slot='item-group'] > [role='listitem']")).ToHaveCountAsync(3);
        await Assertions.Expect(dossier.Locator("[data-slot='item-media'] svg[aria-hidden='true']")).ToHaveCountAsync(3);
        await Assertions.Expect(dossier.Locator("[data-slot='item-actions'] [data-slot='badge']")).ToHaveCountAsync(3);

        var canvasBox = await canvas.BoundingBoxAsync();
        var dossierBox = await dossier.BoundingBoxAsync();
        Assert.NotNull(canvasBox);
        Assert.NotNull(dossierBox);
        Assert.InRange(Math.Abs((dossierBox!.X + (dossierBox.Width / 2)) - (canvasBox!.X + (canvasBox.Width / 2))), 0, 2);

        await page.GetByTestId("control-item-variant").SelectOptionAsync("Muted");
        await page.GetByTestId("control-item-size").SelectOptionAsync("Small");
        await page.GetByTestId("control-item-media-variant").SelectOptionAsync("Image");
        await page.GetByTestId("control-item-link").CheckAsync();

        var links = dossier.Locator("a[data-slot='item']");
        await Assertions.Expect(links).ToHaveCountAsync(3);
        await Assertions.Expect(links.First).ToHaveAttributeAsync("data-variant", "muted");
        await Assertions.Expect(links.First).ToHaveAttributeAsync("data-size", "sm");
        var images = dossier.Locator("[data-slot='item-media'][data-variant='image'] img[alt]");
        await Assertions.Expect(images).ToHaveCountAsync(3);
        await images.First.EvaluateAsync("image => image.decode()");
        Assert.True(await images.First.EvaluateAsync<bool>("image => image.complete && image.naturalWidth > 0"));

        var source = page.Locator("#preview .component-code pre").First;
        await Assertions.Expect(source).ToContainTextAsync("Variant=\"ShadcnItemVariant.Muted\"");
        await Assertions.Expect(source).ToContainTextAsync("Size=\"ShadcnItemSize.Small\"");
        await Assertions.Expect(source).ToContainTextAsync("Href=\"#item-workspace-plan\"");
        await Assertions.Expect(source).ToContainTextAsync("images/attachments/workspace-plan.png");

        await page.SetViewportSizeAsync(390, 844);
        await Assertions.Expect(dossier).ToBeVisibleAsync();
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);
    }

    [Fact]
    public async Task LabelDossierUsesThePackageInputAndSynchronizesInteractionAndSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/label").ToString());
        var labelDossier = page.GetByTestId("label-dossier");
        var labelInput = page.GetByTestId("label-project-input");
        await Assertions.Expect(labelDossier).ToBeVisibleAsync();
        await Assertions.Expect(labelInput).ToHaveAttributeAsync("data-slot", "input");
        await Assertions.Expect(page.Locator("label[for='dossier-label-input']")).ToContainTextAsync("Project name");
        await labelInput.FillAsync("Fixture inspection · Revision D");
        await Assertions.Expect(page.GetByTestId("label-project-preview")).ToHaveTextAsync("Fixture inspection · Revision D");
        await page.GetByTestId("control-label-disabled").CheckAsync();
        await Assertions.Expect(labelInput).ToBeDisabledAsync();
        await Assertions.Expect(labelDossier).ToHaveAttributeAsync("data-disabled", "true");
        await Assertions.Expect(page.Locator("#preview .component-code")).ToContainTextAsync("Disabled=\"true\"");
    }

    [Fact]
    public async Task TypographyDossierKeepsReadableLogicalRhythmAcrossInteractiveAndAccessibleModes()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/typography").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("documentation-theme-toggle").EvaluateAsync("element => element.click()");
        await page.GetByTestId("documentation-direction-toggle").EvaluateAsync("element => element.click()");

        var typeset = page.Locator("#preview [data-slot='typeset']");
        var list = typeset.Locator(".shadcn-typography--unordered-list");
        var inlineCode = typeset.Locator(".shadcn-typography--inline-code");
        await Assertions.Expect(typeset).ToHaveCSSAsync("max-inline-size", "min(100%, 768px)");
        await Assertions.Expect(list).ToHaveCSSAsync("margin-block-start", "8px");
        await Assertions.Expect(list).ToHaveCSSAsync("padding-inline-start", "16px");
        await Assertions.Expect(inlineCode).ToHaveCSSAsync("overflow-wrap", "anywhere");
        await Assertions.Expect(typeset.Locator(".shadcn-typography--h1")).ToHaveCSSAsync("font-size", "36px");
        await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("dir", "rtl");

        var gaps = await typeset.EvaluateAsync<double[]>("""
            element => {
                const canvas = element.closest('[data-testid="component-preview-canvas"]');
                const content = element.getBoundingClientRect();
                const bounds = canvas.getBoundingClientRect();
                return [content.left - bounds.left, bounds.right - content.right];
            }
            """);
        Assert.InRange(Math.Abs(gaps[0] - gaps[1]), 0, 1);

        await page.GetByTestId("control-typography-variant").SelectOptionAsync("OrderedList");
        await page.GetByTestId("control-typeset-tag").SelectOptionAsync("article");
        await page.GetByTestId("control-typeset-size").SelectOptionAsync("1.125rem");
        await page.GetByTestId("control-typeset-leading").SelectOptionAsync("1.8");
        await page.GetByTestId("control-typeset-flow").SelectOptionAsync("1.5rem");
        await page.GetByTestId("control-typeset-max-width").SelectOptionAsync("32rem");

        var source = page.Locator("#preview [data-slot='code-block'] pre");
        await Assertions.Expect(source).ToContainTextAsync("<ShadcnTypeset Tag=\"article\" Size=\"1.125rem\" Leading=\"1.8\" Flow=\"1.5rem\" MaxWidth=\"32rem\">");
        await Assertions.Expect(source).ToContainTextAsync("Variant=\"ShadcnTypographyVariant.OrderedList\"");
        await Assertions.Expect(source).ToContainTextAsync("<li>Confirm the drawing revision</li>");

        var select = page.GetByTestId("control-typography-variant");
        await select.FocusAsync();
        Assert.NotEqual("none", await select.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
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
        await Assertions.Expect(progress.Locator("[data-slot='progress-indicator']")).ToHaveAttributeAsync("style", new Regex("--shadcn-progress-ratio:\\s*0\\.31"));
        await Assertions.Expect(page.Locator("#preview .showcase-progress-demo")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#preview .showcase-progress-demo__summary small")).ToHaveTextAsync("8.9 MB of 28.8 MB");

        await page.GetByTestId("control-progress-indeterminate").CheckAsync();
        await Assertions.Expect(progress).ToHaveAttributeAsync("data-state", "indeterminate");
        await Assertions.Expect(progress).Not.ToHaveAttributeAsync("aria-valuenow", "31");
        await Assertions.Expect(progress).ToHaveAttributeAsync("aria-label", "Preparing upload");
        await Assertions.Expect(page.Locator("#preview .showcase-progress-demo__summary small")).ToHaveTextAsync("Preparing secure upload…");
        Assert.Equal("none", await progress.Locator("[data-slot='progress-indicator']")
            .EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/calendar").ToString());
        var calendar = page.Locator("#preview [data-slot='calendar']");
        await calendar.Locator("[data-day='2026-08-20']").ClickAsync();
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-20']")).ToHaveAttributeAsync("data-selected-single", "true");
        await Assertions.Expect(calendar).ToHaveAttributeAsync("data-selected-date", "2026-08-20");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/button").ToString());
        await Assertions.Expect(page.GetByTestId("button-dossier-preview").Locator("[data-testid^='button-variant-']")).ToHaveCountAsync(6);
        await Assertions.Expect(page.GetByTestId("button-dossier-preview").Locator(".showcase-button-dossier__sizes [data-slot='button']")).ToHaveCountAsync(4);
        await Assertions.Expect(page.GetByTestId("button-dossier-preview").Locator(".showcase-button-dossier__icon-sizes [data-slot='button']")).ToHaveCountAsync(4);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/checkbox").ToString());
        var checkboxPreview = page.GetByTestId("checkbox-dossier-preview");
        await Assertions.Expect(checkboxPreview.Locator("[data-slot='checkbox']")).ToHaveCountAsync(6);
        var checkbox = page.GetByTestId("action-checkbox");
        await checkbox.CheckAsync();
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
        await checkbox.UncheckAsync();
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
        var updates = page.GetByTestId("checkbox-updates");
        await updates.FocusAsync();
        await updates.PressAsync("Space");
        await Assertions.Expect(updates).ToHaveAttributeAsync("aria-checked", "false");
        await Assertions.Expect(page.GetByTestId("checkbox-indeterminate")).ToHaveAttributeAsync("aria-checked", "mixed");
        await Assertions.Expect(page.GetByTestId("checkbox-readonly")).ToHaveAttributeAsync("aria-readonly", "true");
        await Assertions.Expect(page.GetByTestId("checkbox-invalid")).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(page.GetByTestId("checkbox-disabled")).ToBeDisabledAsync();

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
        await Assertions.Expect(avatarDemo.Locator("[data-slot='avatar']")).ToHaveCountAsync(4);
        await Assertions.Expect(avatarDemo.Locator("img[src*='operator-thai.png']")).ToHaveCountAsync(1);
        var operatorPortrait = avatarDemo.Locator("img[src*='operator-thai.png']");
        Assert.True(await operatorPortrait.EvaluateAsync<bool>("image => image.complete && image.naturalWidth > 0"));
        Assert.Equal("cover", await operatorPortrait.EvaluateAsync<string>("image => getComputedStyle(image).objectFit"));
        var portraitFrame = await operatorPortrait.EvaluateAsync<double[]>("image => { const rect = image.getBoundingClientRect(); return [rect.width, rect.height]; }");
        Assert.InRange(Math.Abs(portraitFrame[0] - portraitFrame[1]), 0, 0.5);
        await page.GetByTestId("control-avatar-badge").CheckAsync();
        var onlineBadge = avatarDemo.Locator("[data-slot='avatar-badge']").First;
        await Assertions.Expect(onlineBadge).ToHaveAttributeAsync("aria-label", "Online");
        Assert.True(await onlineBadge.EvaluateAsync<bool>("element => { const color = getComputedStyle(element).backgroundColor; if (color.startsWith('oklch')) return color.includes('145'); const channels = color.match(/[\\d.]+/g)?.slice(0, 3).map(Number); return channels?.length === 3 && channels[1] > channels[0] && channels[1] > channels[2]; }"));
        await page.GetByTestId("control-avatar-group").CheckAsync();
        await Assertions.Expect(avatarDemo.Locator("[data-testid='avatar-group-preview'] [data-slot='avatar']")).ToHaveCountAsync(3);
        var groupedAvatarSources = await avatarDemo.Locator("img").EvaluateAllAsync<string[]>("elements => elements.map(element => element.getAttribute('src'))");
        Assert.Equal(4, groupedAvatarSources.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(groupedAvatarSources, source => source?.Contains("reviewer-thai.png", StringComparison.Ordinal) == true);
        Assert.Contains(groupedAvatarSources, source => source?.Contains("assistant-thai.png", StringComparison.Ordinal) == true);
        await page.GetByTestId("control-avatar-group").UncheckAsync();
        await page.GetByTestId("control-avatar-failed").CheckAsync();
        Assert.True(await avatarDemo.Locator("[data-slot='avatar-fallback'][data-state='visible']").CountAsync() >= 1);
        await Assertions.Expect(avatarDemo.Locator("[data-slot='avatar-image'][data-state='error']")).ToHaveCountAsync(1);
        Assert.Equal("32px", await avatarDemo.Locator("[data-slot='avatar']").First.EvaluateAsync<string>("element => getComputedStyle(element).width"));

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
        Assert.Equal("end", await avatar.EvaluateAsync<string>("element => getComputedStyle(element).alignSelf"));
        await Assertions.Expect(message.Locator("img[data-avatar='operator']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#preview [data-slot='message']").Nth(1).Locator("[data-slot='message-avatar']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("#preview img[data-avatar='assistant']")).ToHaveCountAsync(1);
        var footer = message.Locator("[data-slot='message-footer']");
        Assert.Equal("0", await footer.EvaluateAsync<string>("element => getComputedStyle(element).opacity"));
        await message.HoverAsync();
        await Assertions.Expect(footer).ToHaveCSSAsync("opacity", "1");
        await Assertions.Expect(footer.Locator("button").First).ToBeVisibleAsync();
        await page.GetByTestId("control-message-footer-always").CheckAsync();
        await page.Mouse.MoveAsync(1, 1);
        await Assertions.Expect(footer).ToHaveAttributeAsync("data-visibility", "always");
        await Assertions.Expect(footer).ToHaveCSSAsync("opacity", "1");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message-scroller").ToString());
        var scrollerAvatars = page.Locator("#preview .showcase-scroller-frame img.showcase-message-avatar-image");
        await Assertions.Expect(scrollerAvatars).ToHaveCountAsync(2);
        var scrollerAvatarSources = await scrollerAvatars.EvaluateAllAsync<string[]>("elements => elements.map(element => element.getAttribute('src'))");
        Assert.Equal(2, scrollerAvatarSources.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(scrollerAvatarSources, source => source?.Contains("operator-thai.png", StringComparison.Ordinal) == true);
        Assert.Contains(scrollerAvatarSources, source => source?.Contains("assistant-thai.png", StringComparison.Ordinal) == true);
        await Assertions.Expect(page.GetByTestId("scroller-send")).ToBeEnabledAsync();
        await Assertions.Expect(page.Locator(".showcase-scroller-composer input")).ToHaveValueAsync("อธิบายวิธีติดตามข้อความล่าสุดให้หน่อย");
        await page.GetByTestId("scroller-send").ClickAsync();
        await Assertions.Expect(page.GetByTestId("scroller-streaming")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".showcase-scroller-frame [data-slot='message-scroller-item']")).ToHaveCountAsync(4);

        await page.SetViewportSizeAsync(390, 844);
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/data-table").ToString());
        var dataTable = page.Locator("#preview .showcase-data-table");
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);
        await Assertions.Expect(dataTable.Locator(".shadcn-data-table-frame")).ToBeVisibleAsync();
        Assert.Equal("grid", await dataTable.Locator("[data-slot='data-table-toolbar']").EvaluateAsync<string>("element => getComputedStyle(element).display"));
    }

    [Fact]
    public async Task DatePickerDossierUsesOneResponsiveTriggerForSingleAndRangeSelection()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/date-picker").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var trigger = page.GetByTestId("forms-dossier-date-picker");
        var root = trigger.Locator("xpath=ancestor-or-self::*[@data-slot='date-picker'][1]");
        await Assertions.Expect(root.Locator("[data-slot='date-picker-input']")).ToHaveCountAsync(0);
        await Assertions.Expect(root.Locator("[data-slot='date-picker-content']")).ToHaveCountAsync(0);

        await trigger.ClickAsync();
        var content = root.Locator("[data-slot='date-picker-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        var triggerBox = await trigger.BoundingBoxAsync();
        var contentBox = await content.BoundingBoxAsync();
        Assert.NotNull(triggerBox);
        Assert.NotNull(contentBox);
        Assert.True(contentBox!.Y >= triggerBox!.Y + triggerBox.Height - 1);
        Assert.InRange(contentBox.X, 0, 390 - contentBox.Width + 1);
        await Assertions.Expect(root).ToHaveCSSAsync("direction", "rtl");
        await trigger.ClickAsync();

        await page.GetByTestId("control-date-picker-mode").SelectOptionAsync("Single");
        await trigger.ClickAsync();
        await root.Locator("[data-day='2026-08-20']").ClickAsync();
        await Assertions.Expect(trigger).ToContainTextAsync("20");
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("@bind-Value=\"SelectedDate\"");

        var clear = root.Locator("[data-slot='date-picker-clear']");
        await Assertions.Expect(clear).ToBeVisibleAsync();
        await Assertions.Expect(clear.Locator("svg")).ToHaveCountAsync(1);
        await clear.ClickAsync();
        await Assertions.Expect(clear).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToContainTextAsync("Pick a delivery date");
    }

    [Fact]
    public async Task CardDossierStaysCenteredResponsiveAndSynchronizesInteractiveSource()
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/card").ToString());
        var canvas = page.GetByTestId("component-preview-canvas");
        var card = page.GetByTestId("card-dossier-preview");
        var action = page.GetByTestId("card-toggle-production");

        await Assertions.Expect(card).ToContainTextAsync("Production order #MO-2418");
        await Assertions.Expect(page.GetByTestId("card-production-status")).ToHaveTextAsync("In progress");
        await action.FocusAsync();
        await action.PressAsync("Enter");
        await Assertions.Expect(page.GetByTestId("card-production-status")).ToHaveTextAsync("Paused");
        await Assertions.Expect(action).ToHaveTextAsync("Resume production");
        await Assertions.Expect(action).ToHaveAttributeAsync("aria-pressed", "true");

        var centered = await card.EvaluateAsync<bool>("""
            card => {
                const canvas = card.closest('[data-testid="component-preview-canvas"]');
                const cardRect = card.getBoundingClientRect();
                const canvasRect = canvas.getBoundingClientRect();
                return Math.abs((cardRect.left + cardRect.width / 2) - (canvasRect.left + canvasRect.width / 2)) <= 1;
            }
            """);
        Assert.True(centered, "The Card dossier should remain centered in its preview canvas.");

        await page.GetByTestId("control-card-size").SelectOptionAsync("Small");
        await Assertions.Expect(card).ToHaveAttributeAsync("data-size", "sm");
        await Assertions.Expect(page.Locator("#preview pre").First).ToContainTextAsync("Size=\"ShadcnCardSize.Small\"");
        await page.GetByTestId("control-card-spacing").CheckAsync();
        await Assertions.Expect(card).ToHaveAttributeAsync("style", new Regex("--shadcn-card-spacing: 0.75rem"));
        await Assertions.Expect(page.Locator("#preview pre").First).ToContainTextAsync("Spacing=\"0.75rem\"");

        await page.GetByTestId("control-card-action").UncheckAsync();
        await Assertions.Expect(page.GetByTestId("card-toggle-production")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByTestId("card-production-status")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#preview pre").First).Not.ToContainTextAsync("ToggleProduction");

        await canvas.EvaluateAsync("element => { element.dir = 'rtl'; element.setAttribute('data-shadcn-theme', 'dark'); }");
        await Assertions.Expect(card).ToHaveCSSAsync("overflow", "hidden");

        await page.SetViewportSizeAsync(390, 844);
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);
        await Assertions.Expect(card).ToBeVisibleAsync();
        await Assertions.Expect(card.Locator(".showcase-card-dossier__metrics")).ToHaveCSSAsync("grid-template-columns", new Regex("^.+$"));
        Assert.Empty(errors);
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
