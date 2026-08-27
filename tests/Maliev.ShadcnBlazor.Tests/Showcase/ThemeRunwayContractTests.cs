using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Runway;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeBentoContractTests
{
    [Theory]
    [InlineData(ThemeStudioIconLibrary.Lucide, "lucide")]
    [InlineData(ThemeStudioIconLibrary.Phosphor, "phosphor")]
    [InlineData(ThemeStudioIconLibrary.Tabler, "tabler")]
    [InlineData(ThemeStudioIconLibrary.Hugeicons, "hugeicons")]
    public void EveryCompanionPackageResolvesTheCuratedSemanticIconSet(ThemeStudioIconLibrary library, string expectedLibrary)
    {
        foreach (var workflow in new[] { "production-analytics", "quality-alert", "inspection-camera", "api-credentials", "quotation-data-table", "drawing-attachment", "machine-cell", "assistant-conversation", "operator-profile", "project-questionnaire", "shipping-handoff", "assigned-reviewers", "quotation-files" })
            Assert.Equal(expectedLibrary, ThemeStudioIconResolver.Resolve(library, workflow).Library);
    }

    [Fact]
    public void PreviewUsesOneResponsiveInteractiveBentoGridWithRealCardBorders()
    {
        var root = FindRoot();
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("data-testid=\"theme-bento\"", bento, StringComparison.Ordinal);
        Assert.Contains("<ShadcnBentoGrid Class=\"theme-bento__grid\"", bento, StringComparison.Ordinal);
        Assert.Contains("<ShadcnBentoItem", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("mirror", bento, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inert", bento, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-runway-track", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("column-count:", css, StringComparison.Ordinal);
        Assert.Contains("padding-block: 1rem", css, StringComparison.Ordinal);
        Assert.Contains("padding-inline: 0", css, StringComparison.Ordinal);
        Assert.Contains("--theme-use-case-boundary:", css, StringComparison.Ordinal);
        Assert.Contains("border: 1px solid transparent", css, StringComparison.Ordinal);
        Assert.Contains("background-clip: padding-box", css, StringComparison.Ordinal);
        Assert.Contains(".theme-use-case-card::after", css, StringComparison.Ordinal);
        Assert.Contains("border: var(--shadcn-style-border-width, 1px) solid var(--theme-use-case-boundary)", css, StringComparison.Ordinal);
        Assert.Contains("border-radius: inherit", css, StringComparison.Ordinal);
        Assert.Contains("var(--shadcn-style-shadow, none)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("grid-auto-flow: dense", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-region { min-inline-size: 0;", css, StringComparison.Ordinal);
        Assert.Contains("border: 0; border-radius: 0; background: transparent", css, StringComparison.Ordinal);
        Assert.Contains("Gap=\"1rem\"", bento, StringComparison.Ordinal);
    }

    [Fact]
    public void BentoUsesOnlyDedicatedCuratedWorkflowCards()
    {
        var root = FindRoot();
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        Assert.Contains("IThemeUseCaseRegistry", bento, StringComparison.Ordinal);
        Assert.Contains("@foreach (var card in FilteredCards)", bento, StringComparison.Ordinal);
        Assert.Contains("FilteredCards => Registry.All", bento, StringComparison.Ordinal);
        Assert.Contains("<ThemeUseCaseCardHost", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("IThemeScenarioRegistry", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeScenarioBentoCard", bento, StringComparison.Ordinal);
    }

    [Fact]
    public void AnimationTicksOnlyInvalidateCardsThatActuallyConsumeAnimatedFrames()
    {
        var root = FindRoot();
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        var animatedHost = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeAnimatedUseCaseCardHost.razor");

        Assert.Contains("AnimatedCardIds.Contains(card.Id)", bento, StringComparison.Ordinal);
        Assert.Contains("<ThemeAnimatedUseCaseCardHost", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("AnimationState.Changed += OnChanged", bento, StringComparison.Ordinal);
        Assert.Contains("AnimationState.Changed += OnChanged", animatedHost, StringComparison.Ordinal);
        Assert.Contains("<ThemeUseCaseCardHost", animatedHost, StringComparison.Ordinal);
    }

    [Fact]
    public void CuratedCardsResolveSemanticIconsInsideWorkflowContentInsteadOfRepeatingDecorativeHeaders()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");
        var resolver = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Theming", "Runway", "ThemeStudioIconResolver.cs");

        Assert.Contains("Name=\"ThemeStudioIconLibrary\" Value=\"State.IconLibrary\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("IconLibrary=\"IconLibrary\"", bento, StringComparison.Ordinal);
        Assert.Contains("data-theme-workflow-icon", card, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-use-case-card__title-icon", card, StringComparison.Ordinal);
        Assert.Contains("theme-workflow-inline-icon", card, StringComparison.Ordinal);
        Assert.Contains("ThemeStudioIconResolver.Resolve", card, StringComparison.Ordinal);
        foreach (var library in new[] { "LucideIconCatalog", "PhosphorIconCatalog", "TablerIconCatalog", "HugeiconsIconCatalog" })
            Assert.Contains(library, resolver, StringComparison.Ordinal);
    }

    [Fact]
    public void InteractiveWorkflowCardsExposeFilesProcessEditorAndComposerReplyContext()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");

        Assert.Contains("_dropzoneFiles", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachmentGroup", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachmentAction", card, StringComparison.Ordinal);
        Assert.Contains("RemoveUploadedFile", card, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-uploaded-file\"", card, StringComparison.Ordinal);
        Assert.Contains("_processEditorOpen", card, StringComparison.Ordinal);
        Assert.Contains("theme-process-editor", card, StringComparison.Ordinal);
        Assert.Contains("_replyQuote", card, StringComparison.Ordinal);
        Assert.Contains("ShadcnMessageReplyQuote", card, StringComparison.Ordinal);
        Assert.Contains("Label=\"@T(\"Copy message\")\"", card, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemeStudioUsesUniversalHeaderSidebarControlsAndNoLegacyToolbarOrCatalog()
    {
        var root = FindRoot();
        var layout = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeStudioLayout.razor");
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");

        Assert.Contains("<DocumentationHeader", layout, StringComparison.Ordinal);
        Assert.Contains("<ThemeBento", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<ThemePresetDock", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<PreviewToolbar", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<ThemeScenarioBrowser", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeColorGroup", inspector, StringComparison.Ordinal);
        Assert.Contains("theme-device-choice--{viewport.Id}", inspector, StringComparison.Ordinal);
        Assert.Contains("preview-animation-pause", inspector, StringComparison.Ordinal);
        Assert.Contains("resetInitialPreviewScroll", page, StringComparison.Ordinal);
    }

    [Fact]
    public void BentoIncludesFullyInteractiveOverlayComponentExamples()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");

        Assert.Contains("<ShadcnDialog", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnDrawer", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSheet", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnDropdownMenu", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnHoverCard", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTooltip", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnContextMenu", card, StringComparison.Ordinal);
        Assert.DoesNotContain("Disabled=\"IsMirror\"", card, StringComparison.Ordinal);
    }

    [Fact]
    public void CuratedDeckNamesTheMissingDataConversationAndFeedbackWorkflows()
    {
        var root = FindRoot();
        var registry = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Theming", "Runway", "ThemeUseCaseRegistry.cs");
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");

        foreach (var id in new[] { "production-analytics", "drawing-attachment", "inspection-table", "quotation-data-table", "quality-alert", "conversation-marker", "assistant-conversation", "project-questionnaire" })
            Assert.Contains($"\"{id}\"", registry, StringComparison.Ordinal);

        Assert.Contains("<ShadcnChart", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAttachment", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTable", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnDataTable", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAlert", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarker", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScroller", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnQuestionnaire", card, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemeOverlaysHaveResponsiveDrawerGeometryAndPointerHoverFeedback()
    {
        var css = Read(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-overlays-menus.css");

        Assert.Contains("@media (min-width: 40.001rem)", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-drawer-content[data-swipe-axis=\"y\"]", css, StringComparison.Ordinal);
        Assert.Contains("inline-size: min(40rem, calc(100vw - 2rem))", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-drawer-content[data-swipe-axis=\"y\"] .shadcn-drawer-footer", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-dropdown-menu-item:hover", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-dropdown-menu-checkbox-item:hover", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-dropdown-menu-radio-item:hover", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-dropdown-menu-sub-trigger:hover", css, StringComparison.Ordinal);
    }

    [Fact]
    public void CuratedRunwayDoesNotExposeTheInternalScenarioQaMatrix()
    {
        var root = FindRoot();
        var page = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "ThemeStudio.razor");
        var inspector = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "ThemeInspector.razor");

        Assert.DoesNotContain("ThemeScenarioBrowser", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Component coverage", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("Component matrix", page + inspector, StringComparison.Ordinal);
    }

    [Fact]
    public void CuratedCardsUsePackageSurfacesWithoutDecorativeStatusChrome()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");

        Assert.Contains("<ShadcnCard", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnCardHeader", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnCardContent", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAvatarImage", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageGroup", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnBubble", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScrollerProvider", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScrollerViewport", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMessageScrollerItem", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMarker", card, StringComparison.Ordinal);
        Assert.Contains("<ShadcnQuestionnaire", card, StringComparison.Ordinal);
        Assert.Contains("Class=\"theme-runway-dropzone\"", card, StringComparison.Ordinal);
        Assert.Contains("SelectionChanged=\"HandleDropzoneSelectionAsync\"", card, StringComparison.Ordinal);
        Assert.Contains("Loading=\"_dropzoneUploading\"", card, StringComparison.Ordinal);
        Assert.Contains("ButtonType=\"ShadcnButtonType.Submit\"", card, StringComparison.Ordinal);
        Assert.Contains("BusyText=\"@T(\"Saving\")\"", card, StringComparison.Ordinal);
        Assert.Contains("SuccessText=\"@T(\"Saved\")\"", card, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-use-case-card__eyebrow", card, StringComparison.Ordinal);
        Assert.DoesNotContain("private string Status", card, StringComparison.Ordinal);
        Assert.Contains("FormatPercent", card, StringComparison.Ordinal);
        Assert.Contains("<ThemeTypingText Class=\"theme-runway-typing-text\"", card, StringComparison.Ordinal);
        Assert.Contains("@TurnText(turn)", card, StringComparison.Ordinal);
        Assert.DoesNotContain("AssistantMessage[..Math.Min(Frame.ChatCharacters", card, StringComparison.Ordinal);
    }

    [Fact]
    public void AssistantTypingRevealUsesCssAndRespectsReducedMotion()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");
        var animatedInput = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeAnimatedInput.razor");
        var animatedTextarea = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeAnimatedTextarea.razor");
        var typingText = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeTypingText.razor");
        var composer = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeConversationComposer.razor");
        var script = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "theme-studio.js");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("<ThemeTypingText Class=\"theme-runway-typing-text\"", card, StringComparison.Ordinal);
        Assert.Contains("Class=\"theme-animated-input__ink\"", animatedInput, StringComparison.Ordinal);
        Assert.Contains("Class=\"theme-animated-textarea__ink\"", animatedTextarea, StringComparison.Ordinal);
        Assert.Contains("StringInfo.GetTextElementEnumerator", typingText, StringComparison.Ordinal);
        Assert.Contains("class=\"theme-typing-glyph\"", typingText, StringComparison.Ordinal);
        Assert.Contains("animation-name: theme-typing-glyph-reveal", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes theme-typing-glyph-reveal", css, StringComparison.Ordinal);
        Assert.Contains("animation-fill-mode: backwards", css, StringComparison.Ordinal);
        Assert.DoesNotContain("clip-path: inset(0 100% 0 0)", css, StringComparison.Ordinal);
        Assert.Contains("<ThemeConversationComposer Locale=\"Locale\" OnSend=\"SendConversationMessage\"", card, StringComparison.Ordinal);
        Assert.Contains("@bind-Value=\"_text\"", composer, StringComparison.Ordinal);
        Assert.Contains("preservePreviewScrollOnInput", composer, StringComparison.Ordinal);
        Assert.Contains("preservePreviewScrollOnInput", script, StringComparison.Ordinal);
        Assert.Contains("SendConversationMessage", card, StringComparison.Ordinal);
        Assert.Contains("AssistantResponses", card, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-scope[data-preview-reduced-motion=\"true\"] .theme-typing-glyph", css, StringComparison.Ordinal);
    }

    [Fact]
    public void BentoUsesTheReusableRevealBoundaryWithoutPrivateRuntimeOwnership()
    {
        var root = FindRoot();
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("<ShadcnRevealGroup", bento, StringComparison.Ordinal);
        Assert.Contains("Paused=\"Paused\"", bento, StringComparison.Ordinal);
        Assert.Contains("ReducedMotion=\"ReducedMotion\"", bento, StringComparison.Ordinal);
        Assert.Contains("<ShadcnReveal @key=\"card.Id\"", bento, StringComparison.Ordinal);
        Assert.Contains("Cascade=\"true\"", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-bento.js", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("attachBentoReveal", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-bento-card-reveal", css, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "theme-bento.js")));
    }

    [Fact]
    public void CuratedWorkflowsShareOneBilingualCopyBoundary()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");
        var composer = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeConversationComposer.razor");
        var console = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeOperationsConsole.razor");
        var copy = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Theming", "Runway", "ThemeRunwayCopy.cs");

        Assert.Contains("ThemeRunwayCopy.Get(Locale", card, StringComparison.Ordinal);
        Assert.Contains("ThemeRunwayCopy.Get(Locale", composer, StringComparison.Ordinal);
        Assert.Contains("ThemeRunwayCopy.Get(Locale", console, StringComparison.Ordinal);
        Assert.Contains("[Parameter] public ThemeStudioLocale Locale", composer, StringComparison.Ordinal);
        Assert.Contains("[Parameter] public ThemeStudioLocale Locale", console, StringComparison.Ordinal);
        Assert.Contains("ThemeStudioLocale.Thai", copy, StringComparison.Ordinal);
        Assert.Contains("กำลังการผลิตรายสัปดาห์", copy, StringComparison.Ordinal);
        Assert.Contains("กำลังการผลิตรายสัปดาห์", copy, StringComparison.Ordinal);
        Assert.Contains("บันทึกโปรไฟล์", copy, StringComparison.Ordinal);
        Assert.Contains("TurnText(turn)", card, StringComparison.Ordinal);
    }

    [Fact]
    public void OperationsConsolesUseDistinctWorkflowOverviewsWithoutClippingDepth()
    {
        var root = FindRoot();
        var console = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeOperationsConsole.razor");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("data-overview=\"production\"", console, StringComparison.Ordinal);
        Assert.Contains("data-overview=\"quality\"", console, StringComparison.Ordinal);
        Assert.Contains("data-overview=\"handoff\"", console, StringComparison.Ordinal);
        Assert.Contains("Quality evidence readiness", console, StringComparison.Ordinal);
        Assert.Contains("Delivery route", console, StringComparison.Ordinal);
        Assert.DoesNotContain(".theme-operations-console { min-inline-size: 0; max-inline-size: 100%; overflow-x: clip; }", css, StringComparison.Ordinal);
        Assert.Contains(".theme-operations-console { min-inline-size: 0; max-inline-size: 100%; overflow: visible; }", css, StringComparison.Ordinal);
        Assert.Contains(".theme-use-case-card:has(.theme-operations-console) { overflow: visible; }", css, StringComparison.Ordinal);
    }

    private static string Read(string root, params string[] segments) => File.ReadAllText(Path.Combine([root, .. segments]));
    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
