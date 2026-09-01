using Maliev.ShadcnBlazor.Components.DataDisplay;
using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Overlays;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class DataDisplayExamples
{
    private sealed record Payment(string Id, string Email, string Status, string Method, double Amount);
    private sealed record Invoice(string Id, string Status, string Method, string Amount);
    private static readonly Payment[] Payments = [
        new("1", "ken99@example.com", "success", "Card", 316), new("2", "abe45@example.com", "success", "Transfer", 242), new("3", "monserrat44@example.com", "processing", "Card", 837),
        new("4", "silas22@example.com", "success", "PromptPay", 874), new("5", "carmella@example.com", "failed", "Transfer", 721), new("6", "preecha@example.com", "pending", "PromptPay", 590),
        new("7", "wipada@example.com", "processing", "Card", 445), new("8", "niran@example.com", "success", "Transfer", 680)
    ];
    private static readonly Invoice[] Invoices = [
        new("INV001", "Paid", "Credit Card", "฿8,500"), new("INV002", "Pending", "PayPal", "฿3,250"), new("INV003", "Unpaid", "Bank Transfer", "฿12,400"),
        new("INV004", "Paid", "Credit Card", "฿5,900"), new("INV005", "Paid", "PayPal", "฿7,750")
    ];
    private static readonly ShadcnDataTableColumn<Payment>[] Columns = [
        new("status", "Status", row => row.Status) { Filterable = true, Hideable = true, MinWidth = "8rem" },
        new("email", "Email", row => row.Email) { Sortable = true, Filterable = true, Hideable = true, MinWidth = "14rem" },
        new("method", "Method", row => row.Method) { Filterable = true, Hideable = true, MinWidth = "8rem" },
        new("amount", "Amount", row => row.Amount) { Sortable = true, Filterable = true, Hideable = true, Alignment = ShadcnTableAlignment.End, MinWidth = "7rem" }
    ];

    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch { "table" => [Table()], "data-table" => [DataTable()], "chart" => [Chart()], _ => [] };

    private static ComponentExampleDefinition Table()
    {
        var selected = false; var expanded = false; var disabled = false; var footer = true; var borders = true;
        RenderFragment preview = b => { b.OpenComponent<TableDossierPreview>(0); b.AddAttribute(1, "Selected", selected); b.AddAttribute(2, "Expanded", expanded); b.AddAttribute(3, "Disabled", disabled); b.AddAttribute(4, "Footer", footer); b.AddAttribute(5, "Borders", borders); b.CloseComponent(); };
        string Source()
        {
            var firstRowState = string.Join(" ", new[]
            {
                selected ? "Selected=\"true\"" : string.Empty,
                expanded ? "Expanded=\"true\"" : string.Empty,
                disabled ? "Disabled=\"true\"" : string.Empty
            }.Where(value => value.Length > 0));
            var first = Invoices[0];
            var firstRow = $"        <ShadcnTableRow{(firstRowState.Length > 0 ? $" {firstRowState}" : string.Empty)}><ShadcnTableCell><ShadcnButton Variant=\"ShadcnButtonVariant.Ghost\" Size=\"ShadcnButtonSize.Small\" aria-expanded=\"@expanded\" aria-controls=\"invoice-INV001-details\" OnClick=\"ToggleDetails\">{first.Id}</ShadcnButton></ShadcnTableCell><ShadcnTableCell>{first.Status}</ShadcnTableCell><ShadcnTableCell>{first.Method}</ShadcnTableCell><ShadcnTableCell>{first.Amount}</ShadcnTableCell></ShadcnTableRow>" + Environment.NewLine
                + "        @if (expanded)" + Environment.NewLine
                + "        {" + Environment.NewLine
                + "            <ShadcnTableRow id=\"invoice-INV001-details\"><ShadcnTableCell ColSpan=\"4\"><strong>Payment reference</strong> CC-8851-TH · <strong>Inspection</strong> Reconciled and ready for archive</ShadcnTableCell></ShadcnTableRow>" + Environment.NewLine
                + "        }";
            var rows = string.Join(Environment.NewLine, new[] { firstRow }.Concat(Invoices.Skip(1).Select(invoice =>
                $"        <ShadcnTableRow><ShadcnTableCell>{invoice.Id}</ShadcnTableCell><ShadcnTableCell>{invoice.Status}</ShadcnTableCell><ShadcnTableCell>{invoice.Method}</ShadcnTableCell><ShadcnTableCell>{invoice.Amount}</ShadcnTableCell></ShadcnTableRow>")));
            var footerMarkup = footer
                ? "    <ShadcnTableFooter><ShadcnTableRow><ShadcnTableCell ColSpan=\"3\">Total</ShadcnTableCell><ShadcnTableCell>฿37,800</ShadcnTableCell></ShadcnTableRow></ShadcnTableFooter>"
                : string.Empty;
            return $"""
@using Maliev.ShadcnBlazor.Components.Actions

<ShadcnTable Class="showcase-table" Borders="{borders.ToString().ToLowerInvariant()}" ExpectedColumnCount="4">
    <ShadcnTableCaption>รายการใบแจ้งหนี้ล่าสุด</ShadcnTableCaption>
    <ShadcnTableHeader>
        <ShadcnTableRow>
            <ShadcnTableHead>Invoice</ShadcnTableHead>
            <ShadcnTableHead>Status</ShadcnTableHead>
            <ShadcnTableHead>Method</ShadcnTableHead>
            <ShadcnTableHead>Amount</ShadcnTableHead>
        </ShadcnTableRow>
    </ShadcnTableHeader>
    <ShadcnTableBody>
{rows}
    </ShadcnTableBody>
{footerMarkup}
</ShadcnTable>
""" + Environment.NewLine + Environment.NewLine
                + "@code {" + Environment.NewLine
                + $"    private bool expanded = {expanded.ToString().ToLowerInvariant()};" + Environment.NewLine
                + "    private void ToggleDetails() => expanded = !expanded;" + Environment.NewLine
                + "}";
        }
        var example = Example("table", "Responsive semantic table", "Show a realistic invoice list with caption, status, payment method, selection, and a total footer.", Source(), preview, [Toggle("table-selected", "Selected row", v => selected = v), Toggle("table-expanded", "Expanded row", v => expanded = v), Toggle("table-disabled", "Disabled row", v => disabled = v), Toggle("table-footer", "Footer", v => footer = v, true), Toggle("table-borders", "Borders", v => borders = v, true)], ["caption", "footer", "selected", "expanded", "disabled", "actions", "responsive-overflow", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition DataTable()
    {
        var loading = false; var error = false; var manual = false; var empty = false; var toolbarMode = ShadcnDataTableToolbarMode.Compact;
        var state = new ShadcnDataTableState { PageSize = 5 };
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnDataTable<Payment>>(0); b.AddAttribute(1, "Class", "showcase-data-table"); b.AddAttribute(2, "Items", empty ? Array.Empty<Payment>() : Payments); b.AddAttribute(3, "Columns", Columns); b.AddAttribute(4, "RowKey", (Func<Payment, string>)(row => row.Id)); b.AddAttribute(5, "DefaultState", state); b.AddAttribute(6, "StateChanged", EventCallback.Factory.Create<ShadcnDataTableState>(typeof(DataDisplayExamples), next => state = next)); b.AddAttribute(7, "PageSizeOptions", new[] { 5, 10, 25 }); b.AddAttribute(8, "Loading", loading); b.AddAttribute(9, "Error", error ? "Unable to load payments." : null); b.AddAttribute(10, "Manual", manual); b.AddAttribute(11, "TotalCount", manual ? 12 : 0); b.AddAttribute(12, "FilterPlaceholder", "Search payments..."); b.AddAttribute(13, "EmptyText", "No results."); b.AddAttribute(14, "FiltersLabel", "Filters"); b.AddAttribute(15, "ColumnsLabel", "Columns"); b.AddAttribute(16, "ActionsLabel", "Actions"); b.AddAttribute(17, "SelectAllLabel", "Select all payments"); b.AddAttribute(18, "SelectRowLabel", (Func<Payment, string>)(row => $"Select {row.Email}")); b.AddAttribute(19, "ToolbarMode", toolbarMode); b.AddAttribute(20, "ToolbarStartTemplate", (RenderFragment)(x => { x.OpenElement(0, "span"); x.AddAttribute(1, "class", "showcase-data-table-toolbar-summary"); x.AddContent(2, $"{Payments.Length} payments"); x.CloseElement(); })); b.AddAttribute(21, "ToolbarEndTemplate", (RenderFragment)(x => { x.OpenElement(0, "button"); x.AddAttribute(1, "type", "button"); x.AddAttribute(2, "class", "showcase-data-table-export"); x.AddContent(3, "Export CSV"); x.CloseElement(); })); b.AddAttribute(22, "RowActionTemplate", (RenderFragment<Payment>)(item => x =>
            {
                x.OpenComponent<ShadcnDropdownMenu>(0); x.AddAttribute(1, "ChildContent", (RenderFragment)(menu =>
                {
                    menu.OpenComponent<ShadcnDropdownMenuTrigger>(0); menu.AddAttribute(1, "Class", "showcase-data-table-row-action"); menu.AddAttribute(2, "aria-label", $"Open actions for {item.Email}"); menu.AddAttribute(3, "ChildContent", MoreIcon); menu.CloseComponent();
                    menu.OpenComponent<ShadcnDropdownMenuContent>(10); menu.AddAttribute(11, "Align", ShadcnOverlayAlign.End); menu.AddAttribute(12, "ChildContent", (RenderFragment)(content =>
                    {
                        AddText<ShadcnDropdownMenuItem>(content, 0, "View payment");
                        AddText<ShadcnDropdownMenuItem>(content, 10, $"Copy {item.Id}");
                    })); menu.CloseComponent();
                })); x.CloseComponent();
            })); b.CloseComponent();
        };
        const string sourceTemplate = """
@using Maliev.ShadcnBlazor.Components.DataDisplay
@using Maliev.ShadcnBlazor.Components.Overlays

<ShadcnDataTable TItem="Payment"
                  Items="__ITEMS__"
                  Columns="@Columns"
                  RowKey="@(row => row.Id)"
                  DefaultState="@TableState"
                  StateChanged="OnTableStateChanged"
                  PageSizeOptions="@[5, 10, 25]"
                  ToolbarMode="ShadcnDataTableToolbarMode.__TOOLBAR_MODE__"
__STATE_ATTRIBUTES__
                  FilterPlaceholder="Search payments..."
                  EmptyText="No results."
                  FiltersLabel="Filters"
                  ColumnsLabel="Columns"
                  ActionsLabel="Actions"
                  SelectAllLabel="Select all payments"
                  SelectRowLabel="@(row => $\"Select {row.Email}\")"
                  RowActionTemplate="@((Payment row) => @<ShadcnDropdownMenu><ShadcnDropdownMenuTrigger Class=\"showcase-data-table-row-action\" aria-label=\"Open actions for @row.Email\"><svg viewBox=\"0 0 16 16\" aria-hidden=\"true\"><circle cx=\"3\" cy=\"8\" r=\"1\" /><circle cx=\"8\" cy=\"8\" r=\"1\" /><circle cx=\"13\" cy=\"8\" r=\"1\" /></svg></ShadcnDropdownMenuTrigger><ShadcnDropdownMenuContent Align=\"ShadcnOverlayAlign.End\"><ShadcnDropdownMenuItem>View payment</ShadcnDropdownMenuItem><ShadcnDropdownMenuItem>Copy @row.Id</ShadcnDropdownMenuItem></ShadcnDropdownMenuContent></ShadcnDropdownMenu>)">
    <ToolbarStartTemplate><span class="showcase-data-table-toolbar-summary">8 payments</span></ToolbarStartTemplate>
    <ToolbarEndTemplate><button type="button" class="showcase-data-table-export">Export CSV</button></ToolbarEndTemplate>
</ShadcnDataTable>

@code {
    private sealed record Payment(string Id, string Email, string Status, string Method, double Amount);

    private IReadOnlyList<Payment> Payments = [
        new("1", "ken99@example.com", "success", "Card", 316),
        new("2", "abe45@example.com", "success", "Transfer", 242),
        new("3", "monserrat44@example.com", "processing", "Card", 837),
        new("4", "silas22@example.com", "success", "PromptPay", 874),
        new("5", "carmella@example.com", "failed", "Transfer", 721),
        new("6", "preecha@example.com", "pending", "PromptPay", 590),
        new("7", "wipada@example.com", "processing", "Card", 445),
        new("8", "niran@example.com", "success", "Transfer", 680)
    ];

    private IReadOnlyList<ShadcnDataTableColumn<Payment>> Columns = [
        new("status", "Status", row => row.Status) { Filterable = true, Hideable = true, MinWidth = "8rem" },
        new("email", "Email", row => row.Email) { Sortable = true, Filterable = true, Hideable = true, MinWidth = "14rem" },
        new("method", "Method", row => row.Method) { Filterable = true, Hideable = true, MinWidth = "8rem" },
        new("amount", "Amount", row => row.Amount) { Sortable = true, Filterable = true, Hideable = true, Alignment = ShadcnTableAlignment.End, MinWidth = "7rem" }
    ];

    private ShadcnDataTableState TableState = __TABLE_STATE__;
    private Task OnTableStateChanged(ShadcnDataTableState next) { TableState = next; return Task.CompletedTask; }
}
""";
        string Source()
        {
            var stateAttributes = string.Join(Environment.NewLine, new[]
            {
                loading ? "                  Loading=\"true\"" : string.Empty,
                error ? "                  Error=\"Unable to load payments.\"" : string.Empty,
                manual ? "                  Manual=\"true\"\n                  TotalCount=\"12\"" : string.Empty
            }.Where(value => value.Length > 0));
            return sourceTemplate
                .Replace("__ITEMS__", empty ? "@(Array.Empty<Payment>())" : "@Payments", StringComparison.Ordinal)
                .Replace("__STATE_ATTRIBUTES__", stateAttributes, StringComparison.Ordinal)
                .Replace("__TOOLBAR_MODE__", toolbarMode.ToString(), StringComparison.Ordinal)
                .Replace("__TABLE_STATE__", StateSource(state), StringComparison.Ordinal);
        }
        var example = Example("data-table", "Typed payments data table", "Filter, sort, select, hide columns, and page through a realistic typed payment collection.", Source(), preview, [Select("data-table-toolbar-mode", "Toolbar mode", toolbarMode.ToString(), Enum.GetNames<ShadcnDataTableToolbarMode>(), value => toolbarMode = Enum.Parse<ShadcnDataTableToolbarMode>(value)), Toggle("data-table-loading", "Loading", v => loading = v), Toggle("data-table-error", "Error", v => error = v), Toggle("data-table-empty", "Empty", v => empty = v), Toggle("data-table-manual", "Manual paging", v => manual = v)], ["sort", "filter", "selection", "pagination", "visibility", "compact-toolbar", "row-actions", "manual", "loading", "empty", "error", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }

    private static RenderFragment MoreIcon => x =>
    {
        x.OpenElement(0, "svg"); x.AddAttribute(1, "viewBox", "0 0 16 16"); x.AddAttribute(2, "aria-hidden", "true"); x.AddAttribute(3, "focusable", "false");
        x.OpenElement(4, "circle"); x.AddAttribute(5, "cx", "3"); x.AddAttribute(6, "cy", "8"); x.AddAttribute(7, "r", "1"); x.CloseElement();
        x.OpenElement(8, "circle"); x.AddAttribute(9, "cx", "8"); x.AddAttribute(10, "cy", "8"); x.AddAttribute(11, "r", "1"); x.CloseElement();
        x.OpenElement(12, "circle"); x.AddAttribute(13, "cx", "13"); x.AddAttribute(14, "cy", "8"); x.AddAttribute(15, "r", "1"); x.CloseElement(); x.CloseElement();
    };

    private static string StateSource(ShadcnDataTableState state)
    {
        var properties = new List<string>();
        if (!string.IsNullOrWhiteSpace(state.Query)) properties.Add($"Query = \"{state.Query.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"");
        if (state.Sorts.Count > 0) properties.Add($"Sorts = [{string.Join(", ", state.Sorts.Select(sort => $"new(\"{sort.ColumnKey}\", ShadcnSortDirection.{sort.Direction})"))}]");
        if (state.ColumnFilters.Count > 0) properties.Add($"ColumnFilters = new Dictionary<string, string> {{ {string.Join(", ", state.ColumnFilters.Select(filter => $"[\"{filter.Key}\"] = \"{filter.Value}\""))} }}");
        if (state.HiddenColumnKeys.Count > 0) properties.Add($"HiddenColumnKeys = new HashSet<string>([{string.Join(", ", state.HiddenColumnKeys.Select(key => $"\"{key}\""))}], StringComparer.Ordinal)");
        if (state.SelectedKeys.Count > 0) properties.Add($"SelectedKeys = new HashSet<string>([{string.Join(", ", state.SelectedKeys.Select(key => $"\"{key}\""))}], StringComparer.Ordinal)");
        if (state.PageIndex > 0) properties.Add($"PageIndex = {state.PageIndex}");
        properties.Add($"PageSize = {state.PageSize}");
        return $"new() {{ {string.Join(", ", properties)} }}";
    }

    private static ComponentExampleDefinition Chart()
    {
        var type = ShadcnChartType.Bar; var loading = false; var hideLegend = false; var stacked = false; var primaryAxis = true; var secondaryAxis = false; var majorGrid = true; var minorGrid = false;
        bool IsCartesian() => type is ShadcnChartType.Bar or ShadcnChartType.Line or ShadcnChartType.Area;
        bool SupportsStacking() => type is ShadcnChartType.Bar or ShadcnChartType.Area;
        IReadOnlyList<string> months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun"];
        IReadOnlyList<ShadcnChartSeries> monthlySeries = [new("desktop", [186, 305, 237, 284, 312, 356]), new("mobile", [80, 200, 120, 168, 190, 224])];
        IReadOnlyList<string> deviceCategories = ["Traffic"];
        IReadOnlyList<ShadcnChartSeries> deviceSeries = [new("desktop", [1680]), new("mobile", [982])];
        var config = new ShadcnChartConfig { ["desktop"] = new("Desktop") { Color = "var(--shadcn-chart-1)" }, ["mobile"] = new("Mobile") { Theme = new("var(--shadcn-chart-2)", "var(--shadcn-chart-4)") } };
        RenderFragment preview = b => { var cartesian = IsCartesian(); b.OpenComponent<ShadcnChart>(0); b.AddAttribute(1, "Class", "showcase-chart-dossier"); b.AddAttribute(2, "Id", "dossier"); b.AddAttribute(3, "Title", cartesian ? "ยอดผู้เข้าชม" : "สัดส่วนผู้เข้าชมตามอุปกรณ์"); b.AddAttribute(4, "Description", cartesian ? "สรุปการเข้าชมเว็บไซต์ 6 เดือนล่าสุด" : "สัดส่วนการเข้าชมรวมจากเดสก์ท็อปและมือถือ"); b.AddAttribute(5, "Type", type); b.AddAttribute(6, "Config", config); b.AddAttribute(7, "Categories", cartesian ? months : deviceCategories); b.AddAttribute(8, "Series", cartesian ? monthlySeries : deviceSeries); b.AddAttribute(9, "Loading", loading); b.AddAttribute(10, "ShowLegend", !hideLegend); b.AddAttribute(11, "ShowPrimaryYAxis", cartesian && primaryAxis); b.AddAttribute(12, "ShowSecondaryYAxis", cartesian && secondaryAxis); b.AddAttribute(13, "ShowMajorGrid", cartesian && majorGrid); b.AddAttribute(14, "ShowMinorGrid", cartesian && minorGrid); b.AddAttribute(15, "BarRadius", 0d); b.AddAttribute(16, "InitialHeight", 260d); b.AddAttribute(17, "Stacked", SupportsStacking() && stacked); b.AddAttribute(18, "LegendInteractive", true); b.AddAttribute(19, "Animated", true); b.CloseComponent(); };
        const string sourceTemplate = """
@using Maliev.ShadcnBlazor.Components.DataDisplay

<ShadcnChart Class="showcase-chart-dossier"
             Id="dossier"
             Title="__TITLE__"
             Description="__DESCRIPTION__"
             Type="ShadcnChartType.__TYPE__"
             Categories="@__CATEGORIES__"
             Series="@__SERIES__"
             Config="@Config"
             Loading="__LOADING__"
             ShowLegend="__SHOW_LEGEND__"
             ShowPrimaryYAxis="__PRIMARY_AXIS__"
             ShowSecondaryYAxis="__SECONDARY_AXIS__"
             ShowMajorGrid="__MAJOR_GRID__"
             ShowMinorGrid="__MINOR_GRID__"
             BarRadius="0"
             InitialHeight="260"
             Stacked="__STACKED__"
             LegendInteractive="true"
             Animated="true" />

@code {
    private readonly IReadOnlyList<string> Months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun"];
    private readonly IReadOnlyList<ShadcnChartSeries> MonthlySeries = [
        new("desktop", [186, 305, 237, 284, 312, 356]),
        new("mobile", [80, 200, 120, 168, 190, 224])
    ];
    private readonly IReadOnlyList<string> DeviceCategories = ["Traffic"];
    private readonly IReadOnlyList<ShadcnChartSeries> DeviceSeries = [
        new("desktop", [1680]),
        new("mobile", [982])
    ];
    private readonly ShadcnChartConfig Config = new()
    {
        ["desktop"] = new("Desktop") { Color = "var(--shadcn-chart-1)" },
        ["mobile"] = new("Mobile") { Theme = new("var(--shadcn-chart-2)", "var(--shadcn-chart-4)") }
    };
}
""";
        string Source() => sourceTemplate
            .Replace("__TITLE__", IsCartesian() ? "ยอดผู้เข้าชม" : "สัดส่วนผู้เข้าชมตามอุปกรณ์", StringComparison.Ordinal)
            .Replace("__DESCRIPTION__", IsCartesian() ? "สรุปการเข้าชมเว็บไซต์ 6 เดือนล่าสุด" : "สัดส่วนการเข้าชมรวมจากเดสก์ท็อปและมือถือ", StringComparison.Ordinal)
            .Replace("__TYPE__", type.ToString(), StringComparison.Ordinal)
            .Replace("__CATEGORIES__", IsCartesian() ? "Months" : "DeviceCategories", StringComparison.Ordinal)
            .Replace("__SERIES__", IsCartesian() ? "MonthlySeries" : "DeviceSeries", StringComparison.Ordinal)
            .Replace("__LOADING__", loading ? "true" : "false", StringComparison.Ordinal)
            .Replace("__SHOW_LEGEND__", hideLegend ? "false" : "true", StringComparison.Ordinal)
            .Replace("__PRIMARY_AXIS__", IsCartesian() && primaryAxis ? "true" : "false", StringComparison.Ordinal)
            .Replace("__SECONDARY_AXIS__", IsCartesian() && secondaryAxis ? "true" : "false", StringComparison.Ordinal)
            .Replace("__MAJOR_GRID__", IsCartesian() && majorGrid ? "true" : "false", StringComparison.Ordinal)
            .Replace("__MINOR_GRID__", IsCartesian() && minorGrid ? "true" : "false", StringComparison.Ordinal)
            .Replace("__STACKED__", SupportsStacking() && stacked ? "true" : "false", StringComparison.Ordinal);
        var example = Example("chart", "Interactive traffic overview", "Compare bar, line, area, pie, and donut charts with type-appropriate controls.", Source(), preview, [Select("chart-type", "Chart type", type.ToString(), Enum.GetNames<ShadcnChartType>(), value => type = Enum.Parse<ShadcnChartType>(value)), Toggle("chart-stacked", "Stacked bars or areas", v => stacked = v, isEnabled: SupportsStacking), Toggle("chart-legend", "Hide legend", v => hideLegend = v), Toggle("chart-loading", "Loading", v => loading = v), Toggle("chart-primary-axis", "Primary axis", v => primaryAxis = v, true, IsCartesian), Toggle("chart-secondary-axis", "Secondary axis", v => secondaryAxis = v, isEnabled: IsCartesian), Toggle("chart-major-grid", "Major grid", v => majorGrid = v, true, IsCartesian), Toggle("chart-minor-grid", "Minor grid", v => minorGrid = v, isEnabled: IsCartesian)], ["bar", "line", "area", "pie", "donut", "axes", "major-grid", "minor-grid", "tooltip", "legend", "theme", "keyboard", "resize", "loading", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Example(string slug, string title, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, "Live package component with caller-owned localized state.", $"<Shadcn{ToPascal(slug)} />", preview, controls, tags);
    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false, Func<bool>? isEnabled = null) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)), isEnabled);
    private static ComponentParameterControl Select(string id, string label, string value, IReadOnlyList<string> options, Action<string> apply) => new(id, label, ComponentParameterControlKind.Select, value, options, apply);
    private static void AddText<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", (RenderFragment)(c => c.AddContent(0, text))); b.CloseComponent(); }
    private static string ToPascal(string value) => string.Concat(value.Split('-').Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
}
