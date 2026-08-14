using Bunit;
using Maliev.ShadcnBlazor.Components.DataDisplay;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.DataDisplay;

public sealed class DataTableTests : BunitContext
{
    private static readonly Payment[] Rows =
    [
        new("1", "somchai@maliev.com", "processing", 837),
        new("2", "anong@maliev.com", "success", 242),
        new("3", "niran@maliev.com", "failed", 316)
    ];

    private static readonly ShadcnDataTableColumn<Payment>[] Columns =
    [
        new("status", "สถานะ", row => row.Status) { Filterable = true },
        new("email", "อีเมล", row => row.Email) { Sortable = true, Filterable = true },
        new("amount", "จำนวนเงิน", row => row.Amount) { Sortable = true, Hideable = true }
    ];

    [Fact]
    public void DataTableRendersTypedRowsAndAllSemanticStates()
    {
        var cut = RenderTable();

        Assert.NotNull(cut.Find("[data-slot='data-table']"));
        Assert.Equal(3, cut.FindAll("tbody tr[data-slot='table-row']").Count);
        Assert.Equal("none", cut.Find("button[data-column='email']").GetAttribute("data-sort"));
        Assert.Equal("none", cut.Find("th[data-column='email']").GetAttribute("aria-sort"));
        Assert.Equal("0 จาก 3 แถวถูกเลือก", cut.Find("[data-slot='data-table-selection-summary']").TextContent);
        Assert.Equal("หน้า 1 จาก 1", cut.Find("[data-slot='data-table-page-summary']").TextContent);

        cut.Render(parameters => parameters
            .Add(component => component.Loading, true)
            .Add(component => component.LoadingContent, Text("กำลังโหลด")));
        Assert.Equal("status", cut.Find("[data-slot='data-table-loading']").GetAttribute("role"));
        Assert.Equal("กำลังโหลด", cut.Find("[data-slot='data-table-loading']").TextContent);

        cut.Render(parameters => parameters
            .Add(component => component.Loading, false)
            .Add(component => component.Error, "โหลดไม่สำเร็จ"));
        Assert.Equal("alert", cut.Find("[data-slot='data-table-error']").GetAttribute("role"));

        cut.Render(parameters => parameters
            .Add(component => component.Error, (string?)null)
            .Add(component => component.Items, Array.Empty<Payment>()));
        Assert.Equal("ไม่พบผลลัพธ์", cut.Find("[data-slot='data-table-empty']").TextContent);
    }

    [Fact]
    public void SortFilterSelectionVisibilityAndPaginationPublishControlledState()
    {
        var states = new List<ShadcnDataTableState>();
        var state = new ShadcnDataTableState { PageSize = 1 };
        var cut = RenderTable(state, next => states.Add(next));

        cut.Find("button[data-column='email']").Click();
        Assert.Equal(ShadcnSortDirection.Ascending, Assert.Single(states[^1].Sorts).Direction);
        cut.Render(parameters => parameters.Add(component => component.State, states[^1]));
        cut.Find("button[data-column='email']").Click();
        Assert.Equal(ShadcnSortDirection.Descending, Assert.Single(states[^1].Sorts).Direction);

        cut.Find("input[data-slot='data-table-filter']").Input("niran");
        Assert.Equal("niran", states[^1].Query);

        cut.Render(parameters => parameters.Add(component => component.State, state));
        cut.Find("input[data-row-key='1']").Change(true);
        Assert.Contains("1", states[^1].SelectedKeys);

        cut.Render(parameters => parameters.Add(component => component.State, state));
        cut.Find("input[data-column-visibility='amount']").Change(false);
        Assert.Contains("amount", states[^1].HiddenColumnKeys);

        cut.Render(parameters => parameters.Add(component => component.State, state));
        cut.Find("button[data-slot='data-table-next']").Click();
        Assert.Equal(1, states[^1].PageIndex);
    }

