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
        Assert.Contains("class=\"theme-bento__grid\"", bento, StringComparison.Ordinal);
        Assert.DoesNotContain("mirror", bento, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inert", bento, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-runway-track", bento, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: repeat(4, minmax(0, 1fr))", css, StringComparison.Ordinal);
        Assert.Contains("border: 1px solid var(--shadcn-border)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("box-shadow: 0 0 0 1px", css, StringComparison.Ordinal);
        Assert.DoesNotContain("grid-auto-flow: dense", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-region { min-inline-size: 0;", css, StringComparison.Ordinal);
        Assert.Contains("border: 0; border-radius: 0; background: transparent", css, StringComparison.Ordinal);
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
        Assert.Contains("Class=\"theme-runway-dropzone\"", card, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-use-case-card__eyebrow", card, StringComparison.Ordinal);
        Assert.DoesNotContain("private string Status", card, StringComparison.Ordinal);
        Assert.Contains("FormatPercent", card, StringComparison.Ordinal);
        Assert.Contains("class=\"theme-runway-typing\"", card, StringComparison.Ordinal);
        Assert.Contains("@AssistantMessage", card, StringComparison.Ordinal);
        Assert.DoesNotContain("AssistantMessage[..Math.Min(Frame.ChatCharacters", card, StringComparison.Ordinal);
    }

    [Fact]
    public void AssistantTypingRevealUsesCssAndRespectsReducedMotion()
    {
        var root = FindRoot();
        var card = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeUseCaseCardHost.razor");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Contains("class=\"theme-runway-typing-character\"", card, StringComparison.Ordinal);
        Assert.Contains("--typing-index: @index", card, StringComparison.Ordinal);
        Assert.Contains("<span class=\"shadcn-sr-only\">@AssistantMessage</span>", card, StringComparison.Ordinal);
        Assert.Contains("class=\"theme-runway-typing\" aria-hidden=\"true\"", card, StringComparison.Ordinal);
        Assert.Contains("animation: theme-runway-type-character", css, StringComparison.Ordinal);
        Assert.Contains("@keyframes theme-runway-type-character", css, StringComparison.Ordinal);
        Assert.DoesNotContain("theme-runway-typing-reveal", css, StringComparison.Ordinal);
        Assert.DoesNotContain(".theme-runway-typing { display: inline-block; max-inline-size: 100%; clip-path", css, StringComparison.Ordinal);
        Assert.Contains(".theme-preview-scope[data-preview-reduced-motion=\"true\"] .theme-runway-typing-character { animation: none !important; opacity: 1;", css, StringComparison.Ordinal);
    }

    private static string Read(string root, params string[] segments) => File.ReadAllText(Path.Combine([root, .. segments]));
    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
