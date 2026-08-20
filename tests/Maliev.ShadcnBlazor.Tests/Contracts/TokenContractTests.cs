using System.Text;
using System.Text.RegularExpressions;
namespace Maliev.ShadcnBlazor.Tests.Contracts;

public sealed class TokenContractTests
{
    [Fact]
    public void StylesheetExposesPinnedNeutralTokensThroughRootAndDarkThemeRules()
    {
        var stylesheet = CssStylesheet.Load(FindCss());

        var root = stylesheet.GetRequiredRule(":root");
        Assert.Equal("oklch(1 0 0)", root.GetRequiredDeclaration("--shadcn-background"));
        Assert.Equal("oklch(0.145 0 0)", root.GetRequiredDeclaration("--shadcn-foreground"));
        Assert.Equal("oklch(0.205 0 0)", root.GetRequiredDeclaration("--shadcn-primary"));
        Assert.Equal("oklch(0.985 0 0)", root.GetRequiredDeclaration("--shadcn-primary-foreground"));
        Assert.Equal("oklch(0.922 0 0)", root.GetRequiredDeclaration("--shadcn-border"));
        Assert.Equal("oklch(0.922 0 0)", root.GetRequiredDeclaration("--shadcn-input"));
        Assert.Equal("oklch(0.708 0 0)", root.GetRequiredDeclaration("--shadcn-ring"));
        Assert.Equal("'Geist', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif", root.GetRequiredDeclaration("--shadcn-font-sans"));
        Assert.Equal("'JetBrains Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace", root.GetRequiredDeclaration("--shadcn-font-mono"));
        Assert.Equal("0.625rem", root.GetRequiredDeclaration("--shadcn-radius"));
        Assert.Equal("2.25rem", root.GetRequiredDeclaration("--shadcn-control-height"));
        Assert.Equal("1", root.GetRequiredDeclaration("--shadcn-spacing-multiplier"));
        Assert.Equal("3px", root.GetRequiredDeclaration("--shadcn-focus-ring-width"));
        Assert.Equal("0px", root.GetRequiredDeclaration("--shadcn-focus-ring-offset"));
        Assert.Equal("150ms", root.GetRequiredDeclaration("--shadcn-motion-duration"));
        Assert.Equal("100ms", root.GetRequiredDeclaration("--shadcn-motion-duration-fast"));
        Assert.Equal("1.4s", root.GetRequiredDeclaration("--shadcn-motion-duration-slow"));
        Assert.Equal("ease", root.GetRequiredDeclaration("--shadcn-motion-easing-standard"));
        Assert.Equal("ease-out", root.GetRequiredDeclaration("--shadcn-motion-easing-enter"));

        var dark = stylesheet.GetRequiredRule("[data-shadcn-theme=\"dark\"]");
        Assert.Equal("oklch(0.145 0 0)", dark.GetRequiredDeclaration("--shadcn-background"));
        Assert.Equal("oklch(0.985 0 0)", dark.GetRequiredDeclaration("--shadcn-foreground"));
        Assert.DoesNotContain(stylesheet.GetAllDeclarationNames(), name => name.StartsWith("--mud-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PackageBundlesTheFontsUsedByTheDefaultTokens()
    {
        var root = FindRoot();
        var fonts = Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "fonts");
        foreach (var file in new[]
        {
            "geist-sans-variable.woff2", "noto-sans-thai.woff2", "jetbrains-mono-latin.woff2"
        })
        {
            var path = Path.Combine(fonts, file);
            Assert.True(File.Exists(path), $"Missing bundled font {file}.");
            Assert.True(new FileInfo(path).Length > 10_000, $"Bundled font {file} is unexpectedly small.");
        }

        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-base.css"));
        Assert.Contains("font-family: \"Geist\"", css, StringComparison.Ordinal);
        Assert.Contains("font-family: \"Noto Sans Thai\"", css, StringComparison.Ordinal);
        Assert.Contains("font-family: \"JetBrains Mono\"", css, StringComparison.Ordinal);
        Assert.Contains(":where(:lang(th), [lang|=\"th\"])", css, StringComparison.Ordinal);
    }

    [Fact]
    public void StylesheetAppliesAccessibilityContractsInTheirMediaQueries()
    {
        var stylesheet = CssStylesheet.Load(FindCss());

        var coarsePointer = stylesheet.GetRequiredMedia("pointer: coarse");
        var scopeButton = coarsePointer.GetRequiredRule(".shadcn-scope :where(button, [role=\"button\"])");
        var overlayButton = coarsePointer.GetRequiredRule(".shadcn-overlay-scope :where(button, [role=\"button\"])");
        var scopeTextControl = coarsePointer.GetRequiredRule(".shadcn-scope :where(input, select, textarea)");
        var overlayTextControl = coarsePointer.GetRequiredRule(".shadcn-overlay-scope :where(input, select, textarea)");

        Assert.Equal("2.75rem", scopeButton.GetRequiredDeclaration("min-width"));
        Assert.Equal("2.75rem", scopeButton.GetRequiredDeclaration("min-height"));
        Assert.Equal("2.75rem", overlayButton.GetRequiredDeclaration("min-width"));
        Assert.Equal("2.75rem", overlayButton.GetRequiredDeclaration("min-height"));
        Assert.DoesNotContain("min-width", scopeTextControl.Declarations.Keys, StringComparer.Ordinal);
        Assert.Equal("2.75rem", scopeTextControl.GetRequiredDeclaration("min-height"));
        Assert.DoesNotContain("min-width", overlayTextControl.Declarations.Keys, StringComparer.Ordinal);
        Assert.Equal("2.75rem", overlayTextControl.GetRequiredDeclaration("min-height"));

        var reducedMotion = stylesheet.GetRequiredMedia("prefers-reduced-motion: reduce")
            .GetRequiredRule(".shadcn-scope *");
        Assert.Equal("var(--shadcn-reduced-motion-duration) !important", reducedMotion.GetRequiredDeclaration("animation-duration"));
        Assert.Equal("var(--shadcn-reduced-motion-duration) !important", reducedMotion.GetRequiredDeclaration("transition-duration"));

        var alwaysReduced = stylesheet.GetRequiredRule(".shadcn-scope[data-shadcn-reduced-motion=\"always\"] *");
        Assert.Equal("var(--shadcn-reduced-motion-duration) !important", alwaysReduced.GetRequiredDeclaration("animation-duration"));
        Assert.Equal("var(--shadcn-reduced-motion-duration) !important", alwaysReduced.GetRequiredDeclaration("transition-duration"));

        var forcedColors = stylesheet.GetRequiredMedia("forced-colors: active")
            .GetRequiredRule(".shadcn-scope :focus-visible");
        Assert.Equal("2px solid Highlight", forcedColors.GetRequiredDeclaration("outline"));
        Assert.Equal("2px", forcedColors.GetRequiredDeclaration("outline-offset"));
    }

    [Fact]
    public void SharedThemeControlsDriveRealPackageComponentRulesWithoutChangingPinnedDefaults()
    {
        var root = FindRoot();
        var actions = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-actions.css"));
        var semantic = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-semantic-foundations.css"));
        var mud = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-mudblazor.css"));

        Assert.Contains("gap: calc(0.5rem * var(--shadcn-spacing-multiplier))", actions, StringComparison.Ordinal);
        Assert.Contains("gap: calc(var(--shadcn-toggle-group-gap) * 0.25rem * var(--shadcn-spacing-multiplier))", actions, StringComparison.Ordinal);
        Assert.Contains("padding: calc(1rem * var(--shadcn-spacing-multiplier))", semantic, StringComparison.Ordinal);
        Assert.Contains("margin-block-start: calc(var(--shadcn-typeset-flow) * var(--shadcn-spacing-multiplier))", semantic, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-font-mono: var(--shadcn-font-mono)", semantic, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-measure: 70ch", semantic, StringComparison.Ordinal);
        Assert.Contains("max-inline-size: min(100%, var(--shadcn-typeset-measure))", semantic, StringComparison.Ordinal);
        Assert.Contains("margin-inline: auto", semantic, StringComparison.Ordinal);
        Assert.Contains("margin-block: calc(0.5rem * var(--shadcn-spacing-multiplier)) 0", semantic, StringComparison.Ordinal);
        Assert.Contains("padding-inline-start: calc(1rem * var(--shadcn-spacing-multiplier))", semantic, StringComparison.Ordinal);
        Assert.Contains(":is(.shadcn-typography--unordered-list, .shadcn-typography--ordered-list)", semantic, StringComparison.Ordinal);
        Assert.DoesNotContain("margin-inline-start: calc(1.5rem * var(--shadcn-spacing-multiplier))", semantic, StringComparison.Ordinal);
        Assert.DoesNotContain("font-size: 3rem", semantic, StringComparison.Ordinal);
        Assert.Matches(@"\.shadcn-typography\s*\{[^}]*overflow-wrap:\s*anywhere", semantic);
        Assert.Contains("font-family: var(--shadcn-font-mono)", semantic, StringComparison.Ordinal);
        Assert.Contains("padding-inline: calc(0.625rem * var(--shadcn-spacing-multiplier))", mud, StringComparison.Ordinal);
        Assert.Contains("height: var(--shadcn-control-height)", actions, StringComparison.Ordinal);
        Assert.Contains("border-radius: var(--shadcn-radius-md)", actions, StringComparison.Ordinal);
        Assert.Contains("box-shadow: var(--shadcn-shadow-xs)", actions, StringComparison.Ordinal);
        Assert.All(new[] { actions, semantic, mud }, css =>
        {
            Assert.Contains("var(--shadcn-focus-ring", css, StringComparison.Ordinal);
            Assert.Contains("var(--shadcn-motion-duration", css, StringComparison.Ordinal);
            Assert.Contains("var(--shadcn-motion-easing", css, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void PackageCssContainsNoInertHardCodedSpacingOrStandardMotionDeclarations()
    {
        var cssFiles = Directory.GetFiles(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css"), "*.css");
        var spacing = new Regex(@"(?:^|[;{])\s*(?:gap|row-gap|column-gap|padding(?:-(?:inline|block|top|right|bottom|left)(?:-(?:start|end))?)?|(?:scroll-)?margin(?:-(?:inline|block|top|right|bottom|left)(?:-(?:start|end))?)?)\s*:\s*(?!0(?:\s|;|$)|\s*-)[^;]*(?:\d*\.\d+|\d+)(?:r?em)\b(?![^;]*--shadcn-spacing-multiplier)", RegexOptions.Multiline | RegexOptions.CultureInvariant);
        var fixedMotion = new Regex(@"(?:^|[;{])\s*(?:transition|animation)(?:-(?:duration|timing-function))?\s*:\s*(?!none\s*;)[^;]*\b\d+(?:\.\d+)?m?s\b", RegexOptions.Multiline | RegexOptions.CultureInvariant);
        var unscopedShadcnToken = new Regex(@"var\(--(?:border|card|card-foreground|radius-(?:sm|md|lg)|destructive|muted-foreground|ring)\)", RegexOptions.CultureInvariant);

        foreach (var path in cssFiles)
        {
            var css = File.ReadAllText(path);
            Assert.DoesNotMatch(spacing, css);
            Assert.DoesNotMatch(unscopedShadcnToken, css);
            if (!path.EndsWith("shadcn-base.css", StringComparison.Ordinal))
                Assert.DoesNotMatch(fixedMotion, css);
        }
    }

    private static string FindCss()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;

        return Path.Combine(directory!.FullName, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-base.css");
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }

    private sealed class CssStylesheet
    {
        private readonly IReadOnlyList<CssRule> _rules;
        private readonly IReadOnlyList<CssMediaRule> _mediaRules;

        private CssStylesheet(IReadOnlyList<CssRule> rules, IReadOnlyList<CssMediaRule> mediaRules)
        {
            _rules = rules;
            _mediaRules = mediaRules;
        }

        public static CssStylesheet Load(string path) => Parse(File.ReadAllText(path));

        public CssRule GetRequiredRule(string selector) =>
            _rules.SingleOrDefault(rule => rule.Selectors.Contains(selector, StringComparer.Ordinal))
            ?? throw new Xunit.Sdk.XunitException($"Missing CSS rule for selector '{selector}'.");

        public CssMediaRule GetRequiredMedia(string query) =>
            _mediaRules.SingleOrDefault(media => string.Equals(media.Query, query, StringComparison.Ordinal))
            ?? throw new Xunit.Sdk.XunitException($"Missing CSS media query '{query}'.");

        public IEnumerable<string> GetAllDeclarationNames() =>
            _rules.SelectMany(rule => rule.Declarations.Keys)
                .Concat(_mediaRules.SelectMany(media => media.Rules.SelectMany(rule => rule.Declarations.Keys)));

        private static CssStylesheet Parse(string css)
        {
            var rules = new List<CssRule>();
            var mediaRules = new List<CssMediaRule>();
            var index = 0;

            while (TryReadBlock(css, ref index, out var prelude, out var body))
            {
                if (prelude.StartsWith("@media", StringComparison.Ordinal))
                {
                    mediaRules.Add(new CssMediaRule(
                        NormalizeMediaQuery(prelude["@media".Length..]),
                        ParseRules(body)));
                }
                else if (!prelude.StartsWith('@'))
                {
                    rules.Add(CreateRule(prelude, body));
                }
            }

            return new CssStylesheet(rules, mediaRules);
        }

        private static IReadOnlyList<CssRule> ParseRules(string css)
        {
            var rules = new List<CssRule>();
            var index = 0;
            while (TryReadBlock(css, ref index, out var prelude, out var body))
            {
                if (!prelude.StartsWith('@'))
                    rules.Add(CreateRule(prelude, body));
            }

            return rules;
        }

        private static CssRule CreateRule(string selectorList, string declarations)
        {
            var selectors = SplitSelectors(selectorList)
                .ToArray();
            var properties = declarations.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(declaration => declaration.Split(':', 2, StringSplitOptions.TrimEntries))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => Normalize(parts[1]), StringComparer.Ordinal);

            return new CssRule(selectors, properties);
        }

        private static IEnumerable<string> SplitSelectors(string selectorList)
        {
            var start = 0;
            var parenthesisDepth = 0;
            var attributeDepth = 0;

            for (var index = 0; index < selectorList.Length; index++)
            {
                switch (selectorList[index])
                {
                    case '(':
                        parenthesisDepth++;
                        break;
                    case ')':
                        parenthesisDepth--;
                        break;
                    case '[':
                        attributeDepth++;
                        break;
                    case ']':
                        attributeDepth--;
                        break;
                    case ',' when parenthesisDepth == 0 && attributeDepth == 0:
                        yield return Normalize(selectorList[start..index]);
                        start = index + 1;
                        break;
                }
            }

            yield return Normalize(selectorList[start..]);
        }

        private static bool TryReadBlock(string css, ref int index, out string prelude, out string body)
        {
            while (index < css.Length && char.IsWhiteSpace(css[index]))
                index++;

            if (index >= css.Length)
            {
                prelude = string.Empty;
                body = string.Empty;
                return false;
            }

            var openBrace = css.IndexOf('{', index);
            if (openBrace < 0)
            {
                prelude = string.Empty;
                body = string.Empty;
                return false;
            }

            prelude = Normalize(css[index..openBrace]);
            var depth = 1;
            var cursor = openBrace + 1;
            for (; cursor < css.Length && depth > 0; cursor++)
            {
                if (css[cursor] == '{') depth++;
                if (css[cursor] == '}') depth--;
            }

            if (depth != 0)
                throw new Xunit.Sdk.XunitException("CSS contains an unclosed block.");

            body = css[(openBrace + 1)..(cursor - 1)];
            index = cursor;
            return true;
        }

        private static string Normalize(string value) => string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        private static string NormalizeMediaQuery(string value)
        {
            var normalized = Normalize(value);
            return normalized.StartsWith('(') && normalized.EndsWith(')')
                ? normalized[1..^1]
                : normalized;
        }
    }

    private sealed record CssRule(IReadOnlyList<string> Selectors, IReadOnlyDictionary<string, string> Declarations)
    {
        public string GetRequiredDeclaration(string property) =>
            Declarations.TryGetValue(property, out var value)
                ? value
                : throw new Xunit.Sdk.XunitException($"Missing CSS declaration '{property}'.");
    }

    private sealed record CssMediaRule(string Query, IReadOnlyList<CssRule> Rules)
    {
        public CssRule GetRequiredRule(string selector) =>
            Rules.SingleOrDefault(rule => rule.Selectors.Contains(selector, StringComparer.Ordinal))
            ?? throw new Xunit.Sdk.XunitException($"Missing CSS rule for selector '{selector}' in media query '{Query}'.");
    }
}
