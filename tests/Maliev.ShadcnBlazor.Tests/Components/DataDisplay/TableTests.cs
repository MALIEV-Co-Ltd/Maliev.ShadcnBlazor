using Bunit;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.DataDisplay;

public sealed class TableTests : BunitContext
{
    [Fact]
    public void TableRendersTheExactSemanticCompositionAndPinnedSlots()
    {
        var cut = Render<ShadcnTable>(parameters => parameters
            .Add(component => component.Class, "consumer-table")
            .Add(component => component.Style, "min-width: 42rem")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["aria-describedby"] = "orders-help",
                ["data-slot"] = "wrong"
            })
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnTableCaption>(0);
                builder.AddAttribute(1, nameof(ShadcnTableCaption.ChildContent), Text("รายการใบสั่งซื้อ"));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnTableHeader>(2);
                builder.AddAttribute(3, nameof(ShadcnTableHeader.ChildContent), (RenderFragment)(header =>
                {
                    header.OpenComponent<ShadcnTableRow>(0);
                    header.AddAttribute(1, nameof(ShadcnTableRow.ChildContent), (RenderFragment)(row =>
                    {
                        row.OpenComponent<ShadcnTableHead>(0);
                        row.AddAttribute(1, nameof(ShadcnTableHead.ChildContent), Text("เลขที่"));
                        row.CloseComponent();
                    }));
                    header.CloseComponent();
                }));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnTableBody>(4);
                builder.AddAttribute(5, nameof(ShadcnTableBody.ChildContent), (RenderFragment)(body =>
                {
                    body.OpenComponent<ShadcnTableRow>(0);
                    body.AddAttribute(1, nameof(ShadcnTableRow.ChildContent), (RenderFragment)(row =>
                    {
                        row.OpenComponent<ShadcnTableCell>(0);
                        row.AddAttribute(1, nameof(ShadcnTableCell.ChildContent), Text("PO-42"));
                        row.CloseComponent();
                    }));
                    body.CloseComponent();
                }));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnTableFooter>(6);
                builder.AddAttribute(7, nameof(ShadcnTableFooter.ChildContent), (RenderFragment)(footer =>
                {
                    footer.OpenComponent<ShadcnTableRow>(0);
                    footer.AddAttribute(1, nameof(ShadcnTableRow.ChildContent), (RenderFragment)(row =>
                    {
                        row.OpenComponent<ShadcnTableCell>(0);
                        row.AddAttribute(1, nameof(ShadcnTableCell.ChildContent), Text("1 รายการ"));
                        row.CloseComponent();
                    }));
                    footer.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var container = cut.Find("[data-slot='table-container']");
        var table = cut.Find("table[data-slot='table']");
        Assert.Equal(container, table.ParentElement);
        Assert.Equal("orders-help", table.GetAttribute("aria-describedby"));
        Assert.Contains("consumer-table", table.ClassList);
        Assert.Contains("min-width: 42rem", table.GetAttribute("style"));
        Assert.Equal("รายการใบสั่งซื้อ", cut.Find("caption[data-slot='table-caption']").TextContent);
        Assert.NotNull(cut.Find("thead[data-slot='table-header']"));
        Assert.NotNull(cut.Find("tbody[data-slot='table-body']"));
        Assert.NotNull(cut.Find("tfoot[data-slot='table-footer']"));
        Assert.Equal("col", cut.Find("th[data-slot='table-head']").GetAttribute("scope"));
        Assert.Equal("PO-42", cut.Find("td[data-slot='table-cell']").TextContent);
    }

    [Fact]
    public void RowExposesSelectedAndExpandedStateWithoutAllowingOwnedAttributeOverrides()
    {
        var cut = RenderRow(parameters => parameters
            .Add(component => component.Selected, true)
            .Add(component => component.Expanded, true)
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["data-state"] = "wrong", ["aria-expanded"] = "false", ["data-testid"] = "row" }));

        var row = cut.Find("tr[data-slot='table-row']");
        Assert.Equal("selected", row.GetAttribute("data-state"));
        Assert.Equal("true", row.GetAttribute("data-expanded"));
        Assert.Null(row.GetAttribute("aria-expanded"));
        Assert.Equal("row", row.GetAttribute("data-testid"));
    }

