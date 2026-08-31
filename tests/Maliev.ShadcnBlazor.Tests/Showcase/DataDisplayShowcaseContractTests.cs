using System.Globalization;
using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.AspNetCore.Components;
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
                var nextValue = control.Kind switch
                {
                    ComponentParameterControlKind.Toggle => bool.Parse(control.Value) ? "false" : "true",
                    ComponentParameterControlKind.Select => control.Options.First(option => !string.Equals(option, control.Value, StringComparison.Ordinal)),
                    ComponentParameterControlKind.Number => (double.Parse(control.Value, CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture),
                    _ => throw new InvalidOperationException($"Unsupported control kind {control.Kind}.")
                };
                control.Apply(nextValue);
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
        Assert.Equal(5, rendered.FindAll("[data-slot='data-table'] .showcase-data-table-row-action[data-slot='dropdown-menu-trigger']").Count);
        Assert.DoesNotContain("•••", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("aria-label=\\\"Open actions for @row.Email\\\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnDropdownMenu", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnDropdownMenuItem>View payment</ShadcnDropdownMenuItem>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("PageSize = 5", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ken99@example.com", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("@maliev.com", example.RazorSource, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DataTablePreviewUsesPackageOwnedControlsAndBalancedColumnGeometry()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("data-table").Single();
        var rendered = Render(example.Preview);

        Assert.Equal(2, rendered.FindAll("[data-slot='data-table-sort-icon']").Count);
        Assert.DoesNotContain("↕", rendered.Markup, StringComparison.Ordinal);
        Assert.NotEmpty(rendered.FindAll(".shadcn-data-table-action-cell"));
        Assert.All(rendered.FindAll(".shadcn-data-table-action-cell"), cell =>
            Assert.Contains("shadcn-data-table-action-cell", cell.ClassList));
        Assert.Contains("text-align: end", rendered.Find("[data-column='amount']").GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void DataTableSourceTracksDirectPreviewInteractions()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("data-table").Single();
        var rendered = Render(example.Preview);

        rendered.Find("button[data-column='email']").Click();
        rendered.Find("input[data-slot='data-table-filter']").Input("ken99");
        rendered.Find("input[data-row-key='1']").Change(true);

        Assert.Contains("new(\"email\", ShadcnSortDirection.Ascending)", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Query = \"ken99\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("SelectedKeys = new HashSet<string>([\"1\"]", example.RazorSource, StringComparison.Ordinal);
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
        Assert.Equal("4", rendered.Find("table[data-slot='table']").GetAttribute("data-expected-columns"));
        Assert.Contains("<ShadcnTableHead>Method</ShadcnTableHead>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTableCell ColSpan=\"3\">Total</ShadcnTableCell>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ExpectedColumnCount=\"4\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void DataDisplaySourcesFollowInteractiveStateControls()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var table = registry.GetBySlug("table").Single();
        table.Controls.Single(control => control.Id == "table-borders").Apply("false");
        table.Controls.Single(control => control.Id == "table-selected").Apply("true");
        Assert.Contains("Class=\"showcase-table\" Borders=\"false\"", table.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("showcase-table--borderless", table.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Borders=\"false\"", table.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTableRow Selected=\"true\">", table.RazorSource, StringComparison.Ordinal);

        var dataTable = registry.GetBySlug("data-table").Single();
        dataTable.Controls.Single(control => control.Id == "data-table-empty").Apply("true");
        dataTable.Controls.Single(control => control.Id == "data-table-manual").Apply("true");
        dataTable.Controls.Single(control => control.Id == "data-table-error").Apply("true");
        Assert.Contains("Items=\"@(Array.Empty<Payment>())\"", dataTable.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Manual=\"true\"", dataTable.RazorSource, StringComparison.Ordinal);
        Assert.Contains("TotalCount=\"12\"", dataTable.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Error=\"Unable to load payments.\"", dataTable.RazorSource, StringComparison.Ordinal);

        var chart = registry.GetBySlug("chart").Single();
        var chartType = chart.Controls.Single(control => control.Id == "chart-type");
        Assert.Equal(ComponentParameterControlKind.Select, chartType.Kind);
        Assert.Equal(Enum.GetNames<ShadcnChartType>(), chartType.Options);
        Assert.DoesNotContain(chart.Controls, control => control.Id is "chart-line" or "chart-area");
        chart.Controls.Single(control => control.Id == "chart-stacked").Apply("true");
        chartType.Apply(nameof(ShadcnChartType.Pie));
        Assert.False(chart.Controls.Single(control => control.Id == "chart-stacked").IsEnabled);
        Assert.False(chart.Controls.Single(control => control.Id == "chart-primary-axis").IsEnabled);
        Assert.False(chart.Controls.Single(control => control.Id == "chart-major-grid").IsEnabled);
        chart.Controls.Single(control => control.Id == "chart-loading").Apply("true");
        chart.Controls.Single(control => control.Id == "chart-legend").Apply("true");
        Assert.Contains("Type=\"ShadcnChartType.Pie\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Stacked=\"false\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Loading=\"true\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowLegend=\"false\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("BarRadius=\"0\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("InitialHeight=\"260\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowMajorGrid=\"false\"", chart.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TableExpansionUsesAnAccessibleRowTriggerAndRevealsUsefulDetail()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("table").Single();
        var rendered = Render(example.Preview);
        var trigger = rendered.Find("button[aria-controls='invoice-INV001-details']");

        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(rendered.FindAll("#invoice-INV001-details"));
        trigger.Click();

        var details = rendered.Find("#invoice-INV001-details");
        Assert.Equal("true", rendered.Find("button[aria-controls='invoice-INV001-details']").GetAttribute("aria-expanded"));
        Assert.Equal("4", details.QuerySelector("[data-slot='table-cell']")!.GetAttribute("colspan"));
        Assert.Contains("Payment reference", details.TextContent, StringComparison.Ordinal);
        Assert.Contains("Inspection", details.TextContent, StringComparison.Ordinal);
        Assert.Contains("aria-expanded=\"@expanded\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("invoice-INV001-details", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ColSpan=\"4\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TableExpansionTracksDossierControlParameterChanges()
    {
        var rendered = Render<TableDossierPreview>(parameters => parameters.Add(component => component.Expanded, false));

        Assert.Null(rendered.Find("tbody [data-slot='table-row']").GetAttribute("data-expanded"));

        await rendered.InvokeAsync(() => rendered.Instance.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(TableDossierPreview.Expanded)] = true
        })));

        Assert.Equal("true", rendered.Find("tbody [data-slot='table-row']").GetAttribute("data-expanded"));
        Assert.Equal("true", rendered.Find("button[aria-controls='invoice-INV001-details']").GetAttribute("aria-expanded"));
        Assert.Single(rendered.FindAll("#invoice-INV001-details"));
    }

    [Fact]
    public void ChartDossierControlsAxesAndGridLevelsIndependentlyWithExactSource()
    {
        var chart = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("chart").Single();
        chart.Controls.Single(control => control.Id == "chart-primary-axis").Apply("false");
        chart.Controls.Single(control => control.Id == "chart-secondary-axis").Apply("true");
        chart.Controls.Single(control => control.Id == "chart-major-grid").Apply("false");
        chart.Controls.Single(control => control.Id == "chart-minor-grid").Apply("true");

        var rendered = Render(chart.Preview);
        Assert.Empty(rendered.FindAll("[data-axis='primary']"));
        Assert.Single(rendered.FindAll("[data-axis='secondary']"));
        Assert.Empty(rendered.FindAll("[data-grid-level='major']"));
        Assert.NotEmpty(rendered.FindAll("[data-grid-level='minor']"));
        Assert.Contains("ShowPrimaryYAxis=\"false\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowSecondaryYAxis=\"true\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowMajorGrid=\"false\"", chart.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowMinorGrid=\"true\"", chart.RazorSource, StringComparison.Ordinal);
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
