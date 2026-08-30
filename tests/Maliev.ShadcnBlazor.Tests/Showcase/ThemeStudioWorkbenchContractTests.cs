using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Fonts;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeStudioWorkbenchContractTests : BunitContext
{
    public ThemeStudioWorkbenchContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
        Services.AddSingleton(new HttpClient(new FontCatalogHandler())
        {
            BaseAddress = new Uri("https://showcase.invalid/"),
        });
        Services.AddSingleton<GoogleFontCatalogService>();
    }

    [Fact]
    public void InspectorExposesOneLocalizedFiveSwatchPaletteSummary()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        var cut = Render<ThemeInspector>(parameters => parameters.Add(component => component.State, state));

        var summary = cut.Find("[data-testid='theme-palette-summary']");
        Assert.Equal("theme-visual-treatment-controls", summary.ParentElement?.GetAttribute("data-testid"));
        Assert.NotNull(summary.PreviousElementSibling?.QuerySelector("[data-testid='theme-color-treatment']"));
        Assert.Equal(5, summary.QuerySelectorAll("[data-palette-summary-swatch]").Length);
        Assert.Contains("Active palette", summary.TextContent, StringComparison.Ordinal);
        Assert.Contains("Needs review", summary.TextContent, StringComparison.Ordinal);
        Assert.Equal("BUTTON", summary.QuerySelector("[data-testid='theme-palette-customize']")!.TagName);

        cut.Find("[data-testid='locale-thai']").Click();
        summary = cut.Find("[data-testid='theme-palette-summary']");
        Assert.Contains("ชุดสีที่ใช้งาน", summary.TextContent, StringComparison.Ordinal);
        Assert.Contains("ปรับแต่งชุดสี", summary.TextContent, StringComparison.Ordinal);
        Assert.Contains("ต้องตรวจสอบ", summary.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Active palette", summary.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Customize palette", summary.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteCustomizationExpandsInsideTheSidebarSummary()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        var cut = Render<ThemePaletteSummary>(parameters => parameters.Add(component => component.State, state));

        Assert.Equal("closed", cut.Find("[data-testid='theme-palette-summary']").GetAttribute("data-state"));

        cut.Find("[data-testid='theme-palette-customize']").Click();

        var summary = cut.Find("[data-testid='theme-palette-summary']");
        var workbench = summary.QuerySelector("[data-testid='theme-palette-workbench']");
        Assert.NotNull(workbench);
        Assert.Equal("true", cut.Find("[data-testid='theme-palette-customize']").GetAttribute("aria-expanded"));
        Assert.NotNull(workbench!.QuerySelector("[data-testid='theme-palette-anchor-brand']"));
        Assert.Contains("Main color", workbench.TextContent, StringComparison.Ordinal);
        Assert.Contains("Generate from main color", workbench.TextContent, StringComparison.Ordinal);
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
            Assert.Equal(role == ShadcnPaletteAnchorRole.Brand ? 1 : 2, editor.QuerySelectorAll("button[aria-label]").Length);
        });

        state.SetLocale(ThemeStudioLocale.Thai);
        cut.Render();
        var workbench = cut.Find("[data-testid='theme-palette-workbench']");
        Assert.Contains("สร้างจากสีหลัก", workbench.TextContent, StringComparison.Ordinal);
        Assert.Contains("ความกลมกลืน", workbench.TextContent, StringComparison.Ordinal);
        Assert.Contains("แบรนด์", workbench.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Generate palette", workbench.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Harmony", workbench.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Brand", workbench.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void LegacyAnchorLockIsProjectedInTheWorkbenchWithoutChangingCanonicalBytes()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        var legacy = state.CreateDocument() with
        {
            Palette = new ShadcnPaletteRecipe(
                ShadcnPaletteRecipe.LegacyAlgorithmVersion,
                42,
                "neutral",
                [])
        };
        Assert.True(state.ImportDocument(legacy));
        var canonical = state.SerializeDocument();
        state.Workbench.OpenPaletteWorkbench();
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));
        var lockButton = cut.Find("[data-testid='theme-palette-anchor-support'] [data-palette-lock]");
        Assert.Equal("false", lockButton.GetAttribute("aria-pressed"));

        lockButton.Click();

        lockButton = cut.Find("[data-testid='theme-palette-anchor-support'] [data-palette-lock]");
        Assert.Equal("true", lockButton.GetAttribute("aria-pressed"));
        Assert.Equal(ShadcnPaletteRecipe.LegacyAlgorithmVersion, state.Document.Palette.AlgorithmVersion);
        Assert.Equal(canonical, state.SerializeDocument());

        lockButton.Click();
        Assert.Equal("false", cut.Find("[data-testid='theme-palette-anchor-support'] [data-palette-lock]")
            .GetAttribute("aria-pressed"));
        Assert.Equal(canonical, state.SerializeDocument());
    }

    [Fact]
    public void SummaryAndWorkbenchShareFullLocalizedContrastReadiness()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        Assert.True(state.GeneratePalette(117));
        state.Workbench.OpenPaletteWorkbench();
        var summary = Render<ThemePaletteSummary>(parameters => parameters.Add(component => component.State, state));
        var workbench = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));

        Assert.Equal("ready", summary.Find("[data-palette-contrast]").GetAttribute("data-palette-contrast"));
        Assert.Equal("ready", workbench.Find("[data-palette-contrast]").GetAttribute("data-palette-contrast"));
        Assert.Contains("Contrast ready", summary.Markup, StringComparison.Ordinal);
        Assert.Contains("Contrast ready", workbench.Markup, StringComparison.Ordinal);

        state.SetToken(ThemeStudioScheme.Light, "border", state.Applied.Light.Background);
        summary.Render();
        workbench.Render();

        Assert.Contains(state.Validation.ContrastResults, result =>
            result.Kind == ShadcnContrastKind.Boundary && !result.Passes);
        Assert.Equal("review", summary.Find("[data-palette-contrast]").GetAttribute("data-palette-contrast"));
        Assert.Equal("review", workbench.Find("[data-palette-contrast]").GetAttribute("data-palette-contrast"));
        Assert.Contains("Needs review", summary.Markup, StringComparison.Ordinal);
        Assert.Contains("Needs review", workbench.Markup, StringComparison.Ordinal);
        var englishDetails = workbench.FindAll(".theme-palette-workbench__diagnostics li");
        Assert.Equal(state.PaletteReviewDiagnostics.Count, englishDetails.Count);
        Assert.Contains(englishDetails, item =>
            item.TextContent.Contains("light.border", StringComparison.Ordinal) &&
            item.TextContent.Contains("3:1", StringComparison.Ordinal));
        Assert.Equal(
            state.PaletteReviewDiagnostics.Count,
            state.PaletteReviewDiagnostics.DistinctBy(message => (message.Code, message.Path, message.Message)).Count());

        state.SetLocale(ThemeStudioLocale.Thai);
        summary.Render();
        workbench.Render();
        Assert.Contains("ต้องตรวจสอบ", summary.Markup, StringComparison.Ordinal);
        Assert.Contains("ต้องตรวจสอบ", workbench.Markup, StringComparison.Ordinal);
        var thaiDetails = workbench.Find(".theme-palette-workbench__diagnostics").TextContent;
        Assert.Contains("light.border", thaiDetails, StringComparison.Ordinal);
        Assert.Contains("3:1", thaiDetails, StringComparison.Ordinal);
        Assert.Contains("คอนทราสต์", thaiDetails, StringComparison.Ordinal);
        Assert.DoesNotContain("Boundary contrast", thaiDetails, StringComparison.Ordinal);

        Assert.True(state.Undo());
        summary.Render();
        workbench.Render();
        Assert.Equal("ready", summary.Find("[data-palette-contrast]").GetAttribute("data-palette-contrast"));
        Assert.Equal("ready", workbench.Find("[data-palette-contrast]").GetAttribute("data-palette-contrast"));
        Assert.Contains("คอนทราสต์พร้อมใช้งาน", summary.Markup, StringComparison.Ordinal);
        Assert.Contains("คอนทราสต์พร้อมใช้งาน", workbench.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteAnchorTextDraftDoesNotMutateStateOrHistoryUntilCommit()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        state.Workbench.OpenPaletteWorkbench();
        var changed = 0;
        state.Changed += (_, _) => changed++;
        var original = state.PaletteAnchors.Brand;
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));
        var input = cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");

        input.Input("#dc2626");

        Assert.Equal("#dc2626", input.GetAttribute("value"));
        Assert.Equal(original, state.PaletteAnchors.Brand);
        Assert.False(state.CanUndo);
        Assert.Equal(0, changed);

        input.Change("#dc2626");

        Assert.NotEqual(original, state.PaletteAnchors.Brand);
        Assert.StartsWith("oklch(", state.PaletteAnchors.Brand, StringComparison.Ordinal);
        Assert.True(state.CanUndo);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void InvalidPaletteAnchorCommitIsAssociatedWithLocalizedActionableHelp()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        state.Workbench.OpenPaletteWorkbench();
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));
        var input = cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");

        input.Input("not-a-color");
        Assert.Null(input.GetAttribute("aria-invalid"));
        input.Change("not-a-color");

        input = cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        var describedBy = input.GetAttribute("aria-describedby");
        Assert.Equal("theme-palette-anchor-brand-error", describedBy);
        var error = cut.Find($"#{describedBy}");
        Assert.Contains("Enter a color as", error.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("กรอกค่าสี", error.TextContent, StringComparison.Ordinal);

        state.SetLocale(ThemeStudioLocale.Thai);
        cut.Render();
        error = cut.Find($"#{describedBy}");
        Assert.Contains("กรอกค่าสี", error.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Enter a color as", error.TextContent, StringComparison.Ordinal);
        Assert.NotEmpty(cut.FindAll("[data-testid='theme-palette-workbench'] .theme-palette-workbench__diagnostics li"));
        Assert.DoesNotContain("Brand must", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectedAnchorDraftCanBeRetriedAfterPaletteConstraintsChange()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        state.SetToken(ThemeStudioScheme.Light, "primary", state.Applied.Light.Background);
        state.SetToken(ThemeStudioScheme.Light, "primaryForeground", state.Applied.Light.Background);
        state.SetPaletteLock(ThemeStudioScheme.Light, "primary", true);
        state.SetPaletteLock(ThemeStudioScheme.Light, "primaryForeground", true);
        state.Workbench.OpenPaletteWorkbench();
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));
        var input = cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");

        input.Input("#2563eb");
        input.Change("#2563eb");
        Assert.NotEmpty(state.PaletteDiagnostics);
        Assert.Equal("true", cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']")
            .GetAttribute("aria-invalid"));

        state.SetPaletteLock(ThemeStudioScheme.Light, "primary", false);
        state.SetPaletteLock(ThemeStudioScheme.Light, "primaryForeground", false);
        cut.Render();
        input = cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");
        input.Change("#2563eb");

        input = cut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");
        Assert.Null(input.GetAttribute("aria-invalid"));
        Assert.StartsWith("oklch(", state.PaletteAnchors.Brand, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectedPaletteGenerationAnnouncesAnErrorWithoutChangingTheDocument()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        state.SetToken(ThemeStudioScheme.Light, "primary", state.Applied.Light.Background);
        state.SetToken(ThemeStudioScheme.Light, "primaryForeground", state.Applied.Light.Background);
        state.SetPaletteLock(ThemeStudioScheme.Light, "primary", true);
        state.SetPaletteLock(ThemeStudioScheme.Light, "primaryForeground", true);
        var before = state.SerializeDocument();
        state.Workbench.OpenPaletteWorkbench();
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));

        cut.Find("[data-testid='theme-palette-generate']").Click();

        Assert.Equal(before, state.SerializeDocument());
        Assert.Equal("Palette error", cut.Find("[data-testid='theme-palette-status']").TextContent);
        Assert.Contains(state.PaletteDiagnostics, message => message.Code == "palette-locked-constraint");
        Assert.Contains(
            "Contrast between light.primaryForeground and light.primary is 1:1; 4.5:1 is required.",
            cut.Find("[data-testid='theme-palette-workbench'] .theme-palette-workbench__diagnostics").TextContent,
            StringComparison.Ordinal);

        state.SetLocale(ThemeStudioLocale.Thai);
        cut.Render();
        var thaiDiagnostics = cut.Find("[data-testid='theme-palette-workbench'] .theme-palette-workbench__diagnostics").TextContent;
        Assert.Contains(
            "คอนทราสต์ระหว่าง light.primaryForeground และ light.primary เท่ากับ 1:1 โดยต้องมีอย่างน้อย 4.5:1",
            thaiDiagnostics,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Contrast between", thaiDiagnostics, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RapidGenerationRequestsAreCoalescedBeforeTheNextLocalizedGeneration()
    {
        var state = new ThemeStudioState(new NoOpStorage());
        Assert.True(state.SetPaletteAnchor(ShadcnPaletteAnchorRole.Brand, "#2563eb"));
        state.Workbench.OpenPaletteWorkbench();
        var cut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, state));
        var generate = cut.Find("[data-testid='theme-palette-generate']");

        generate.Click();
        var first = cut.Find("[data-testid='theme-palette-status']").TextContent;
        Assert.Equal($"Palette generated: Seed {state.Document.Palette.Seed}", first);

        generate.Click();
        var second = cut.Find("[data-testid='theme-palette-status']").TextContent;
        Assert.Equal(first, second);

        await Task.Delay(550);
        generate.Click();
        var third = cut.Find("[data-testid='theme-palette-status']").TextContent;
        Assert.Equal($"Palette generated: Seed {state.Document.Palette.Seed}", third);
        Assert.NotEqual(first, third);

        state.SetLocale(ThemeStudioLocale.Thai);
        cut.Render();
        await Task.Delay(550);
        cut.Find("[data-testid='theme-palette-generate']").Click();
        Assert.Equal(
            $"สร้างชุดสีแล้ว: ซีด {state.Document.Palette.Seed}",
            cut.Find("[data-testid='theme-palette-status']").TextContent);
    }

    [Fact]
    public void UnknownDiagnosticThatLooksLikeContrastUsesTheLocalizedSafeFallback()
    {
        var diagnostic = new ShadcnThemeValidationMessage(
            "palette-unknown-diagnostic",
            "light.synthetic",
            "Contrast between light.syntheticForeground and light.synthetic is 1:1; 4.5:1 is required.");

        var englishMessage = ThemeStudioPaletteCopy.English.DiagnosticMessage(diagnostic);
        var thaiMessage = ThemeStudioPaletteCopy.Thai.DiagnosticMessage(diagnostic);

        Assert.Equal("Palette error: light.synthetic", englishMessage);
        Assert.DoesNotContain("Contrast between", englishMessage, StringComparison.Ordinal);
        Assert.Equal("ข้อผิดพลาดของชุดสี: light.synthetic", thaiMessage);
        Assert.DoesNotContain("คอนทราสต์ระหว่าง", thaiMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteAnchorEnterAndBlurEachCommitOnlyOnce()
    {
        var enterState = new ThemeStudioState(new NoOpStorage());
        enterState.Workbench.OpenPaletteWorkbench();
        var enterChanges = 0;
        enterState.Changed += (_, _) => enterChanges++;
        var enterCut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, enterState));
        var enterInput = enterCut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");
        enterInput.Input("#dc2626");
        enterInput.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        enterInput = enterCut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");
        enterInput.Change("#dc2626");
        enterInput.Blur();
        Assert.Equal(1, enterChanges);

        var blurState = new ThemeStudioState(new NoOpStorage());
        blurState.Workbench.OpenPaletteWorkbench();
        var blurChanges = 0;
        blurState.Changed += (_, _) => blurChanges++;
        var blurCut = Render<ThemePaletteWorkbench>(parameters => parameters.Add(component => component.State, blurState));
        var blurInput = blurCut.Find("[data-testid='theme-palette-anchor-brand'] input[type='text']");
        blurInput.Input("#dc2626");
        blurInput.Blur();
        Assert.Equal(1, blurChanges);
        Assert.True(blurState.CanUndo);
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
    public void PaletteWorkbenchIsNestedInTheSidebarWithoutModalBindingMachinery()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var component = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemePaletteWorkbench.razor");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.DoesNotContain("<ThemePaletteWorkbench", page, StringComparison.Ordinal);
        Assert.Contains("<ThemePaletteWorkbench State=\"State\"", Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemePaletteSummary.razor"), StringComparison.Ordinal);
        Assert.DoesNotContain("data-palette-open", page, StringComparison.Ordinal);
        Assert.Contains("State.IsPointerInteractionActive", page, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"theme-palette-workbench\"", component, StringComparison.Ordinal);
        Assert.DoesNotContain("IJSRuntime", component, StringComparison.Ordinal);
        Assert.Contains("GeneratePaletteFromMainColor", component, StringComparison.Ordinal);
        Assert.DoesNotContain(".theme-studio-workbench[data-palette-open=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains(".theme-palette-workbench", css, StringComparison.Ordinal);
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
        Assert.Contains("if (Active && _catalog is null)", typography, StringComparison.Ordinal);
        Assert.DoesNotContain("OnInitializedAsync", typography, StringComparison.Ordinal);
    }

    [Fact]
    public void NativePalettePickerHasFocusInputChangeBlurAndPointerLifecycleFallbacks()
    {
        var root = FindRoot();
        var editor = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemePaletteAnchorEditor.razor");

        Assert.Contains("@onfocus=\"BeginPickerInteraction\"", editor, StringComparison.Ordinal);
        Assert.Contains("@oninput=\"SetPickerValue\"", editor, StringComparison.Ordinal);
        Assert.Contains("@onchange=\"FinalizePickerInteraction\"", editor, StringComparison.Ordinal);
        Assert.Contains("@onblur=\"FinalizePickerInteraction\"", editor, StringComparison.Ordinal);
        Assert.Contains("@onpointerup=\"FinalizePickerInteraction\"", editor, StringComparison.Ordinal);
        Assert.Contains("if (_pickerInteractionActive)", editor, StringComparison.Ordinal);
        Assert.Contains("FinalizePickerInteraction();", editor, StringComparison.Ordinal);
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

    private sealed class FontCatalogHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
    }
}
