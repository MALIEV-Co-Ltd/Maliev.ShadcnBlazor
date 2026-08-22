using Maliev.ShadcnBlazor.Showcase.Documentation;
using System.Text.Json;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DocumentationCatalogTests
{
#pragma warning disable xUnit2029 // Required core contract assertion from the approved task brief.
    [Fact]
    public void Catalog_ReconcilesEveryLedgerEntryWithUniqueStableDocumentationUrls()
    {
        var ledger = ReadLedger();
        var catalog = new ComponentDocumentationCatalog();

        Assert.Equal(65, catalog.All.Count);
        Assert.Empty(catalog.All.GroupBy(x => x.Slug).Where(x => x.Count() > 1));
        Assert.Equal(ledger.Keys.Order(), catalog.All.Select(x => x.Slug).Order());
        Assert.All(catalog.All, entry => Assert.Equal(ledger[entry.Slug].Status, entry.Status.ToString().ToLowerInvariant()));
        Assert.All(catalog.All, entry => Assert.Equal($"docs/components/{entry.Slug}", entry.DocumentationUrl));
        Assert.Equal(65, catalog.All.Count(entry => entry.Status == ComponentDocumentationStatus.Complete));
        Assert.DoesNotContain(catalog.All, entry => entry.Status == ComponentDocumentationStatus.Planned);
    }

    [Fact]
    public void CodeBlockIsAFirstClassCatalogEntryWithPackageApiAndThreeScenarios()
    {
        var catalog = new ComponentDocumentationCatalog();
        var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("code-block"));
        var examples = new Maliev.ShadcnBlazor.Showcase.Documentation.Examples.ComponentExampleRegistry(catalog).GetBySlug(entry.Slug);

        Assert.Equal("Maliev.ShadcnBlazor.Components.Typography", entry.Namespace);
        Assert.Equal("ShadcnCodeBlock", entry.PrimaryType);
        var example = Assert.Single(examples);
        Assert.Equal(3, example.RazorSource.Split("<ShadcnCodeBlock", StringSplitOptions.None).Length - 1);
        Assert.Contains("private Project project = new(\"Bangkok line\")", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private readonly IReadOnlyDictionary<string, string> sources", example.RazorSource, StringComparison.Ordinal);
    }
#pragma warning restore xUnit2029

    [Fact]
    public void FindBySlug_IsCaseInsensitiveAndReturnsNullForUnknownSlugs()
    {
        var catalog = new ComponentDocumentationCatalog();

        Assert.Equal("accordion", catalog.FindBySlug("ACCORDION")?.Slug);
        Assert.Null(catalog.FindBySlug("not-a-shadcn-component"));
    }

    [Fact]
    public void Catalog_UsesAuthoritativeImplementedApiMetadataIndependentOfCertificationStatus()
    {
        var catalog = new ComponentDocumentationCatalog();

        var direction = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("direction"));
        Assert.Equal("Maliev.ShadcnBlazor.Components.Direction", direction.Namespace);
        Assert.Equal("ShadcnDirectionProvider", direction.PrimaryType);

        var accordion = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("accordion"));
        Assert.Equal(ComponentDocumentationStatus.Complete, accordion.Status);
        Assert.Equal("Maliev.ShadcnBlazor.Components.Disclosure", accordion.Namespace);
        Assert.Equal("ShadcnAccordion", accordion.PrimaryType);
    }

    [Fact]
    public void Catalog_PreservesExactLedgerEvidenceForCompleteAndPlannedEntries()
    {
        var catalog = new ComponentDocumentationCatalog();

        Assert.Equal(
            new ComponentDocumentationEvidence(
                Api: true,
                ComponentTests: true,
                Accessibility: true,
                Interaction: true,
                ComputedStyle: true,
                Visual: true,
                Integration: false),
            Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("aspect-ratio")).Evidence);
        Assert.Equal(
            new ComponentDocumentationEvidence(
                Api: true,
                ComponentTests: true,
                Accessibility: true,
                Interaction: true,
                ComputedStyle: true,
                Visual: true,
                Integration: true),
            Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("accordion")).Evidence);
    }

    [Theory]
    [InlineData("  Aspect    Ratio  ")]
    [InlineData("aspect-ratio")]
    [InlineData("ratio")]
    [InlineData("layout primitive")]
    [InlineData("Maliev.ShadcnBlazor.Components.Layout")]
    [InlineData("ShadcnAspectRatio")]
    public void Search_NormalizesWhitespaceAndMatchesEverySearchableMetadataField(string query)
    {
        var results = new ComponentDocumentationCatalog().Search(query, new DocumentationCatalogFilter());

        Assert.Contains(results, entry => entry.Slug == "aspect-ratio");
    }

    [Fact]
    public void Search_UsesAuthoritativeKeyboardMetadataWithoutSuppressingOtherMatches()
    {
        var catalog = new ComponentDocumentationCatalog();

        var shortcutResults = catalog.Search("keyboard shortcut", new DocumentationCatalogFilter());
        var keyboardResults = catalog.Search("keyboard", new DocumentationCatalogFilter());

        Assert.Contains(shortcutResults, entry => entry.Slug == "kbd");
        Assert.Contains(keyboardResults, entry => entry.Slug == "kbd");
        Assert.Contains(keyboardResults, entry => entry.Slug == "command");
    }

    private static IReadOnlyDictionary<string, LedgerEntry> ReadLedger()
    {
        var root = FindRoot();
        var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "docs", "component-catalog.json")));
        return document.RootElement
            .GetProperty("components")
            .EnumerateArray()
            .ToDictionary(
                entry => entry.GetProperty("slug").GetString()!,
                entry => new LedgerEntry(entry.GetProperty("status").GetString()!),
                StringComparer.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find repository root.");
    }

    private sealed record LedgerEntry(string Status);
}
