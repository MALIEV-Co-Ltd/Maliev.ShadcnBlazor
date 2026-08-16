using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DataDisplayShowcaseContractTests : BunitContext
{
    private static readonly string[] Slugs = ["chart", "data-table", "table"];
    public DataDisplayShowcaseContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<IShadcnIdAllocator, ShadcnIdAllocator>();
    }

    [Fact]
    public void EveryCertifiedPlanEightComponentHasRealMetadataApiAndPreview()
    {
        var catalog = new ComponentDocumentationCatalog(); var api = new ComponentApiCatalog(); var registry = new ComponentExampleRegistry(catalog);
        foreach (var slug in Slugs)
        {
            var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug(slug));
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
            Assert.True(entry.Evidence.Api && entry.Evidence.ComponentTests && entry.Evidence.Accessibility && entry.Evidence.Interaction && entry.Evidence.ComputedStyle && entry.Evidence.Visual && entry.Evidence.Integration);
            Assert.Equal("Maliev.ShadcnBlazor.Components.DataDisplay", entry.Namespace);
            Assert.NotNull(entry.PrimaryType);
            Assert.NotEmpty(api.GetByEntry(entry));
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.Equal($"{slug}-primary", example.Id);
            Assert.NotEmpty(example.Controls); Assert.NotEmpty(example.StateTags);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
    }

    [Fact]
    public void EveryPlanEightControlMutatesItsActualCanvas()
    {
        foreach (var slug in Slugs)
            foreach (var controlId in new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id).ToArray())
            {
                var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single();
                var control = example.Controls.Single(candidate => candidate.Id == controlId);
                var before = Render(example.Preview).Markup;
                control.Apply(bool.Parse(control.Value) ? "false" : "true");
                Assert.NotEqual(before, Render(example.Preview).Markup);
            }
    }

    [Fact]
    public void DataTableExampleUsesRealRowsAndAccessibleRowActions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("data-table").Single();
        var rendered = Render(example.Preview);

        Assert.Equal(5, rendered.FindAll("[data-slot='data-table'] tbody tr[data-row-key]").Count);
        Assert.Equal(5, rendered.FindAll("[data-slot='data-table'] .showcase-data-table-row-action").Count);
        Assert.Equal(5, rendered.FindAll("[data-slot='data-table'] .showcase-data-table-row-action svg").Count);
        Assert.DoesNotContain("•••", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("aria-label=\\\"เปิด @row.Email\\\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TableExampleKeepsHeaderBodyAndTotalFooterColumnsAligned()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("table").Single();
        var rendered = Render(example.Preview);

        Assert.Equal(4, rendered.FindAll("[data-slot='table-header'] [data-slot='table-head']").Count);
        Assert.All(rendered.FindAll("[data-slot='table-body'] [data-slot='table-row']"), row =>
            Assert.Equal(4, row.Children.Count(child => child.GetAttribute("data-slot") == "table-cell")));

        var footerCells = rendered.FindAll("[data-slot='table-footer'] [data-slot='table-cell']");
        Assert.Equal(2, footerCells.Count);
        Assert.Equal("3", footerCells[0].GetAttribute("colspan"));
        Assert.Equal("฿37,800", footerCells[1].TextContent.Trim());
        Assert.Contains("<ShadcnTableHead>Method</ShadcnTableHead>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTableCell ColSpan=\"3\">Total</ShadcnTableCell>", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationRouteLinksPinnedAndCurrentReferences()
    {
        var route = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        Assert.Contains("new-york-v4/ui/chart.tsx", route, StringComparison.Ordinal);
        Assert.Contains("new-york-v4/examples/data-table-demo.tsx", route, StringComparison.Ordinal);
        Assert.Contains("new-york-v4/ui/table.tsx", route, StringComparison.Ordinal);
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