    [Fact]
    public void DisabledRowExposesSemanticStateAndCannotBeSelected()
    {
        var cut = RenderRow(parameters => parameters
            .Add(component => component.Disabled, true)
            .Add(component => component.Selected, true));

        var row = cut.Find("tr[data-slot='table-row']");
        Assert.Equal("true", row.GetAttribute("aria-disabled"));
        Assert.Null(row.GetAttribute("data-state"));
        Assert.Contains("shadcn-table-row-disabled", row.ClassList);
    }

    [Fact]
    public void TableRejectsMissingContentAndInvalidHeadingScope()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnTable>());
        Assert.ThrowsAny<Exception>(() => Render<ShadcnTableRow>(parameters => parameters.AddChildContent("Broken")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnTableCell>(parameters => parameters.AddChildContent("Broken")));
        Assert.ThrowsAny<Exception>(() => RenderHead((ShadcnTableHeadScope)999));
    }

    [Fact]
    public void HeadAndCellOwnValidSpanAndAssociationAttributes()
    {
        var head = RenderHead(ShadcnTableHeadScope.Row, 2);
        var cell = RenderCell(3, 2, "customer amount");

        Assert.Equal("row", head.Find("th").GetAttribute("scope"));
        Assert.Equal("2", head.Find("th").GetAttribute("colspan"));
        Assert.Equal("3", cell.Find("td").GetAttribute("colspan"));
        Assert.Equal("2", cell.Find("td").GetAttribute("rowspan"));
        Assert.Equal("customer amount", cell.Find("td").GetAttribute("headers"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void CellsRejectInvalidSpans(int invalidSpan)
    {
        Assert.ThrowsAny<Exception>(() => RenderCell(invalidSpan));
        Assert.ThrowsAny<Exception>(() => RenderHead(rowSpan: invalidSpan));
    }

    [Fact]
    public void TableCssOwnsPinnedResponsiveAndLogicalStates()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-data-display.css");
        var css = File.ReadAllText(path);

        Assert.Contains(".shadcn-table-container", css, StringComparison.Ordinal);
        Assert.Contains("overflow-x: auto", css, StringComparison.Ordinal);
        Assert.Contains("caption-side: bottom", css, StringComparison.Ordinal);
        Assert.Contains("text-align: start", css, StringComparison.Ordinal);
        Assert.Contains("[data-state=\"selected\"]", css, StringComparison.Ordinal);
        Assert.Contains("[data-expanded=\"true\"]", css, StringComparison.Ordinal);
        Assert.Contains("forced-colors", css, StringComparison.Ordinal);
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);

    private IRenderedComponent<ShadcnTable> RenderRow(Action<ComponentParameterCollectionBuilder<ShadcnTableRow>> configure) => Render<ShadcnTable>(table => table
        .AddChildContent<ShadcnTableBody>(body => body.AddChildContent<ShadcnTableRow>(row =>
        {
            configure(row);
            row.AddChildContent<ShadcnTableCell>(cell => cell.AddChildContent("Value"));
        })));

    private IRenderedComponent<ShadcnTable> RenderHead(ShadcnTableHeadScope scope = ShadcnTableHeadScope.Column, int? colSpan = null, int? rowSpan = null) => Render<ShadcnTable>(table => table
        .AddChildContent<ShadcnTableHeader>(header => header.AddChildContent<ShadcnTableRow>(row => row.AddChildContent<ShadcnTableHead>(head => head
            .Add(component => component.Scope, scope)
            .Add(component => component.ColSpan, colSpan)
            .Add(component => component.RowSpan, rowSpan)
            .AddChildContent("Customer")))));

    private IRenderedComponent<ShadcnTable> RenderCell(int? colSpan = null, int? rowSpan = null, string? headers = null) => Render<ShadcnTable>(table => table
        .AddChildContent<ShadcnTableBody>(body => body.AddChildContent<ShadcnTableRow>(row => row.AddChildContent<ShadcnTableCell>(cell => cell
            .Add(component => component.ColSpan, colSpan)
            .Add(component => component.RowSpan, rowSpan)
            .Add(component => component.Headers, headers)
            .AddChildContent("MALIEV")))));
}
