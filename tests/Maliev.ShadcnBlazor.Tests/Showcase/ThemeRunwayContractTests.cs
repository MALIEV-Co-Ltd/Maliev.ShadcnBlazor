namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeBentoContractTests
{
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
        Assert.Contains("border: 1px solid var(--shadcn-border)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("box-shadow: 0 0 0 1px", css, StringComparison.Ordinal);
        Assert.DoesNotContain("grid-auto-flow: dense", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-region { min-inline-size: 0;", css, StringComparison.Ordinal);
        Assert.Contains("border: 0; border-radius: 0; background: transparent", css, StringComparison.Ordinal);
    }

    [Fact]
    public void BentoUsesOnlyDedicatedCuratedWorkflowCards()
    {
        var root = FindRoot();
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        Assert.Contains("IThemeUseCaseRegistry", bento, StringComparison.Ordinal);
        Assert.Contains("Registry.All.OrderBy", bento, StringComparison.Ordinal);
        Assert.Contains("<ThemeUseCaseCardHost", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("IThemeScenarioRegistry", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeScenarioBentoCard", bento, StringComparison.Ordinal);
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
        Assert.DoesNotContain("theme-use-case-card__eyebrow", card, StringComparison.Ordinal);
        Assert.DoesNotContain("private string Status", card, StringComparison.Ordinal);
        Assert.Contains("FormatPercent", card, StringComparison.Ordinal);
        Assert.Contains("class=\"theme-runway-typing-text\"", card, StringComparison.Ordinal);
        Assert.Contains("@turn.Text", card, StringComparison.Ordinal);
        Assert.DoesNotContain("AssistantMessage[..Math.Min(Frame.ChatCharacters", card, StringComparison.Ordinal);
    }

    [Fact]
    public void AssistantTypingRevealUsesCssAndRespectsReducedMotion()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("class=\"theme-runway-typing-text\"", card, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-runway-typing-character", card, StringComparison.Ordinal);
        Assert.DoesNotContain("SplitTextElements", card, StringComparison.Ordinal);
        Assert.Contains("animation: theme-runway-type-text", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes theme-runway-type-text", css, StringComparison.Ordinal);
        Assert.Contains("forwards", css, StringComparison.Ordinal);
        Assert.Contains("clip-path", css, StringComparison.Ordinal);
        Assert.Contains("@bind-Value=\"_composerText\"", card, StringComparison.Ordinal);
        Assert.Contains("SendConversationMessage", card, StringComparison.Ordinal);
        Assert.Contains("AssistantResponses", card, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-scope[data-preview-reduced-motion=\"true\"] .theme-runway-typing-text", css, StringComparison.Ordinal);
    }

    [Fact]
    public void BentoRevealIsPreviewScopedAndOneTime()
    {
        var root = FindRoot();
        var bento = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeBento.razor");
        var script = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "theme-bento.js");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("attachBentoReveal", bento, StringComparison.Ordinal);
        Assert.Contains("IntersectionObserver", script, StringComparison.Ordinal);
        Assert.Contains("unobserve", script, StringComparison.Ordinal);
        Assert.Contains("data-reveal-state", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-scope", css, StringComparison.Ordinal);
    }

    private static string Read(string root, params string[] segments) => File.ReadAllText(Path.Combine([root, .. segments]));
    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
