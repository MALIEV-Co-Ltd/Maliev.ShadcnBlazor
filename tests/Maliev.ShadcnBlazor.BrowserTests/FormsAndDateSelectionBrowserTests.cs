using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;
using System.Text.Json;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class FormsAndDateSelectionBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task NativeSelectDossierUsesGroupedNativeSemanticsAndUpdatesItsOperationalSummary()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/native-select").ToString());

        var preview = page.GetByTestId("native-select-dossier-preview");
        var select = preview.GetByTestId("forms-dossier-native-select");
        await Assertions.Expect(preview).ToBeVisibleAsync();
        await Assertions.Expect(select.Locator("optgroup")).ToHaveCountAsync(2);
        await Assertions.Expect(select.Locator("option:disabled")).ToHaveCountAsync(1);
        await select.SelectOptionAsync("urgent");
        await Assertions.Expect(page.GetByTestId("native-select-lead-time")).ToContainTextAsync("2–3 business days");
        await page.GetByTestId("control-native-select-compact").CheckAsync();
        await Assertions.Expect(select).ToHaveAttributeAsync("data-size", "sm");
        await select.FocusAsync();
        await Assertions.Expect(select).ToBeFocusedAsync();
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth-document.documentElement.clientWidth, document.body.scrollWidth-document.body.clientWidth)"), 0, 1);
    }

    [Theory]
    [InlineData(1440, 900, "light", "ltr")]
    [InlineData(390, 844, "dark", "ltr")]
    [InlineData(320, 568, "dark", "rtl")]
    public async Task RouteRendersWithoutOverflowAndUsesThemeDirectionAndResponsiveContent(int width, int height, string theme, string direction)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = width, Height = height }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(Url(theme, direction));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();

        await Assertions.Expect(page.Locator("[data-shadcn-scope]")).ToHaveAttributeAsync("data-shadcn-theme", theme);
        await Assertions.Expect(page.Locator("[data-shadcn-scope]")).ToHaveAttributeAsync("dir", direction);
        var primaryFixture = page.GetByTestId("forms-native-form");
        await Assertions.Expect(primaryFixture.Locator("[data-slot='input'], [data-slot='input-group-control']")).ToHaveCountAsync(2);
        await Assertions.Expect(primaryFixture.Locator("[data-slot='input-otp']")).ToHaveCountAsync(2);
        await Assertions.Expect(primaryFixture.Locator("[data-slot='input-otp-slot']")).ToHaveCountAsync(10);
        await Assertions.Expect(page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]").Locator("[data-slot='calendar-day']")).ToHaveCountAsync(42);
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task SelectComboboxOtpCalendarAndFormPayloadOperateInRealBrowser()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 1000 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("light", "ltr"));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();

        var select = page.GetByTestId("forms-select");
        await select.FocusAsync();
        await select.PressAsync("ArrowDown");
        await Assertions.Expect(select).ToHaveAttributeAsync("aria-expanded", "true");
        await select.PressAsync("End");
        await select.PressAsync("Enter");
        await Assertions.Expect(select).ToContainTextAsync("Metal 3D printing");

        var combobox = page.GetByTestId("forms-combobox");
        await combobox.FillAsync("316");
        await Assertions.Expect(page.Locator("[data-slot='combobox-content'] [role='option']")).ToHaveCountAsync(1);
        await combobox.PressAsync("ArrowDown");
        var activeId = await combobox.GetAttributeAsync("aria-activedescendant");
        Assert.False(string.IsNullOrWhiteSpace(activeId));
        await combobox.PressAsync("Enter");
        await Assertions.Expect(combobox).ToHaveValueAsync("Stainless 316L");

        var otp = page.Locator("[data-slot='input-otp']").First;
        await otp.FillAsync("12 a3-4567");
        await Assertions.Expect(otp).ToHaveValueAsync("123456");

        await Assertions.Expect(page.Locator("input[name='process']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("input[name='material']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("input[name='verificationCode']")).ToHaveCountAsync(1);
        Assert.Equal("forms-native-form", await page.Locator("input[name='process']").EvaluateAsync<string>("input => input.form?.dataset.testid ?? 'none'"));
        var payloadJson = await page.GetByTestId("forms-native-form").EvaluateAsync<string>(
            "form => JSON.stringify(Object.fromEntries([...new FormData(form).keys()].map(key => [key, new FormData(form).getAll(key)])))");
        var payload = JsonSerializer.Deserialize<Dictionary<string, string[]>>(payloadJson)!;
        Assert.Equal(["slm"], payload["process"]);
        Assert.Equal(["ss316"], payload["material"]);
        Assert.Equal(["123456"], payload["verificationCode"]);
        Assert.Equal(["2026-08-20"], payload["deliveryDate"]);

        var dateInput = page.GetByTestId("forms-date-picker").Locator("xpath=ancestor-or-self::*[@data-slot='date-picker'][1]").Locator("[data-slot='date-picker-input']");
        await dateInput.FillAsync("not a date");
        await dateInput.PressAsync("Tab");
        await Assertions.Expect(dateInput).ToHaveValueAsync("not a date");
        Assert.False(await page.GetByTestId("forms-native-form").EvaluateAsync<bool>("form => form.checkValidity()"));
        Assert.Equal(0, await page.Locator("input[name='deliveryDate']").CountAsync());
        var submitCount = await page.GetByTestId("forms-native-form").EvaluateAsync<int>("form => { let count=0; form.addEventListener('submit', () => count++); form.requestSubmit(); return count; }");
        Assert.Equal(0, submitCount);
        await Assertions.Expect(page.Locator("[data-slot='date-picker-form-control']")).ToBeFocusedAsync();

        var calendar = page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]");
        var selectedRange = calendar.Locator("[data-range-start='true'],[data-range-middle='true'],[data-range-end='true']");
        Assert.True(await selectedRange.CountAsync() >= 2);
        var focusedDay = calendar.Locator("[data-slot='calendar-day']").Filter(new() { HasText = "20" }).First;
        await focusedDay.FocusAsync();
        await focusedDay.PressAsync("ArrowRight");
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-21']")).ToBeFocusedAsync();

    }

    [Fact]
    public async Task NativeFieldsInputGroupMultipleComboboxCalendarAndDatePickerOperateInRealBrowser()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 1000 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("light", "ltr"));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();

        var form = page.GetByTestId("forms-native-form");
        var input = form.Locator("input[name='partName']");
        await input.FillAsync("Valve body");
        await Assertions.Expect(input).ToHaveValueAsync("Valve body");
        var textarea = form.Locator("textarea[name='notes']");
        await textarea.FillAsync("Deburr and inspect");
        await Assertions.Expect(textarea).ToHaveValueAsync("Deburr and inspect");

        var budgetGroup = form.Locator("[data-slot='input-group']").First;
        await budgetGroup.Locator("[data-slot='input-group-addon']").First.ClickAsync();
        await Assertions.Expect(form.Locator("input[name='budget']")).ToBeFocusedAsync();

        var multiple = page.GetByTestId("forms-multiple-combobox");
        var multipleRoot = multiple.Locator("xpath=ancestor::*[@data-slot='combobox'][1]");
        await Assertions.Expect(multipleRoot.Locator("[data-slot='combobox-chip']")).ToHaveCountAsync(1);
        await multiple.FocusAsync();
        await Assertions.Expect(multiple).ToHaveAttributeAsync("aria-expanded", "true");
        await multipleRoot.Locator("[data-slot='combobox-item'][data-value='ss316']").ClickAsync();
        await Assertions.Expect(multipleRoot.Locator("[data-slot='combobox-chip']")).ToHaveCountAsync(2);
        await multipleRoot.GetByRole(AriaRole.Button, new() { Name = "Remove Aluminum 6061" }).ClickAsync();
        await Assertions.Expect(multipleRoot.Locator("[data-slot='combobox-chip']")).ToHaveCountAsync(1);

        var calendar = page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]");
        await calendar.Locator("[data-day='2026-08-24']").ClickAsync();
        await calendar.Locator("[data-day='2026-08-26']").ClickAsync();
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-24']")).ToHaveAttributeAsync("data-range-start", "true");
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-25']")).ToHaveAttributeAsync("data-range-middle", "true");
        await Assertions.Expect(calendar.Locator("[data-day='2026-08-26']")).ToHaveAttributeAsync("data-range-end", "true");
        await calendar.Locator("[data-slot='calendar-next']").ClickAsync();
        await Assertions.Expect(calendar.Locator("[data-slot='calendar-month-select'] [data-slot='select-value']")).ToHaveTextAsync("กันยายน");

        var picker = page.GetByTestId("forms-date-picker");
        await picker.ClickAsync();
        var pickerRoot = picker.Locator("xpath=ancestor-or-self::*[@data-slot='date-picker'][1]");
        await pickerRoot.Locator("[data-day='2026-09-24']").ClickAsync();
        await Assertions.Expect(picker).ToContainTextAsync("24");
        await pickerRoot.Locator("[data-slot='date-picker-clear']").ClickAsync();
        await Assertions.Expect(picker).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(form.Locator("input[name='deliveryDate']")).ToHaveValueAsync(string.Empty);

        var payloadJson = await form.EvaluateAsync<string>("form => JSON.stringify(Object.fromEntries(new FormData(form)))");
        var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(payloadJson)!;
        Assert.Equal("Valve body", payload["partName"]);
        Assert.Equal("Deburr and inspect", payload["notes"]);
        Assert.Equal("ss316", payload["materials"]);
        Assert.Equal(string.Empty, payload["deliveryDate"]);
    }

    [Fact]
    public async Task DarkFocusAndInvalidStatesUseTokenBackedComputedStyles()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 800, Height = 900 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("dark", "ltr"));
        var input = page.Locator("[data-slot='input']").First;
        await input.FocusAsync();
        var styles = await input.EvaluateAsync<string[]>("element => { const s=getComputedStyle(element); return [s.height,s.borderStyle,s.boxShadow,s.backgroundColor]; }");
        Assert.Equal("36px", styles[0]);
        Assert.Equal("solid", styles[1]);
        Assert.NotEqual("none", styles[2]);
        Assert.NotEqual("rgba(0, 0, 0, 0)", styles[3]);
        Assert.False(await page.Locator("[data-slot='input-otp']").First.EvaluateAsync<bool>("input => input.hasAttribute('pattern')"));
        var calendar = page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]");
        var yearSelect = calendar.Locator("[data-slot='calendar-year-select']");
        await yearSelect.Locator("[data-slot='select-trigger']").ClickAsync();
        await Assertions.Expect(yearSelect.Locator("[role='option'][data-value='2026']")).ToHaveTextAsync("2569✓");
    }

    [Fact]
    public async Task OtpCaretDeletionThaiGraphemesRtlAndReadonlyNativeSelectOperateInRealDom()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 800, Height = 900 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("light", "ltr"));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();

        var otp = page.Locator("[data-slot='input-otp']").First;
        await otp.FillAsync("123456");
        await otp.EvaluateAsync("input => input.setSelectionRange(3, 3)");
        await otp.PressAsync("Backspace");
        await Assertions.Expect(otp).ToHaveValueAsync("12456");
        await otp.EvaluateAsync("input => input.setSelectionRange(2, 2)");
        await otp.PressAsync("Delete");
        await Assertions.Expect(otp).ToHaveValueAsync("1256");

        var thaiOtp = page.GetByTestId("forms-thai-otp");
        await thaiOtp.FillAsync("ก้ขค");
        await Assertions.Expect(thaiOtp).ToHaveValueAsync("ก้ขค");
        await Assertions.Expect(thaiOtp).ToHaveAttributeAsync("dir", "rtl");
        await thaiOtp.EvaluateAsync("input => { input.setSelectionRange(2, 2); input.dispatchEvent(new Event('select')); }");
        await Assertions.Expect(page.GetByTestId("forms-thai-otp-root").Locator("[data-slot='input-otp-slot']").Nth(1)).ToHaveAttributeAsync("data-active", "true");
        await thaiOtp.EvaluateAsync("input => input.setSelectionRange(0, 2)");
        await thaiOtp.PressAsync("Backspace");
        await Assertions.Expect(thaiOtp).ToHaveValueAsync("ขค");

        var readOnlySelect = page.GetByTestId("forms-native-select-readonly");
        await Assertions.Expect(readOnlySelect).ToHaveValueAsync("standard");
        await readOnlySelect.SelectOptionAsync("urgent");
        await Assertions.Expect(readOnlySelect).ToHaveValueAsync("standard");
    }

    [Fact]
    public async Task NamedAccessibilityRulesCoverEveryFormsAndDateSelectionComponent()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 1000 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("light", "ltr"));
        var root = page.GetByTestId("forms-date-fixture");
        await root.WaitForAsync();

        await Assertions.Expect(page.GetByTestId("forms-select")).ToHaveAttributeAsync("aria-haspopup", "listbox");
        await Assertions.Expect(page.GetByTestId("forms-combobox")).ToHaveAttributeAsync("role", "combobox");
        await Assertions.Expect(page.Locator("[data-slot='input-otp']").First).ToHaveAttributeAsync("aria-label", "Verification code");
        await Assertions.Expect(page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]").Locator("[role='grid']")).Not.ToHaveAttributeAsync("aria-label", "");
        Assert.Single(await page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]").Locator("[data-slot='calendar-day'][tabindex='0']:not(:disabled)").AllAsync());

        var violations = await root.EvaluateAsync<string[]>("""
            root => {
              const issues=[];
              for (const el of root.querySelectorAll('input,textarea,select,button,[role=combobox],[role=grid]')) {
                const name=(el.getAttribute('aria-label')||el.getAttribute('aria-labelledby')||el.textContent||'').trim();
                if (!name && !el.closest('label') && el.type !== 'hidden') issues.push(`unnamed:${el.dataset.slot||el.tagName}`);
                const described=(el.getAttribute('aria-describedby')||'').split(/\s+/).filter(Boolean);
                for (const id of described) if (!document.getElementById(id)) issues.push(`missing-description:${id}`);
              }
              const ids=[...root.querySelectorAll('[id]')].map(x=>x.id);
              if (new Set(ids).size !== ids.length) issues.push('duplicate-id');
              return issues;
            }
            """);
        Assert.Empty(violations);
    }

    [Fact]
    public async Task EveryFormComponentPassesNamedAccessibilityRulesInRestingOpenReadonlyAndInvalidStates()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 1000 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("light", "ltr"));
        var root = page.GetByTestId("forms-date-fixture");
        await root.WaitForAsync();

        var namedControls = new Dictionary<string, ILocator>(StringComparer.Ordinal)
        {
            ["input"] = page.Locator("input[name='partName']"),
            ["textarea"] = page.Locator("textarea[name='notes']"),
            ["native-select"] = page.Locator("select[name='priority']"),
            ["input-group"] = page.Locator("input[name='budget']"),
            ["input-otp"] = page.Locator("[data-slot='input-otp']").First,
            ["select"] = page.GetByTestId("forms-select"),
            ["combobox"] = page.GetByTestId("forms-combobox"),
            ["calendar"] = page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]").Locator("[role='grid']"),
            ["date-picker"] = page.GetByTestId("forms-date-picker")
        };
        foreach (var (slug, control) in namedControls)
        {
            await control.WaitForAsync();
            var name = await control.EvaluateAsync<string>("""
                element => (element.getAttribute('aria-label') ||
                  (element.getAttribute('aria-labelledby') || '').split(/\s+/).filter(Boolean).map(id => document.getElementById(id)?.textContent || '').join(' ') ||
                  element.labels?.[0]?.textContent || element.textContent || '').trim()
                """);
            Assert.False(string.IsNullOrWhiteSpace(name), $"{slug} has no accessible name.");
        }

        var readOnly = page.GetByTestId("forms-native-select-readonly");
        await Assertions.Expect(readOnly).ToHaveAttributeAsync("aria-readonly", "true");
        await readOnly.FocusAsync();
        await Assertions.Expect(readOnly).ToBeFocusedAsync();

        var select = page.GetByTestId("forms-select");
        await select.ClickAsync();
        var selectListboxId = await select.GetAttributeAsync("aria-controls");
        Assert.False(string.IsNullOrWhiteSpace(selectListboxId));
        await Assertions.Expect(page.Locator($"#{selectListboxId}")).ToHaveAttributeAsync("role", "listbox");
        await select.PressAsync("Escape");

        var combobox = page.GetByTestId("forms-combobox");
        await combobox.FocusAsync();
        await Assertions.Expect(combobox).ToHaveAttributeAsync("aria-expanded", "true");
        var comboboxListboxId = await combobox.GetAttributeAsync("aria-controls");
        await Assertions.Expect(page.Locator($"#{comboboxListboxId}")).ToHaveAttributeAsync("role", "listbox");
        await combobox.PressAsync("Escape");

        var datePicker = page.GetByTestId("forms-date-picker");
        await datePicker.ClickAsync();
        var dialogId = await datePicker.GetAttributeAsync("aria-controls");
        await Assertions.Expect(page.Locator($"#{dialogId}")).ToHaveAttributeAsync("role", "dialog");
        await Assertions.Expect(page.Locator($"#{dialogId}")).Not.ToHaveAttributeAsync("aria-label", "");
        await datePicker.PressAsync("Escape");
        await Assertions.Expect(datePicker).ToBeFocusedAsync();

        var dateInput = datePicker.Locator("xpath=ancestor-or-self::*[@data-slot='date-picker'][1]").Locator("[data-slot='date-picker-input']");
        await dateInput.FillAsync("not a date");
        await dateInput.PressAsync("Tab");
        await Assertions.Expect(dateInput).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(datePicker).ToHaveAttributeAsync("aria-invalid", "true");

        var issues = await root.EvaluateAsync<string[]>("""
            root => {
              const issues=[];
              const ids=[...root.querySelectorAll('[id]')].map(element => element.id);
              if (new Set(ids).size !== ids.length) issues.push('duplicate-id');
              for (const element of root.querySelectorAll('[aria-controls],[aria-describedby],[aria-labelledby],[aria-activedescendant]')) {
                const referenceAttributes = element.getAttribute('aria-expanded') === 'true'
                  ? ['aria-controls','aria-describedby','aria-labelledby','aria-activedescendant']
                  : ['aria-describedby','aria-labelledby'];
                for (const attribute of referenceAttributes) {
                  for (const id of (element.getAttribute(attribute)||'').split(/\s+/).filter(Boolean))
                    if (!document.getElementById(id)) issues.push(`${attribute}:${element.dataset.slot||element.tagName}:${id}`);
                }
              }
              for (const element of root.querySelectorAll('[aria-hidden="true"]'))
                if (element.matches('input,button,select,textarea,a[href],[tabindex]:not([tabindex="-1"])') || element.querySelector('input,button,select,textarea,a[href],[tabindex]:not([tabindex="-1"])'))
                  issues.push(`focusable-aria-hidden:${element.dataset.slot||element.tagName}`);
              for (const option of root.querySelectorAll('[role="option"]'))
                if (!option.closest('[role="listbox"]')) issues.push(`orphan-option:${option.id}`);
              for (const cell of root.querySelectorAll('[role="gridcell"]'))
                if (!cell.closest('[role="grid"]')) issues.push('orphan-gridcell');
              return issues;
            }
            """);
        Assert.Empty(issues);
        foreach (var slot in await page.GetByTestId("otp-evidence").Locator("[data-slot='input-otp-slot']").AllAsync())
            Assert.Equal("true", await slot.GetAttributeAsync("aria-hidden"));
    }

    [Fact]
    public async Task PopupAndCompositeStatesKeepNamesFocusOwnershipAndKeyboardEscape()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 1000 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("light", "ltr"));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("form-primitives").Locator("[data-slot='input-group']")).ToHaveAttributeAsync("role", "group");
        await Assertions.Expect(page.Locator("[data-slot='input-otp']").First).ToHaveAttributeAsync("aria-label", "Verification code");
        foreach (var slot in await page.GetByTestId("otp-evidence").Locator("[data-slot='input-otp-slot']").AllAsync())
            Assert.Equal("true", await slot.GetAttributeAsync("aria-hidden"));

        var select = page.GetByTestId("forms-select");
        await select.FocusAsync();
        await select.PressAsync("ArrowDown");
        var selectActive = await select.GetAttributeAsync("aria-activedescendant");
        Assert.False(string.IsNullOrWhiteSpace(selectActive));
        await Assertions.Expect(page.Locator($"#{selectActive}")).ToHaveAttributeAsync("role", "option");
        await select.PressAsync("Escape");
        await Assertions.Expect(select).ToHaveAttributeAsync("aria-expanded", "false");

        var combobox = page.GetByTestId("forms-combobox");
        await combobox.FocusAsync();
        await combobox.PressAsync("ArrowDown");
        var comboboxActive = await combobox.GetAttributeAsync("aria-activedescendant");
        Assert.False(string.IsNullOrWhiteSpace(comboboxActive));
        await Assertions.Expect(page.Locator($"#{comboboxActive}")).ToHaveAttributeAsync("role", "option");
        await combobox.PressAsync("Escape");
        await Assertions.Expect(combobox).ToHaveAttributeAsync("aria-expanded", "false");

        var calendar = page.GetByTestId("forms-calendar").Locator("xpath=ancestor-or-self::*[@data-slot='calendar'][1]");
        Assert.Single(await calendar.Locator("[data-slot='calendar-day'][tabindex='0']:not(:disabled)").AllAsync());
        var dateTrigger = page.GetByTestId("forms-date-picker");
        await dateTrigger.ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog, new() { Name = "Choose date" }).First).ToBeVisibleAsync();
        await dateTrigger.PressAsync("Escape");
        await Assertions.Expect(dateTrigger).ToBeFocusedAsync();
        await Assertions.Expect(dateTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Fact]
    public async Task PinnedVegaComputedStylesCoverEveryFormsAndDateSelectionComponent()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 1000 }, ColorScheme = ColorScheme.Dark, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(Url("dark", "rtl"));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();
        foreach (var selector in new[] { "[data-slot='input']", "[data-slot='native-select']", "[data-slot='select-trigger']", "[data-slot='input-group']", "[data-slot='date-picker-trigger']" })
        {
            var values = await page.Locator(selector).First.EvaluateAsync<string[]>("el => { const s=getComputedStyle(el); return [s.height,s.borderStyle,s.borderRadius,s.backgroundColor,s.color]; }");
            Assert.Equal("36px", values[0]);
            Assert.True(values[1] == "solid", $"{selector} computed {string.Join(", ", values)}");
            Assert.NotEqual("0px", values[2]);
            Assert.True(values[3] != "rgba(0, 0, 0, 0)", $"{selector} computed {string.Join(", ", values)}");
            Assert.True(values[3] != values[4], $"{selector} computed {string.Join(", ", values)}");
        }
        Assert.True((await page.Locator("[data-slot='textarea']").First.BoundingBoxAsync())!.Height >= 64);
        Assert.True((await page.Locator("[data-slot='calendar-day']").First.BoundingBoxAsync())!.Width >= 32);
    }

    [Theory]
    [InlineData(320, 568, 1)]
    [InlineData(640, 900, 2)]
    public async Task ResponsiveForcedColorsReducedMotionAndZoomRemainUsable(int width, int height, int zoom)
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = width / zoom, Height = height / zoom }, DeviceScaleFactor = zoom, HasTouch = true, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.EmulateMediaAsync(new() { ForcedColors = ForcedColors.Active, ReducedMotion = ReducedMotion.Reduce });
        await page.GotoAsync(Url("dark", "rtl"));
        await page.GetByTestId("forms-date-fixture").WaitForAsync();
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);
        await Assertions.Expect(page.Locator("input[name='budget']")).ToHaveAttributeAsync("inputmode", "decimal");
        await Assertions.Expect(page.Locator("[data-slot='input-otp']").First).ToHaveAttributeAsync("inputmode", "numeric");
        Assert.Equal("reduce", await page.EvaluateAsync<string>("matchMedia('(prefers-reduced-motion: reduce)').matches ? 'reduce' : 'motion'"));
        Assert.Equal("active", await page.EvaluateAsync<string>("matchMedia('(forced-colors: active)').matches ? 'active' : 'none'"));
    }

    private string Url(string theme, string direction) => new Uri(server.BaseUri, $"/components/forms-and-date-selection?theme={theme}&dir={direction}").ToString();
}
