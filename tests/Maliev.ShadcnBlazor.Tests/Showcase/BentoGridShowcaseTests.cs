using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class BentoGridShowcaseTests
{
    [Fact]
    public void BentoGridIsACategorizedLayoutComponentWithThreeDedicatedExamples()
    {
        var catalog = new ComponentDocumentationCatalog();
        var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("bento-grid"));
        var examples = new ComponentExampleRegistry(catalog).GetBySlug(entry.Slug);

        Assert.Equal("Layout", entry.Category);
        Assert.Equal(3, examples.Count);
        Assert.All(examples, example =>
        {
            Assert.Contains("<ShadcnBentoGrid", example.RazorSource, StringComparison.Ordinal);
            Assert.Contains("<ShadcnBentoItem", example.RazorSource, StringComparison.Ordinal);
        });
        Assert.Contains(examples, example => example.RazorSource.Contains("Masonry=\"true\"", StringComparison.Ordinal));
    }
}