    [Fact]
    public void SelectAllHasNamedTriStateAndSkipsDisabledRows()
    {
        var states = new List<ShadcnDataTableState>();
        var cut = RenderTable(new ShadcnDataTableState { PageSize = 10, SelectedKeys = new HashSet<string>(["1"]) }, next => states.Add(next), row => row.Status != "failed");
        var selectAll = cut.Find("input[data-slot='data-table-select-all']");
        Assert.Equal("mixed", selectAll.GetAttribute("aria-checked"));
        cut.Find("input[data-row-key='3']");
        Assert.True(cut.Find("input[data-row-key='3']").HasAttribute("disabled"));

        selectAll.Change(true);
        Assert.Equal(["1", "2"], states[^1].SelectedKeys.Order());
    }

    [Fact]
    public void ManualModePublishesQueryWithoutLocallyRemovingCallerRows()
    {
        var states = new List<ShadcnDataTableState>();
        var cut = RenderTable(new ShadcnDataTableState { Query = "absent", PageSize = 2, PageIndex = 1 }, next => states.Add(next), manual: true, totalCount: 8);

        Assert.Equal(3, cut.FindAll("tbody tr[data-slot='table-row']").Count);
        Assert.Equal("หน้า 2 จาก 4", cut.Find("[data-slot='data-table-page-summary']").TextContent);
    }

