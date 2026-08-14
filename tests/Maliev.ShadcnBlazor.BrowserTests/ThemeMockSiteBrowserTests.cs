using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeMockSiteBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    public static TheoryData<int, int> ReleaseViewports => new()
    {
        { 1440, 900 },
        { 1024, 768 },
        { 768, 1024 },
        { 390, 844 },
        { 320, 568 }
    };

    [Fact]
    public async Task OperationsWorkflowFiltersCompletesAndExposesEveryDeterministicState()
    {
        await using var context = await NewContextAsync(1280, 900);
        var page = await OpenStudioAsync(context);

        await page.GetByTestId("operations-query").FillAsync("aluminum");
        await Assertions.Expect(page.Locator("[data-testid='operations-table'] tbody tr")).ToHaveCountAsync(2);
        await page.GetByTestId("operation-complete-MO-24018").ClickAsync();
        await Assertions.Expect(page.GetByTestId("operations-announcement")).ToContainTextAsync("MO-24018 moved to completed");

        await SelectOperationsStateAsync(page, "operations-state-loading");
        await Assertions.Expect(page.GetByTestId("operations-skeleton")).ToBeVisibleAsync();
        await SelectOperationsStateAsync(page, "operations-state-empty");
        await Assertions.Expect(page.GetByTestId("operations-empty")).ToBeVisibleAsync();
        await SelectOperationsStateAsync(page, "operations-state-error");
        await Assertions.Expect(page.GetByTestId("operations-error")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ManufacturingWorkflowValidatesReviewsConfirmsAndSucceeds()
    {
        await using var context = await NewContextAsync(1024, 800);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Manufacturing request");

        await page.GetByTestId("manufacturing-state-loading").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-skeleton")).ToBeVisibleAsync();
        await page.GetByTestId("manufacturing-state-empty").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-empty")).ToBeVisibleAsync();
        await page.GetByTestId("manufacturing-state-error").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-state-error-alert")).ToBeVisibleAsync();
        await page.GetByTestId("manufacturing-state-ready").ClickAsync();

        await page.GetByTestId("manufacturing-review").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-errors")).ToContainTextAsync("Project name");
        await page.GetByTestId("manufacturing-project").FillAsync("Pump bracket pilot");
        await page.GetByTestId("manufacturing-part").FillAsync("mounting-bracket.step");
        await page.GetByTestId("manufacturing-attach").ClickAsync();
        await page.GetByTestId("manufacturing-review").ClickAsync();
        await page.GetByTestId("manufacturing-open-confirmation").ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await page.GetByTestId("manufacturing-confirm").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-success")).ToContainTextAsync("MR-240812");
        Assert.Equal(0, await page.Locator("[data-testid='manufacturing-request-mock'] [inert]").CountAsync());
    }

    [Fact]
    public async Task CustomerWorkflowSearchesOpensMessagesAndResetsWhenRevisited()
    {
        await using var context = await NewContextAsync(768, 1024);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Customer workspace");

        await page.GetByTestId("customer-query").FillAsync("วริศรา");
        await Assertions.Expect(page.Locator("[data-testid='customer-table'] tbody tr")).ToHaveCountAsync(1);
        await page.GetByTestId("customer-open-CUS-103").ClickAsync();
        await page.GetByTestId("customer-tab-messages").ClickAsync();
        await page.GetByTestId("customer-message").FillAsync("Your revised drawing is ready.");
        await page.GetByTestId("customer-send-message").ClickAsync();
        await Assertions.Expect(page.GetByTestId("customer-announcement")).ToContainTextAsync("Message sent to วริศรา ตั้งใจ");

        await SelectMockupAsync(page, "Operations dashboard");
        await SelectMockupAsync(page, "Customer workspace");
        await Assertions.Expect(page.GetByTestId("customer-query")).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(page.GetByTestId("customer-detail-sheet")).ToHaveCountAsync(0);
    }

    [Theory]
    [MemberData(nameof(ReleaseViewports))]
    public async Task MockSitesHaveNoOverflowAndKeepCoarseTargetsAtReleaseWidths(int width, int height)
    {
        var errors = new List<string>();
        await using var context = await NewContextAsync(width, height);
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("mock-site-host").WaitForAsync();

        foreach (var selection in new[] { "Operations dashboard", "Manufacturing request", "Customer workspace" })
        {
            await SelectMockupAsync(page, selection);
            var overflow = await page.EvaluateAsync<double>(
                "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
            Assert.InRange(overflow, 0, 1);
        }

        var targetHeight = await page.GetByTestId("customer-state-loading").EvaluateAsync<double>("element => element.getBoundingClientRect().height");
        Assert.True(targetHeight >= 44, $"Expected a 44px coarse target, got {targetHeight}px.");
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ThemeLocaleDirectionAndKeyboardPropagateToEveryMockSite()
    {
        await using var context = await NewContextAsync(390, 844);
        var page = await OpenStudioAsync(context);

        await page.GetByTestId("mode-dark").ClickAsync();
        await page.GetByTestId("direction-rtl").ClickAsync();
        await page.GetByTestId("locale-thai").ClickAsync();
        await page.GetByTestId("viewport-mobile").ClickAsync();
        var chart = page.Locator("input[data-testid='theme-token-dark-chart1']");
        await chart.FillAsync("#7c3aed");
        await chart.PressAsync("Tab");
        await page.GetByTestId("operations-query").FocusAsync();
        await page.Keyboard.TypeAsync("อลูมิเนียม");

        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        await Assertions.Expect(page.GetByTestId("operations-dashboard-mock")).ToHaveAttributeAsync("lang", "th");
        await Assertions.Expect(page.GetByTestId("operations-title")).ToContainTextAsync("ภาพรวมการผลิต");
        await Assertions.Expect(page.GetByTestId("operations-query")).ToBeFocusedAsync();
        await Assertions.Expect(page.Locator(".mock-chart-column").First.Locator("span")).ToHaveCSSAsync("background-color", "rgb(124, 58, 237)");
        var transitionDuration = await page.Locator(".mock-progress-track span").First.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(transitionDuration, 0, 0.00001);

        Assert.Empty(await AccessibilityViolationsAsync(page));

        await SelectMockupAsync(page, "Manufacturing request");
        await Assertions.Expect(page.GetByTestId("manufacturing-request-mock")).ToHaveAttributeAsync("lang", "th");
        Assert.Empty(await AccessibilityViolationsAsync(page));
        await SelectMockupAsync(page, "Customer workspace");
        await Assertions.Expect(page.GetByTestId("customer-workspace-mock")).ToHaveAttributeAsync("lang", "th");
        Assert.Empty(await AccessibilityViolationsAsync(page));
    }

    [Fact]
    public async Task ManufacturingDialogTrapsFocusEscapesAndRestoresItsOpener()
    {
        await using var context = await NewContextAsync(1024, 800);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Manufacturing request");
        await page.GetByTestId("manufacturing-project").FillAsync("Pump bracket pilot");
        await page.GetByTestId("manufacturing-part").FillAsync("mounting-bracket.step");
        await page.GetByTestId("manufacturing-attach").ClickAsync();
        await page.GetByTestId("manufacturing-review").ClickAsync();
        var opener = page.GetByTestId("manufacturing-open-confirmation");
        await opener.ClickAsync();

        await Assertions.Expect(page.GetByTestId("manufacturing-confirm")).ToBeFocusedAsync();
        Assert.True(await opener.EvaluateAsync<bool>("element => element.inert || element.closest('[inert]') !== null"));
        await page.Keyboard.PressAsync("Shift+Tab");
        await Assertions.Expect(page.GetByTestId("manufacturing-cancel")).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(page.GetByTestId("manufacturing-confirm")).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Assertions.Expect(opener).ToBeFocusedAsync();
    }

    [Fact]
    public async Task CustomerSheetManagesFocusAndTabsExposeCompleteKeyboardRelationships()
    {
        await using var context = await NewContextAsync(1024, 800);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Customer workspace");
        var opener = page.GetByTestId("customer-open-CUS-101");
        await opener.ClickAsync();

        var close = page.GetByTestId("customer-detail-close");
        var overview = page.GetByTestId("customer-tab-overview");
        var activity = page.GetByTestId("customer-tab-activity");
        var messages = page.GetByTestId("customer-tab-messages");
        var panel = page.GetByRole(AriaRole.Tabpanel);
        await Assertions.Expect(close).ToBeFocusedAsync();
        Assert.True(await opener.EvaluateAsync<bool>("element => element.inert || element.closest('[inert]') !== null"));
        await Assertions.Expect(overview).ToHaveAttributeAsync("aria-controls", "customer-panel-overview");
        await Assertions.Expect(panel).ToHaveAttributeAsync("id", "customer-panel-overview");
        await Assertions.Expect(panel).ToHaveAttributeAsync("aria-labelledby", "customer-tab-overview");

        await overview.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await Assertions.Expect(activity).ToBeFocusedAsync();
        await Assertions.Expect(activity).ToHaveAttributeAsync("aria-selected", "true");
        await page.Keyboard.PressAsync("End");
        await Assertions.Expect(messages).ToBeFocusedAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Tabpanel)).ToHaveAttributeAsync("id", "customer-panel-messages");
        await page.Keyboard.PressAsync("Home");
        await Assertions.Expect(overview).ToBeFocusedAsync();

        await close.FocusAsync();
        await page.Keyboard.PressAsync("Shift+Tab");
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Share update" })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.GetByTestId("customer-detail-sheet")).ToHaveCountAsync(0);
        await Assertions.Expect(opener).ToBeFocusedAsync();
    }

    [Fact]
    public async Task ThaiModeLocalizesEveryMockSiteInteractiveNameAndDynamicMessage()
    {
        await using var context = await NewContextAsync(1280, 900);
        var page = await OpenStudioAsync(context);
        await page.GetByTestId("locale-thai").ClickAsync();

        await page.GetByTestId("operation-complete-MO-24018").ClickAsync();
        await Assertions.Expect(page.GetByTestId("operations-announcement")).ToContainTextAsync("ย้าย MO-24018");
        Assert.Empty(await UnlocalizedThaiNamesAsync(page));

        await SelectMockupAsync(page, "Manufacturing request");
        await page.GetByTestId("manufacturing-review").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-errors")).ToContainTextAsync("กรุณา");
        Assert.Empty(await UnlocalizedThaiNamesAsync(page));

        await SelectMockupAsync(page, "Customer workspace");
        await page.GetByTestId("customer-open-CUS-101").ClickAsync();
        Assert.Empty(await UnlocalizedThaiNamesAsync(page));
    }

    [Fact]
    public async Task OverlaysIsolateEveryBackgroundBranchAndRestoreExactPriorState()
    {
        await using var context = await NewContextAsync(1024, 800);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Manufacturing request");
        var manufacturingHeader = page.Locator("[data-testid='manufacturing-request-mock'] > .mock-product-header");
        await manufacturingHeader.EvaluateAsync("element => element.setAttribute('aria-hidden', 'false')");
        await OpenManufacturingDialogAsync(page);
        Assert.Empty(await OverlayIsolationViolationsAsync(page, "manufacturing-request-mock", ".mock-dialog-backdrop"));
        await page.GetByTestId("manufacturing-cancel").ClickAsync();
        await Assertions.Expect(manufacturingHeader).ToHaveAttributeAsync("aria-hidden", "false");
        Assert.False(await manufacturingHeader.EvaluateAsync<bool>("element => element.inert"));

        await SelectMockupAsync(page, "Customer workspace");
        var customerHeader = page.Locator("[data-testid='customer-workspace-mock'] > .mock-product-header");
        await customerHeader.EvaluateAsync("element => element.setAttribute('aria-hidden', 'false')");
        await page.GetByTestId("customer-open-CUS-101").ClickAsync();
        Assert.Empty(await OverlayIsolationViolationsAsync(page, "customer-workspace-mock", ".mock-sheet-backdrop"));
        await page.GetByTestId("customer-detail-close").ClickAsync();
        await Assertions.Expect(customerHeader).ToHaveAttributeAsync("aria-hidden", "false");
        Assert.False(await customerHeader.EvaluateAsync<bool>("element => element.inert"));
    }

    [Fact]
    public async Task OverlayDisposalOnMockSwitchRestoresStateAndRemovesTrapListeners()
    {
        await using var context = await NewContextAsync(1024, 800);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Manufacturing request");
        await OpenManufacturingDialogAsync(page);

        await SelectMockupAsync(page, "Customer workspace");
        Assert.Equal(0, await page.Locator("[data-mock-site] [inert], [data-mock-site][inert]").CountAsync());
        await page.GetByTestId("customer-open-CUS-101").ClickAsync();
        await SelectMockupAsync(page, "Operations dashboard");
        Assert.Equal(0, await page.Locator("[data-mock-site] [inert], [data-mock-site][inert]").CountAsync());
        await page.GetByTestId("operations-query").FocusAsync();
        await page.Keyboard.PressAsync("Tab");
        Assert.False(await page.EvaluateAsync<bool>("() => document.activeElement?.closest('[data-mock-site]') == null"));
    }

    [Fact]
    public async Task ManufacturingConfirmationMovesFocusToTheStableSuccessResult()
    {
        await using var context = await NewContextAsync(1024, 800);
        var page = await OpenStudioAsync(context);
        await SelectMockupAsync(page, "Manufacturing request");
        await OpenManufacturingDialogAsync(page);

        await page.GetByTestId("manufacturing-confirm").ClickAsync();

        await Assertions.Expect(page.GetByTestId("manufacturing-success-focus")).ToBeFocusedAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-success")).ToContainTextAsync("MR-240812");
        Assert.Equal(0, await page.Locator("[data-testid='manufacturing-request-mock'] [inert]").CountAsync());
    }

    [Fact]
    public async Task ManufacturingAndCustomerUseComputedThemeRtlThaiReducedMotionAndAccessibleNames()
    {
        await using var context = await NewContextAsync(390, 844);
        var page = await OpenStudioAsync(context);
        await page.GetByTestId("mode-dark").ClickAsync();
        await page.GetByTestId("direction-rtl").ClickAsync();
        await page.GetByTestId("locale-thai").ClickAsync();
        var primary = page.Locator("input[data-testid='theme-token-dark-primary']");
        await primary.FillAsync("#c026d3");
        await primary.PressAsync("Tab");

        await SelectMockupAsync(page, "Manufacturing request");
        var manufacturing = page.GetByTestId("manufacturing-request-mock");
        Assert.Equal("rtl", await manufacturing.EvaluateAsync<string>("element => getComputedStyle(element).direction"));
        await Assertions.Expect(page.GetByTestId("manufacturing-review")).ToHaveCSSAsync("background-color", "rgb(192, 38, 211)");
        await page.GetByTestId("manufacturing-review").ClickAsync();
        await Assertions.Expect(page.GetByTestId("manufacturing-errors")).ToContainTextAsync("กรุณาระบุชื่อโครงการ");
        var progressDuration = await page.Locator("[data-testid='manufacturing-request-mock'] .mock-progress-track span").EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(progressDuration, 0, 0.00001);
        Assert.Empty(await AccessibilityViolationsAsync(page));

        await SelectMockupAsync(page, "Customer workspace");
        var customer = page.GetByTestId("customer-workspace-mock");
        Assert.Equal("rtl", await customer.EvaluateAsync<string>("element => getComputedStyle(element).direction"));
        await Assertions.Expect(customer.GetByRole(AriaRole.Button, new() { Name = "รีเซ็ตพื้นที่ทำงาน" })).ToHaveCSSAsync("background-color", "rgb(192, 38, 211)");
        await page.GetByTestId("customer-state-loading").ClickAsync();
        var animationDuration = await page.GetByTestId("customer-skeleton").Locator("span").First.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).animationDuration) || 0");
        Assert.InRange(animationDuration, 0, 0.00001);
        await page.GetByTestId("customer-state-ready").ClickAsync();
        await page.GetByTestId("customer-open-CUS-101").ClickAsync();
        await page.GetByTestId("customer-tab-messages").ClickAsync();
        await page.GetByTestId("customer-message").FillAsync("พร้อมส่งแบบแก้ไขแล้ว");
        await page.GetByTestId("customer-send-message").ClickAsync();
        await Assertions.Expect(page.GetByTestId("customer-announcement")).ToContainTextAsync("ส่งข้อความถึง กานต์ชนก ศรีสุข แล้ว");
        Assert.Empty(await AccessibilityViolationsAsync(page));
    }

    private async Task<IBrowserContext> NewContextAsync(int width, int height) =>
        await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            HasTouch = true,
            ReducedMotion = ReducedMotion.Reduce
        });

    private async Task<IPage> OpenStudioAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("mock-site-host").WaitForAsync();
        return page;
    }

    private static async Task SelectMockupAsync(IPage page, string label)
    {
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Preview composition" }).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = label, Exact = true }).ClickAsync();
    }

    private static async Task SelectOperationsStateAsync(IPage page, string testId)
    {
        var menu = page.GetByTestId("operations-actions");
        if (await menu.GetAttributeAsync("open") is null)
            await menu.Locator("summary").ClickAsync();
        await page.GetByTestId(testId).ClickAsync();
    }

    private static async Task OpenManufacturingDialogAsync(IPage page)
    {
        await page.GetByTestId("manufacturing-project").FillAsync("Pump bracket pilot");
        await page.GetByTestId("manufacturing-part").FillAsync("mounting-bracket.step");
        await page.GetByTestId("manufacturing-attach").ClickAsync();
        await page.GetByTestId("manufacturing-review").ClickAsync();
        await page.GetByTestId("manufacturing-open-confirmation").ClickAsync();
    }

    private static async Task<IReadOnlyList<string>> OverlayIsolationViolationsAsync(IPage page, string rootTestId, string backdropSelector) =>
        await page.EvaluateAsync<string[]>("""
            ([rootTestId, backdropSelector]) => {
              const root = document.querySelector(`[data-testid="${rootTestId}"]`);
              const backdrop = root?.querySelector(backdropSelector);
              if (!root || !backdrop) return ['root or backdrop missing'];
              const errors = [];
              let branch = backdrop;
              while (branch !== root) {
                for (const sibling of branch.parentElement.children) {
                  if (sibling === branch) continue;
                  if (!sibling.inert) errors.push(`${sibling.tagName}.${sibling.className} is not inert`);
                  if (sibling.getAttribute('aria-hidden') !== 'true') errors.push(`${sibling.tagName}.${sibling.className} is not aria-hidden`);
                }
                branch = branch.parentElement;
              }
              return errors;
            }
            """, new[] { rootTestId, backdropSelector }) ?? Array.Empty<string>();

    private static async Task<IReadOnlyList<string>> AccessibilityViolationsAsync(IPage page) =>
        await page.EvaluateAsync<string[]>("""
            () => {
              const errors = [];
              for (const control of document.querySelectorAll('input, select, textarea, button')) {
                if (control.disabled) continue;
                const name = control.getAttribute('aria-label')
                  || (control.id && document.querySelector(`label[for="${CSS.escape(control.id)}"]`)?.textContent)
                  || control.textContent
                  || control.getAttribute('placeholder');
                if (!name?.trim()) errors.push(`${control.tagName.toLowerCase()} has no accessible name`);
              }
              for (const image of document.querySelectorAll('[data-mock-site] [role="img"]')) {
                if (!image.getAttribute('aria-label') && !image.getAttribute('aria-labelledby')) errors.push('role=img has no name');
              }
              return errors;
            }
            """) ?? Array.Empty<string>();

    private static async Task<IReadOnlyList<string>> UnlocalizedThaiNamesAsync(IPage page) =>
        await page.EvaluateAsync<string[]>("""
            () => {
              const site = document.querySelector('[data-mock-site]');
              const allowed = /^(MALIEV|ME|MO-|CUS-|MR-|QT-|STEP|CAD|M6|CNC|FDM|PA-CF|Aluminum|Stainless|As machined|Bead blasted|Black anodized|[‹›×✓●⌂◇]|\d)/i;
              const thai = /[\u0E00-\u0E7F]/;
              const elements = site?.querySelectorAll('button, input, select, textarea, a, [role="img"], [role="progressbar"]') ?? [];
              return [...elements].map(element => {
                const id = element.id;
                return element.getAttribute('aria-label')
                  || (id && site.querySelector(`label[for="${CSS.escape(id)}"]`)?.textContent)
                  || element.textContent
                  || element.getAttribute('placeholder')
                  || '';
              }).map(name => name.trim()).filter(name => name && !thai.test(name) && !allowed.test(name));
            }
            """) ?? Array.Empty<string>();
}
