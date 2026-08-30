using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DialogDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData(1280, 900, "light", "ltr", false)]
    [InlineData(390, 844, "dark", "rtl", false)]
    [InlineData(800, 800, "light", "ltr", true)]
    public async Task EditableDialogOpensFromItsTriggerAndKeepsModalInteractionPolished(
        int width,
        int height,
        string theme,
        string direction,
        bool forcedColors)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = forcedColors ? ForcedColors.Active : ForcedColors.None
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/dialog").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.EvaluateAsync(
            "({theme,direction}) => { const scope=document.querySelector('.shadcn-scope'); scope?.setAttribute('data-shadcn-theme', theme); scope?.setAttribute('dir', direction); document.documentElement.dir=direction; }",
            new { theme, direction });

        var trigger = page.Locator("[data-slot='dialog-trigger']");
        await Assertions.Expect(trigger).ToBeVisibleAsync();
        await Assertions.Expect(trigger).ToHaveAccessibleNameAsync("Edit profile");
        await Assertions.Expect(page.Locator("[data-slot='dialog-content']")).ToHaveCountAsync(0);

        await trigger.ClickAsync();
        var dialog = page.GetByRole(AriaRole.Dialog);
        await Assertions.Expect(dialog).ToBeVisibleAsync();
        await Assertions.Expect(dialog).ToHaveAttributeAsync("aria-modal", "true");
        var nameInput = dialog.GetByRole(AriaRole.Textbox, new() { Name = "Name", Exact = true });
        var usernameInput = dialog.GetByRole(AriaRole.Textbox, new() { Name = "Username", Exact = true });
        await Assertions.Expect(nameInput).ToHaveValueAsync("Narin Chaiyasit");
        await Assertions.Expect(usernameInput).ToHaveValueAsync("narin.c");
        await nameInput.FillAsync("Narin Chaiyasit, PE");

        var titleId = await dialog.GetAttributeAsync("aria-labelledby");
        var descriptionId = await dialog.GetAttributeAsync("aria-describedby");
        Assert.False(string.IsNullOrWhiteSpace(titleId));
        Assert.False(string.IsNullOrWhiteSpace(descriptionId));
        await Assertions.Expect(page.Locator($"#{titleId}")).ToHaveTextAsync("Edit profile");
        await Assertions.Expect(page.Locator($"#{descriptionId}")).ToContainTextAsync("project team");

        for (var index = 0; index < 7; index++)
        {
            await page.Keyboard.PressAsync("Tab");
            Assert.True(await dialog.EvaluateAsync<bool>("element => element.contains(document.activeElement)"));
        }

        var iconClose = dialog.Locator("[data-slot='dialog-close'][data-icon-only='true']");
        var footer = dialog.Locator("[data-slot='dialog-footer']");
        var iconBox = await iconClose.BoundingBoxAsync();
        var footerBox = await footer.BoundingBoxAsync();
        var dialogBox = await dialog.BoundingBoxAsync();
        Assert.NotNull(iconBox);
        Assert.NotNull(footerBox);
        Assert.NotNull(dialogBox);
        var closeOverlapsFooter =
            iconBox!.X < footerBox!.X + footerBox.Width &&
            iconBox.X + iconBox.Width > footerBox.X &&
            iconBox.Y < footerBox.Y + footerBox.Height &&
            iconBox.Y + iconBox.Height > footerBox.Y;
        Assert.False(closeOverlapsFooter, "The icon dismissal must not overlap footer actions.");
        Assert.True(dialogBox!.X >= 8 && dialogBox.X + dialogBox.Width <= width - 8, "Dialog must stay inside the viewport.");
        Assert.Equal("none", await dialog.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        if (forcedColors)
            Assert.NotEqual("0px", await dialog.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));

        var axe = await dialog.RunAxe();
        Assert.True(!axe.Violations.Any(), $"Dialog axe violations: {string.Join("; ", axe.Violations.Select(violation => $"{violation.Id}: {string.Join(", ", violation.Nodes.Select(node => string.Join(" ", node.Target)))}"))}");

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(dialog).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).Not.ToHaveAttributeAsync("aria-hidden", "true");
        Assert.False(await trigger.EvaluateAsync<bool>("element => element.inert"));
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await trigger.ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await page.GetByTestId("control-dialog-variant").CheckAsync();
        await trigger.ClickAsync();
        await Assertions.Expect(page.Locator("[data-slot='dialog-portal']"))
            .ToHaveAttributeAsync("data-shadcn-dialog-ready", "");
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog)).Not.ToHaveAttributeAsync("aria-modal", "true");
        await Assertions.Expect(page.Locator("#preview .component-code pre")).ToContainTextAsync("Modal=\"false\"");
        await page.EvaluateAsync("""
            () => {
                const target = document.createElement('button');
                target.dataset.testid = 'non-modal-outside-target';
                target.style.cssText = 'position:fixed;inset:0 auto auto 0;width:12px;height:12px;z-index:2147483647';
                target.addEventListener('click', () => target.dataset.clicked = 'true');
                document.body.append(target);
            }
            """);
        await page.Mouse.ClickAsync(6, 6);
        await Assertions.Expect(page.GetByTestId("non-modal-outside-target")).ToHaveAttributeAsync("data-clicked", "true");
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);

        Assert.Empty(errors);
    }
}