    [Fact]
    public void RowActionReceivesTheOriginalTypedItem()
    {
        Payment? received = null;
        var cut = RenderTable(rowAction: item => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "data-row-action", item.Id);
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => received = item));
            builder.AddContent(3, "เปิด");
            builder.CloseElement();
        });

        cut.Find("button[data-row-action='2']").Click();
        Assert.Equal("2", received?.Id);
    }

    [Fact]
    public void ControlledStateRequiresAChangeOwner()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDataTable<Payment>>(parameters => parameters
            .Add(component => component.Items, Rows)
            .Add(component => component.Columns, Columns)
            .Add(component => component.RowKey, row => row.Id)
            .Add(component => component.State, new ShadcnDataTableState())));
    }

    [Fact]
    public void PageSizeFirstLastAndColumnLayoutPublishState()
    {
        ShadcnDataTableColumn<Payment>[] extended =
        [
            new("status", "สถานะ", row => row.Status) { Filterable = true, Order = 2, Width = "12rem" },
            new("email", "อีเมล", row => row.Email) { Sortable = true, Filterable = true, Order = 1 },
            new("amount", "จำนวนเงิน", row => row.Amount) { Sortable = true, Hideable = true, Order = 0, Alignment = ShadcnTableAlignment.End }
        ];
        var states = new List<ShadcnDataTableState>();
        var cut = Render<ShadcnDataTable<Payment>>(parameters => parameters
            .Add(component => component.Items, Rows)
            .Add(component => component.Columns, extended)
            .Add(component => component.RowKey, row => row.Id)
            .Add(component => component.StateChanged, EventCallback.Factory.Create<ShadcnDataTableState>(this, value => states.Add(value))));
        Assert.Equal("amount", cut.FindAll("th[data-column]")[0].GetAttribute("data-column"));
        Assert.Contains("text-align: end", cut.Find("th[data-column='amount']").GetAttribute("style"));
        Assert.Contains("width: 12rem", cut.Find("th[data-column='status']").GetAttribute("style"));
        cut.Find("select[data-slot='data-table-page-size']").Change("25"); Assert.Equal(25, states[^1].PageSize);
        cut.Find("button[data-slot='data-table-last']").Click(); Assert.True(states[^1].PageIndex >= 0);
        cut.Find("button[data-slot='data-table-first']").Click(); Assert.Equal(0, states[^1].PageIndex);
    }

    [Fact]
    public void ColumnFiltersAndShiftSortPreserveCallerOwnedMultiSort()
    {
        var states = new List<ShadcnDataTableState>();
        var cut = RenderTable(changed: next => states.Add(next));
        cut.Find("input[data-column-filter='status']").Input("success");
        Assert.Equal("success", states[^1].ColumnFilters["status"]);

        cut.Render(parameters => parameters.Add(component => component.State, new ShadcnDataTableState
        {
            Sorts = [new("email", ShadcnSortDirection.Ascending)]
        }));
        cut.Find("button[data-column='amount']").Click(new MouseEventArgs { ShiftKey = true });
        Assert.Equal(["email", "amount"], states[^1].Sorts.Select(sort => sort.ColumnKey));
    }

    [Fact]
    public void ManualModePublishesTheCompleteTypedRequest()
    {
        ShadcnDataTableRequest? request = null;
        var state = new ShadcnDataTableState { Query = "งาน", PageSize = 2, Sorts = [new("email", ShadcnSortDirection.Descending)] };
        var cut = Render<ShadcnDataTable<Payment>>(parameters => parameters
            .Add(component => component.Items, Rows.Take(2).ToArray())
            .Add(component => component.Columns, Columns)
            .Add(component => component.RowKey, row => row.Id)
            .Add(component => component.Manual, true)
            .Add(component => component.TotalCount, 8)
            .Add(component => component.State, state)
            .Add(component => component.StateChanged, EventCallback.Factory.Create<ShadcnDataTableState>(this, _ => { }))
            .Add(component => component.RequestChanged, EventCallback.Factory.Create<ShadcnDataTableRequest>(this, value => request = value)));

        cut.Find("button[data-slot='data-table-next']").Click();
        Assert.NotNull(request);
        Assert.Equal("งาน", request.Query);
        Assert.Equal(1, request.PageIndex);
        Assert.Equal("email", Assert.Single(request.Sorts).ColumnKey);
    }

    [Fact]
    public void ManualSelectionSummaryUsesServerTotalAndAllCallerSelectedKeys()
    {
        var cut = RenderTable(new ShadcnDataTableState { PageSize = 2, SelectedKeys = new HashSet<string>(["1", "outside-page"]) }, manual: true, totalCount: 8);
        Assert.Equal("2 จาก 8 แถวถูกเลือก", cut.Find("[data-slot='data-table-selection-summary']").TextContent);
    }

    [Fact]
    public void FilteredPageClampIsPublishedAfterRowsReconcile()
    {
        var states = new List<ShadcnDataTableState>();
        RenderTable(new ShadcnDataTableState { Query = "niran", PageIndex = 2, PageSize = 1 }, next => states.Add(next));
        Assert.Contains(states, state => state.PageIndex == 0);
    }

    private IRenderedComponent<ShadcnDataTable<Payment>> RenderTable(
        ShadcnDataTableState? state = null,
        Action<ShadcnDataTableState>? changed = null,
        Func<Payment, bool>? canSelect = null,
        bool manual = false,
        int totalCount = 0,
        RenderFragment<Payment>? rowAction = null) => Render<ShadcnDataTable<Payment>>(parameters => parameters
            .Add(component => component.Items, Rows)
            .Add(component => component.Columns, Columns)
            .Add(component => component.RowKey, row => row.Id)
            .Add(component => component.State, state)
            .Add(component => component.StateChanged, EventCallback.Factory.Create(this, changed ?? (_ => { })))
            .Add(component => component.CanSelect, canSelect)
            .Add(component => component.Manual, manual)
            .Add(component => component.TotalCount, totalCount)
            .Add(component => component.FilterPlaceholder, "กรองอีเมล...")
            .Add(component => component.EmptyText, "ไม่พบผลลัพธ์")
            .Add(component => component.SelectAllLabel, "เลือกทั้งหมด")
            .Add(component => component.SelectRowLabel, row => $"เลือก {row.Email}")
            .Add(component => component.SelectionSummary, (selected, total) => $"{selected} จาก {total} แถวถูกเลือก")
            .Add(component => component.PageSummary, (page, pages) => $"หน้า {page} จาก {pages}")
            .Add(component => component.RowActionTemplate, rowAction));

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
    private sealed record Payment(string Id, string Email, string Status, decimal Amount);
}
