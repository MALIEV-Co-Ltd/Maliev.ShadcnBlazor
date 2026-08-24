namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ThemeRunwayContractTests
{
    [Fact]
    public void RunwayOwnsTwoOpposingTracksInertMirrorsAndMobileNaturalFlow()
    {
        var root = FindRoot();
        var runway = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Theming", "Runway", "ThemeRunway.razor");
        var script = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "js", "theme-studio-runway.js");
        var css = Read(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css");

        Assert.Equal(2, Count(runway, "data-runway-track="));
        Assert.Contains("aria-hidden=\"true\" inert", runway, StringComparison.Ordinal);
        Assert.Contains("theme-runway__mobile", runway, StringComparison.Ordinal);
        Assert.DoesNotContain("ScenarioRegistry.All", runway, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeScenarioRunwayCard", runway, StringComparison.Ordinal);
        Assert.Contains("scrollTop", script, StringComparison.Ordinal);
        Assert.Contains("onScroll", script, StringComparison.Ordinal);
        Assert.Contains("pointerenter", script, StringComparison.Ordinal);
        Assert.Contains("visibilitychange", script, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", script, StringComparison.Ordinal);
        Assert.Contains("@container (max-width: 43.999rem)", css, StringComparison.Ordinal);
        Assert.Contains(".theme-runway__mobile { display: grid", css, StringComparison.Ordinal);
        Assert.Contains("scrollbar-width: none", css, StringComparison.Ordinal);
        Assert.Contains(".theme-runway__viewport::-webkit-scrollbar { display: none; }", css, StringComparison.Ordinal);
        var runwayStart = css.IndexOf("\n.theme-runway {\n", StringComparison.Ordinal) + 1;
        var runwayRule = css[runwayStart..(css.IndexOf("\n}\n", runwayStart, StringComparison.Ordinal) + 3)];
        Assert.DoesNotContain("border:", runwayRule, StringComparison.Ordinal);
        Assert.DoesNotContain("border-radius:", runwayRule, StringComparison.Ordinal);
        Assert.DoesNotContain("background:", runwayRule, StringComparison.Ordinal);
        Assert.DoesNotContain("padding:", runwayRule, StringComparison.Ordinal);
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
        Assert.Contains("<ThemeRunway", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<ThemePresetDock", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<PreviewToolbar", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<ThemeScenarioBrowser", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeColorGroup", inspector, StringComparison.Ordinal);
        Assert.Contains("theme-device-choice--{viewport.Id}", inspector, StringComparison.Ordinal);
        Assert.Contains("runway-pause", inspector, StringComparison.Ordinal);
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
        Assert.Contains(".theme-runway-typing-character { animation: none; opacity: 1;", css, StringComparison.Ordinal);
    }

    private static int Count(string value, string needle) => (value.Length - value.Replace(needle, string.Empty, StringComparison.Ordinal).Length) / needle.Length;
    private static string Read(string root, params string[] segments) => File.ReadAllText(Path.Combine([root, .. segments]));
    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
