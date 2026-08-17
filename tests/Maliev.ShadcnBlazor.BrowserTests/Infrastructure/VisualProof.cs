using Microsoft.Playwright;
using System.Text.Json;

namespace Maliev.ShadcnBlazor.BrowserTests.Infrastructure;

internal sealed record VisualProofMode(string Name, bool Dark, ViewportSize Viewport)
{
    public static VisualProofMode DesktopLight { get; } = new(
        "desktop-light",
        Dark: false,
        new ViewportSize { Width = 1280, Height = 900 });

    public static VisualProofMode MobileDarkRtl { get; } = new(
        "mobile-dark-rtl",
        Dark: true,
        new ViewportSize { Width = 390, Height = 844 });
}

internal static class ComponentCatalogProof
{
    public static IReadOnlyList<string> LoadCompleted(string root)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "docs", "component-catalog.json")));
        return document.RootElement.GetProperty("components")
            .EnumerateArray()
            .Where(component => string.Equals(component.GetProperty("status").GetString(), "complete", StringComparison.Ordinal))
            .Select(component => component.GetProperty("slug").GetString() ?? throw new InvalidDataException("Catalog component is missing a slug."))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<string> SelectRequested(IReadOnlyList<string> completed) =>
        SelectRequested(completed, Environment.GetEnvironmentVariable("SHADCN_VISUAL_PROOF_SLUGS"));

    internal static IReadOnlyList<string> SelectRequested(IReadOnlyList<string> completed, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return completed;

        var requested = value
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
        var unknown = requested.Except(completed, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (unknown.Length > 0)
            throw new InvalidOperationException($"Unknown component visual-proof slug(s): {string.Join(", ", unknown)}.");

        return completed.Where(requested.Contains).ToArray();
    }
}

internal static class VisualProof
{
    private const double MismatchThreshold = 0.001;

    public static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate solution root.");
    }

    public static string BaselineDirectory(string root) =>
        Path.Combine(root, "docs", "evidence", "component-catalog-baselines");

    public static bool UpdateEnabled => string.Equals(
        Environment.GetEnvironmentVariable("SHADCN_UPDATE_VISUAL_BASELINES"),
        "1",
        StringComparison.Ordinal);

    public static async Task CompareOrUpdateAsync(
        IPage page,
        string slug,
        string mode,
        byte[] actual)
    {
        var root = FindRoot();
        var baselineDirectory = BaselineDirectory(root);
        var baselinePath = Path.Combine(baselineDirectory, $"{slug}--{mode}.png");
        var diagnosticDirectory = Path.Combine(root, "artifacts", "visual-proof", slug, mode);
        Directory.CreateDirectory(diagnosticDirectory);
        await File.WriteAllBytesAsync(Path.Combine(diagnosticDirectory, "actual.png"), actual);

        if (UpdateEnabled)
        {
            Directory.CreateDirectory(baselineDirectory);
            await File.WriteAllBytesAsync(baselinePath, actual);
        }

        Assert.True(File.Exists(baselinePath), $"Missing reviewed visual baseline: {baselinePath}");
        var expected = await File.ReadAllBytesAsync(baselinePath);
        var comparison = await ComparePngsAsync(page, expected, actual);
        if (IsMatch(comparison))
            return;

        var diffPath = Path.Combine(diagnosticDirectory, "diff.png");
        if (comparison.Diff is not null)
            await File.WriteAllBytesAsync(diffPath, Convert.FromBase64String(comparison.Diff));

        Assert.Fail(
            $"{slug} {mode} visual mismatch {comparison.DifferentPixels}/" +
            $"{comparison.CanvasWidth * comparison.CanvasHeight} pixels ({comparison.Ratio:P4}) " +
            $"exceeds {MismatchThreshold:P2}. Actual and diff: {diagnosticDirectory}");
    }

    private static bool IsMatch(VisualComparison comparison) =>
        comparison.ExpectedWidth == comparison.ActualWidth &&
        comparison.ExpectedHeight == comparison.ActualHeight &&
        comparison.Ratio <= MismatchThreshold;

    private static Task<VisualComparison> ComparePngsAsync(IPage page, byte[] expected, byte[] actual) =>
        page.EvaluateAsync<VisualComparison>("""
            async ({ expected, actual }) => {
                const decode = async value => createImageBitmap(await (await fetch(`data:image/png;base64,${value}`)).blob());
                const expectedImage = await decode(expected);
                const actualImage = await decode(actual);
                const canvas = document.createElement('canvas');
                canvas.width = Math.max(expectedImage.width, actualImage.width);
                canvas.height = Math.max(expectedImage.height, actualImage.height);
                const context = canvas.getContext('2d', { willReadFrequently: true });
                context.clearRect(0, 0, canvas.width, canvas.height);
                context.drawImage(expectedImage, 0, 0);
                const expectedPixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                context.clearRect(0, 0, canvas.width, canvas.height);
                context.drawImage(actualImage, 0, 0);
                const actualPixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                const diff = context.createImageData(canvas.width, canvas.height);
                let differentPixels = 0;
                for (let offset = 0; offset < actualPixels.length; offset += 4) {
                    const delta = Math.max(
                        Math.abs(expectedPixels[offset] - actualPixels[offset]),
                        Math.abs(expectedPixels[offset + 1] - actualPixels[offset + 1]),
                        Math.abs(expectedPixels[offset + 2] - actualPixels[offset + 2]),
                        Math.abs(expectedPixels[offset + 3] - actualPixels[offset + 3]));
                    if (delta > 8) {
                        differentPixels++;
                        diff.data[offset] = 255;
                        diff.data[offset + 3] = 255;
                    }
                }
                context.putImageData(diff, 0, 0);
                return {
                    expectedWidth: expectedImage.width,
                    expectedHeight: expectedImage.height,
                    actualWidth: actualImage.width,
                    actualHeight: actualImage.height,
                    canvasWidth: canvas.width,
                    canvasHeight: canvas.height,
                    differentPixels,
                    ratio: differentPixels / (canvas.width * canvas.height),
                    diff: canvas.toDataURL('image/png').split(',')[1]
                };
            }
            """, new
        {
            expected = Convert.ToBase64String(expected),
            actual = Convert.ToBase64String(actual),
        });

    private sealed class VisualComparison
    {
        public int ExpectedWidth { get; set; }
        public int ExpectedHeight { get; set; }
        public int ActualWidth { get; set; }
        public int ActualHeight { get; set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }
        public int DifferentPixels { get; set; }
        public double Ratio { get; set; }
        public string? Diff { get; set; }
    }
}
