using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests.Infrastructure;

internal static class DossierControlExtensions
{
    internal static async Task ChooseOptionAsync(this IPage page, string testId, string value)
    {
        var trigger = page.GetByTestId(testId);
        var select = trigger.Locator("xpath=ancestor-or-self::*[@data-slot='select'][1]");
        await trigger.ClickAsync();
        var option = select.Locator($"[role='option'][data-value='{value}']");
        var label = await option.EvaluateAsync<string>("""
            element => {
                const clone = element.cloneNode(true);
                clone.querySelector('.shadcn-select-item-indicator')?.remove();
                return clone.textContent.trim();
            }
            """);
        await option.ClickAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(trigger.Locator("[data-slot='select-value']")).ToHaveTextAsync(label);
    }
}
