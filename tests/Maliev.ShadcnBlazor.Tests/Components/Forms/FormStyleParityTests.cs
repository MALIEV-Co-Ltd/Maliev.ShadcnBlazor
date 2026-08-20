namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class FormStyleParityTests
{
    [Fact]
    public void FormsStylesheetIsLoadedByShowcase()
    {
        var root = FindRoot();
        var indexPath = Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html");

        Assert.Contains(
            "_content/Maliev.ShadcnBlazor/css/shadcn-forms.css",
            File.ReadAllText(indexPath),
            StringComparison.Ordinal);
    }

    [Fact]
    public void FormsStylesCoverPinnedGeometryInteractionAndPlatformModes()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-forms.css"));

        Assert.Contains("height: var(--shadcn-control-height)", css, StringComparison.Ordinal);
        Assert.Contains("--shadcn-calendar-cell: 2rem", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-calendar-week { margin-top: calc(.375rem * var(--shadcn-spacing-multiplier));", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-input-otp-separator { display: flex; width: 1rem; height: 1rem;", css, StringComparison.Ordinal);
        Assert.Contains("data-range-middle=\"true\"", css, StringComparison.Ordinal);
        Assert.Contains("data-range-complete=\"true\"", css, StringComparison.Ordinal);
        Assert.Contains("data-range-start=\"true\"][data-range-end=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains("[dir=\"rtl\"]", css, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 40rem)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("forced-color-adjust: none", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-calendar-day[data-range-middle=\"true\"]", css, StringComparison.Ordinal);
    }

    [Fact]
    public void FormsStylesKeepClearActionsInsideFieldsAndUseLogicalOtpGeometry()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-forms.css"));

        Assert.Contains(".shadcn-select[data-clearable=\"true\"] .shadcn-select-trigger", css, StringComparison.Ordinal);
        Assert.Contains("padding-inline-end: calc(3.25rem", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-select[data-clearable=\"true\"] .shadcn-select-trigger-icon", css, StringComparison.Ordinal);
        Assert.Contains("grid-area: 1 / 1", css, StringComparison.Ordinal);
        Assert.Contains("margin-inline-end: calc(2rem", css, StringComparison.Ordinal);
        Assert.Contains("border-inline-end: 1px solid var(--shadcn-input)", css, StringComparison.Ordinal);
        Assert.Contains("border-inline-start: 1px solid var(--shadcn-input)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("field-sizing: content", css, StringComparison.Ordinal);
        Assert.Contains(":where(.shadcn-scope, .shadcn-overlay-scope)[data-shadcn-theme=\"dark\"]:is(.shadcn-input", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowcaseReferencePairsPinSelectAndDatePickerTriggerGeometry()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));

        Assert.Contains(".forms-reference-select [data-slot=\"select-trigger\"] { width: 140.047px;", css, StringComparison.Ordinal);
        Assert.Contains(".forms-reference-date-picker [data-slot=\"date-picker-trigger\"] { width: 145.984px;", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
