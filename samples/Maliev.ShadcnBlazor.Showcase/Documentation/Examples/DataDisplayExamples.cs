using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class DataDisplayExamples
{
    private sealed record Payment(string Id, string Email, string Status, double Amount);
    private static readonly Payment[] Payments = [new("1", "somchai@maliev.com", "processing", 837), new("2", "anong@maliev.com", "success", 242), new("3", "niran@maliev.com", "failed", 316)];
    private static readonly ShadcnDataTableColumn<Payment>[] Columns = [new("status", "สถานะ", row => row.Status) { Filterable = true }, new("email", "อีเมล", row => row.Email) { Sortable = true, Filterable = true }, new("amount", "จำนวนเงิน", row => row.Amount) { Sortable = true }];

    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch { "table" => [Table()], "data-table" => [DataTable()], "chart" => [Chart()], _ => [] };

    private static ComponentExampleDefinition Table()
    {
        var selected = false; var expanded = false; var disabled = false; var footer = true;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnTable>(0); b.AddAttribute(1, "ChildContent", (RenderFragment)(t =>
            {
                AddText<ShadcnTableCaption>(t, 0, "รายการใบแจ้งหนี้ล่าสุด");
                t.OpenComponent<ShadcnTableHeader>(10); t.AddAttribute(11, "ChildContent", (RenderFragment)(h => { h.OpenComponent<ShadcnTableRow>(0); h.AddAttribute(1, "ChildContent", (RenderFragment)(r => { AddText<ShadcnTableHead>(r, 0, "Invoice"); AddText<ShadcnTableHead>(r, 10, "Status"); AddText<ShadcnTableHead>(r, 20, "Amount"); })); h.CloseComponent(); })); t.CloseComponent();
                t.OpenComponent<ShadcnTableBody>(20); t.AddAttribute(21, "ChildContent", (RenderFragment)(body => { body.OpenComponent<ShadcnTableRow>(0); body.AddAttribute(1, "Selected", selected); body.AddAttribute(2, "Expanded", expanded); body.AddAttribute(3, "Disabled", disabled); body.AddAttribute(4, "ChildContent", (RenderFragment)(r => { AddText<ShadcnTableCell>(r, 0, "INV001"); AddText<ShadcnTableCell>(r, 10, "Paid"); AddText<ShadcnTableCell>(r, 20, "฿8,500"); })); body.CloseComponent(); })); t.CloseComponent();
                if (footer) { t.OpenComponent<ShadcnTableFooter>(30); t.AddAttribute(31, "ChildContent", (RenderFragment)(f => { f.OpenComponent<ShadcnTableRow>(0); f.AddAttribute(1, "ChildContent", (RenderFragment)(r => AddText<ShadcnTableCell>(r, 0, "Total ฿8,500"))); f.CloseComponent(); })); t.CloseComponent(); }
            })); b.CloseComponent();
        };
        return Example("table", "Responsive semantic table", preview, [Toggle("table-selected", "Selected row", v => selected = v), Toggle("table-expanded", "Expanded row", v => expanded = v), Toggle("table-disabled", "Disabled row", v => disabled = v), Toggle("table-footer", "Footer", v => footer = v, true)], ["caption", "footer", "selected", "expanded", "disabled", "actions", "responsive-overflow", "rtl"]);
    }

    private static ComponentExampleDefinition DataTable()
    {
        var loading = false; var error = false; var manual = false; var empty = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnDataTable<Payment>>(0); b.AddAttribute(1, "Items", empty ? Array.Empty<Payment>() : Payments); b.AddAttribute(2, "Columns", Columns); b.AddAttribute(3, "RowKey", (Func<Payment, string>)(row => row.Id)); b.AddAttribute(6, "Loading", loading); b.AddAttribute(7, "Error", error ? "โหลดข้อมูลไม่สำเร็จ" : null); b.AddAttribute(8, "Manual", manual); b.AddAttribute(9, "TotalCount", manual ? 8 : 0); b.AddAttribute(10, "FilterPlaceholder", "กรองอีเมล..."); b.AddAttribute(11, "EmptyText", "ไม่พบผลลัพธ์"); b.AddAttribute(12, "RowActionTemplate", (RenderFragment<Payment>)(item => x => { x.OpenElement(0, "button"); x.AddAttribute(1, "aria-label", $"เปิด {item.Email}"); x.AddContent(2, "•••"); x.CloseElement(); })); b.CloseComponent();
        };
        return Example("data-table", "Typed payments data table", preview, [Toggle("data-table-loading", "Loading", v => loading = v), Toggle("data-table-error", "Error", v => error = v), Toggle("data-table-empty", "Empty", v => empty = v), Toggle("data-table-manual", "Manual paging", v => manual = v)], ["sort", "filter", "selection", "pagination", "visibility", "row-actions", "manual", "loading", "empty", "error", "rtl"]);
    }

    private static ComponentExampleDefinition Chart()
    {
        var line = false; var loading = false; var hideLegend = false; var stacked = false;
        var config = new ShadcnChartConfig { ["desktop"] = new("Desktop") { Color = "var(--shadcn-chart-1)" }, ["mobile"] = new("Mobile") { Theme = new("var(--shadcn-chart-2)", "var(--shadcn-chart-4)") } };
        RenderFragment preview = b => { b.OpenComponent<ShadcnChart>(0); b.AddAttribute(1, "Id", "dossier"); b.AddAttribute(2, "Title", "ยอดผู้เข้าชม"); b.AddAttribute(3, "Description", "สามเดือนล่าสุด"); b.AddAttribute(4, "Type", line ? ShadcnChartType.Line : ShadcnChartType.Bar); b.AddAttribute(5, "Config", config); b.AddAttribute(6, "Categories", new[] { "Jan", "Feb", "Mar" }); b.AddAttribute(7, "Series", new[] { new ShadcnChartSeries("desktop", [186, 305, 237]), new ShadcnChartSeries("mobile", [80, 200, 120]) }); b.AddAttribute(8, "Loading", loading); b.AddAttribute(9, "ShowLegend", !hideLegend); b.AddAttribute(10, "Stacked", stacked); b.AddAttribute(11, "LegendInteractive", true); b.CloseComponent(); };
        return Example("chart", "Accessible responsive chart", preview, [Toggle("chart-line", "Line chart", v => line = v), Toggle("chart-stacked", "Stacked", v => stacked = v), Toggle("chart-legend", "Hide legend", v => hideLegend = v), Toggle("chart-loading", "Loading", v => loading = v)], ["bar", "line", "area", "donut", "tooltip", "legend", "theme", "keyboard", "resize", "loading", "rtl"]);
    }

    private static ComponentExampleDefinition Example(string slug, string title, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, "Live package component with caller-owned localized state.", $"<Shadcn{ToPascal(slug)} />", preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static void AddText<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", (RenderFragment)(c => c.AddContent(0, text))); b.CloseComponent(); }
    private static string ToPascal(string value) => string.Concat(value.Split('-').Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
}
