using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class VisualStyleScopeShowcaseTests
{
    [Fact]
    public void VisualStyleScopeIsACompleteFoundationComponentWithThreeDedicatedExamples()
    {
        var catalog = new ComponentDocumentationCatalog();
        var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug("visual-style-scope"));
        var examples = new ComponentExampleRegistry(catalog).GetBySlug(entry.Slug);

        Assert.Equal("Foundation", entry.Category);
        Assert.Equal("ShadcnVisualStyleScope", entry.PrimaryType);
        Assert.Equal(3, examples.Count);
        Assert.All(examples, example =>
        {
            Assert.Contains("<ShadcnVisualStyleScope", example.RazorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("theme-studio", example.RazorSource, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("theme-preview", example.RazorSource, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void DedicatedExamplesCoverCompositionInteractionAndEveryStyleLayer()
    {
        var catalog = new ComponentDocumentationCatalog();
        var examples = new ComponentExampleRegistry(catalog).GetBySlug("visual-style-scope");
        var renderedStates = new List<string>();
        foreach (var example in examples)
        {
            renderedStates.Add(example.RazorSource);
            foreach (var control in example.Controls)
            {
                foreach (var option in control.Options)
                {
                    control.Apply(option);
                    renderedStates.Add(example.RazorSource);
                }
            }
        }
        var source = string.Join('\n', renderedStates);

        foreach (var visualStyle in new[] { "Minimal", "Glass", "NeoBrutalist", "LiquidGlass" })
            Assert.Contains($"ShadcnVisualStyle.{visualStyle}", source, StringComparison.Ordinal);
        Assert.Contains("ShadcnColorTreatment.VibrantDark", source, StringComparison.Ordinal);
        Assert.Contains("<ShadcnBentoGrid", source, StringComparison.Ordinal);
        Assert.Contains("<ShadcnDialog", source, StringComparison.Ordinal);
        Assert.Contains("<ShadcnInput", source, StringComparison.Ordinal);
        Assert.All(examples, example => Assert.NotEmpty(example.Controls));
    }
}
