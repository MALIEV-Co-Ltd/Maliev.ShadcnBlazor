using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DropzoneShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task DrawingPackageDropzoneSupportsNativeSelectionDragStateValidationAndSourceSync()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/dropzone").ToString());

        var root = page.Locator("[data-slot='dropzone']");
        var input = root.Locator("[data-slot='dropzone-input']");
        await Assertions.Expect(root).ToBeVisibleAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("accept", ".step,.stp,.pdf");
        await Assertions.Expect(input).ToHaveAttributeAsync("multiple", "");

        await input.FocusAsync();
        await Assertions.Expect(input).ToBeFocusedAsync();
        await root.EvaluateAsync("element => element.dispatchEvent(new DragEvent('dragenter', { bubbles: true, cancelable: true, dataTransfer: new DataTransfer() }))");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-state", "dragging");
        await root.EvaluateAsync("element => element.dispatchEvent(new DragEvent('dragleave', { bubbles: true, cancelable: true, dataTransfer: new DataTransfer() }))");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-state", "idle");

        await input.SetInputFilesAsync(new FilePayload
        {
            Name = "fixture.step",
            MimeType = "model/step",
            Buffer = [1, 2, 3, 4]
        });
        await Assertions.Expect(root.GetByRole(AriaRole.Status)).ToContainTextAsync("1 file selected");
        await Assertions.Expect(root.Locator("[data-slot='dropzone-errors']")).ToHaveCountAsync(0);

        await input.SetInputFilesAsync(new FilePayload
        {
            Name = "notes.txt",
            MimeType = "text/plain",
            Buffer = [1, 2, 3]
        });
        await Assertions.Expect(root).ToHaveAttributeAsync("data-state", "invalid");
        await Assertions.Expect(root.GetByRole(AriaRole.Alert)).ToContainTextAsync("notes.txt is not an accepted file type.");
        var errorId = await root.GetByRole(AriaRole.Alert).GetAttributeAsync("id");
        Assert.False(string.IsNullOrWhiteSpace(errorId));
        Assert.Contains(errorId!, await input.GetAttributeAsync("aria-describedby"), StringComparison.Ordinal);

        await page.GetByTestId("control-dropzone-multiple").UncheckAsync();
        await page.GetByTestId("control-dropzone-loading").CheckAsync();
        await Assertions.Expect(root).ToHaveAttributeAsync("data-state", "loading");
        await Assertions.Expect(root).ToHaveAttributeAsync("aria-busy", "true");
        await Assertions.Expect(input).ToBeDisabledAsync();

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("Multiple=\"false\"");
        await Assertions.Expect(source).ToContainTextAsync("MaxFiles=\"1\"");
        await Assertions.Expect(source).ToContainTextAsync("Loading=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("SelectionChanged=\"HandleSelection\"");
    }

    [Fact]
    public async Task DropzoneRemainsContainedAndUnderstandableInMobileDarkRtlForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/dropzone").ToString());
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var root = page.Locator("[data-slot='dropzone']");
        await Assertions.Expect(root).ToBeVisibleAsync();
        await Assertions.Expect(root).ToContainTextAsync("Drop STEP or PDF drawings here, or choose files");
        await Assertions.Expect(root).ToContainTextAsync("Files remain caller-owned until you upload them.");
        Assert.Equal("rtl", await root.EvaluateAsync<string>("element => getComputedStyle(element).direction"));
        Assert.Equal("1px", await root.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.InRange(
            await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"),
            0,
            1);
    }
}
