using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeStudioWorkbenchContractTests : BunitContext
{
    public ThemeStudioWorkbenchContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void InspectorExposesOneLocalizedFiveSwatchPaletteSummary()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        var cut = Render<ThemeInspector>(parameters => parameters.Add(component => component.State, state));

        var summary = cut.Find("[data-testid='theme-palette-summary']");
        Assert.Equal(5, summary.QuerySelectorAll("[data-palette-summary-swatch]").Length);
        Assert.Contains("Active palette", summary.TextContent, StringComparison.Ordinal);
        Assert.Contains("Contrast ready", summary.TextContent, StringComparison.Ordinal);
        Assert.Equal("BUTTON", summary.QuerySelector("#theme-palette-customize")!.TagName);

        cut.Find("[data-testid='locale-thai']").Click();
        summary = cut.Find("[data-testid='theme-palette-summary']");
        Assert.Contains("ชุดสีที่ใช้งาน", summary.TextContent, StringComparison.Ordinal);
        Assert.Contains("ปรับแต่งชุดสี", summary.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Active palette", summary.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Customize palette", summary.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteWorkbenchRendersFiveAccessibleLocalizedAnchorEditors()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        state.Workbench.OpenPaletteWorkbench();
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));

        Assert.Single(cut.FindAll("[data-testid='theme-palette-workbench']"));
        Assert.Equal(5, cut.FindAll("[data-palette-anchor-role]").Count);
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-palette-generate']"));
        Assert.Single(cut.FindAll("[role='status'][aria-live='polite']"));
        Assert.All(Enum.GetValues<ShadcnPaletteAnchorRole>(), role =>
        {
            var editor = cut.Find($"[data-testid='theme-palette-anchor-{role.ToString().ToLowerInvariant()}']");
            Assert.NotNull(editor.QuerySelector("input[type='color'][aria-label]"));
            Assert.NotNull(editor.QuerySelector("input[type='text'][aria-label]"));
            Assert.Equal(2, editor.QuerySelectorAll("button[aria-label]").Length);
        });

        state.SetLocale(ThemeStudioLocale.Thai);
        cut.Render();
        var workbench = cut.Find("[data-testid='theme-palette-workbench']");
        Assert.Contains("สร้างชุดสี", workbench.TextContent, StringComparison.Ordinal);
        Assert.Contains("ความกลมกลืน", workbench.TextContent, StringComparison.Ordinal);
        Assert.Contains("แบรนด์", workbench.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Generate palette", workbench.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Harmony", workbench.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Brand", workbench.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchUsesOfficialBrandPackageControlsAndNamedLandmarks()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var layout = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioLayout.razor");
        var header = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "DocumentationHeader.razor");
        var sidebar = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioSidebar.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");

        Assert.Contains("<DocumentationHeader", layout, StringComparison.Ordinal);
        Assert.Contains("<ThemeStudioSidebar", page, StringComparison.Ordinal);
        Assert.Contains("<ThemeBento", page, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Theme preview\"", page, StringComparison.Ordinal);
        Assert.Contains("images/brand/MALIEV_BLACK.svg", header, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSidebarProvider", sidebar, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSidebar", sidebar, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSidebarRail", sidebar, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"theme-sidebar-collapse\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSelect", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("<Mud", header + sidebar + inspector, StringComparison.Ordinal);
        Assert.Contains("Label=\"Theme settings\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("id=\"theme-studio-sidebar-region\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"theme-studio-sidebar\"", sidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchExposesAllPreviewAndSettingsSectionsWithStableHooks()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");
        var combined = page + inspector;

        foreach (var hook in new[] { "preview-reduced-motion", "preview-high-contrast", "preview-animation-pause" })
            Assert.Contains($"data-testid=\"{hook}\"", combined, StringComparison.Ordinal);
        Assert.Contains("theme-icon-library-{library.ToString().ToLowerInvariant()}", inspector, StringComparison.Ordinal);

        Assert.Contains("viewport-{viewport.Id}", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("<PreviewToolbar", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeColorGroup", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeGeneratorOptions", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-inspector-nav", inspector, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"theme-radius-select\"", inspector, StringComparison.Ordinal);
        Assert.Contains("Icon=\"@DeviceIcon(viewport)\"", inspector, StringComparison.Ordinal);
        Assert.Contains("Icon=\"@ShuffleIcon\"", inspector, StringComparison.Ordinal);
        Assert.Contains("Icon=\"@UndoIcon\"", inspector, StringComparison.Ordinal);
        Assert.Contains("Icon=\"@RedoIcon\"", inspector, StringComparison.Ordinal);

        foreach (var section in new[] { "preview", "preset", "typography", "icons", "accessibility", "transfer" })
            Assert.Contains($"id=\"theme-settings-{section}\"", inspector, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchCssKeepsTheDocumentBoundedAndSupportsDrawerAccessibilityModes()
    {
        var root = FindRoot();
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains(".theme-preview-scope[data-preview-reduced-motion=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-scope[data-preview-high-contrast=\"true\"] .theme-bento", css, StringComparison.Ordinal);
        Assert.Contains(".documentation-trigger--theme-settings", css, StringComparison.Ordinal);
        Assert.Contains("padding-block: 0", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-sidebar-backdrop", css, StringComparison.Ordinal);
        Assert.Contains("position: fixed", css, StringComparison.Ordinal);
        Assert.Contains("overflow-x: clip", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains(":focus-visible", css, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteWorkbenchHasOneResponsiveDomContractBetweenSettingsAndPreview()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var component = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemePaletteWorkbench.razor");
        var script = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "theme-studio.js");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Equal(1, page.Split("<ThemePaletteWorkbench", StringSplitOptions.None).Length - 1);
        Assert.True(
            page.IndexOf("<ThemeStudioSidebar", StringComparison.Ordinal) < page.IndexOf("<ThemePaletteWorkbench", StringComparison.Ordinal) &&
            page.IndexOf("<ThemePaletteWorkbench", StringComparison.Ordinal) < page.IndexOf("class=\"theme-preview-region\"", StringComparison.Ordinal));
        Assert.Contains("data-palette-open", page, StringComparison.Ordinal);
        Assert.Contains("State.IsPointerInteractionActive", page, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"theme-palette-workbench\"", component, StringComparison.Ordinal);
        Assert.Contains("export function bindPaletteWorkbench(root, returnFocusId)", script, StringComparison.Ordinal);
        Assert.Contains("role", script, StringComparison.Ordinal);
        Assert.Contains("aria-modal", script, StringComparison.Ordinal);
        Assert.Contains("restoreFocus", script, StringComparison.Ordinal);
        Assert.Contains("contenteditable", script, StringComparison.Ordinal);
        Assert.Contains("[role=\"listbox\"]", script, StringComparison.Ordinal);
        Assert.Contains(".theme-studio-workbench[data-palette-open=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".theme-palette-workbench", css, StringComparison.Ordinal);
        Assert.Contains("block-size: 100dvh", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void TypographyUsesSelectableWeightsWithoutDuplicatingASettingsSpecimen()
    {
        var root = FindRoot();
        var typography = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeTypographyEditor.razor");

        Assert.DoesNotContain("theme-typography-specimen", typography, StringComparison.Ordinal);
        Assert.Contains("ShadcnSelect TValue=\"int\"", typography, StringComparison.Ordinal);
        Assert.Contains("State.SetTypographyRole", typography, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewThemeNeverMutatesTheCompanyShell()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var typography = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeTypographyEditor.razor");

        Assert.Contains("data-shadcn-theme=\"@(State.EffectiveDarkMode", page, StringComparison.Ordinal);
        Assert.Contains("dir=\"@(State.Direction", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellState.SetTheme", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellState.SetDirection", page, StringComparison.Ordinal);
        Assert.DoesNotContain("style=\"@TypographyStyle\"", typography, StringComparison.Ordinal);
    }

    private static string Read(string root, params string[] segments) =>
        File.ReadAllText(Path.Combine([root, .. segments]));

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }

    private sealed class NoOpStorage : IThemeStudioStorage
    {
        public ValueTask<ThemeStudioStorageResult> LoadAsync() =>
            ValueTask.FromResult(ThemeStudioStorageResult.Success(null));

        public ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document) =>
            ValueTask.FromResult(ThemeStudioStorageResult.Success(document));
    }
}
