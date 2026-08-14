using Maliev.ShadcnBlazor.Components.DataDisplay;

namespace Maliev.ShadcnBlazor.Tests.Components.DataDisplay;

public sealed class DataTableEngineTests
{
    private static readonly Payment[] Rows =
    [
        new("3", "สมชาย@example.com", "processing", 837),
        new("1", "ken@example.com", "success", 316),
        new("2", "Abe@example.com", "success", 242),
        new("4", null, "failed", 721)
    ];

    private static readonly ShadcnDataTableColumn<Payment>[] Columns =
    [
        new("email", "Email", row => row.Email) { Sortable = true, Filterable = true },
        new("status", "Status", row => row.Status) { Sortable = true, Filterable = true },
        new("amount", "Amount", row => row.Amount) { Sortable = true }
    ];

    [Fact]
    public void EvaluationFiltersThaiCaseInsensitivelyThenStableSortsAndPages()
    {
        var thai = ShadcnDataTableEngine.Evaluate(Rows, Columns, new ShadcnDataTableState
        {
            Query = "สมชาย",
            PageSize = 10
        }, row => row.Id);
        Assert.Equal("3", Assert.Single(thai.FilteredRows).Id);

        var result = ShadcnDataTableEngine.Evaluate(Rows, Columns, new ShadcnDataTableState
        {
            Sorts = [new("status", ShadcnSortDirection.Ascending), new("amount", ShadcnSortDirection.Descending)],
            PageSize = 2,
            PageIndex = 1
        }, row => row.Id);

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(2, result.PageCount);
        Assert.Equal(1, result.PageIndex);
        Assert.Equal(["1", "2"], result.PageRows.Select(row => row.Id));
    }

    [Fact]
    public void ColumnFiltersComposeWithGlobalFilterAndCallerPredicate()
    {
        var result = ShadcnDataTableEngine.Evaluate(Rows, Columns, new ShadcnDataTableState
        {
            Query = "example",
            ColumnFilters = new Dictionary<string, string> { ["status"] = "success" },
            PageSize = 10
        }, row => row.Id, row => row.Amount > 250);

        Assert.Equal("1", Assert.Single(result.FilteredRows).Id);
    }

    [Fact]
    public void EvaluationClampsPageAfterFilteringOrDataRefresh()
    {
        var result = ShadcnDataTableEngine.Evaluate(Rows.Take(1), Columns, new ShadcnDataTableState
        {
            PageIndex = 9,
            PageSize = 2
        }, row => row.Id);

        Assert.Equal(0, result.PageIndex);
        Assert.Equal(1, result.PageCount);
    }

    [Fact]
    public void ManualModeUsesCallerRowsAndTotalWithoutPretendingToFilterOrSort()
    {
        var state = new ShadcnDataTableState
        {
            Query = "not-on-this-page",
            PageIndex = 2,
            PageSize = 2,
            Sorts = [new("email", ShadcnSortDirection.Descending)]
        };
        var result = ShadcnDataTableEngine.EvaluateManual(Rows.Take(2), state, 9, row => row.Id);

        Assert.Equal(9, result.TotalCount);
        Assert.Equal(5, result.PageCount);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(["3", "1"], result.PageRows.Select(row => row.Id));
    }

    [Fact]
    public void SelectionUsesStableKeysAcrossSortFilterPageAndRefresh()
    {
        var selected = new HashSet<string>(["1", "3"], StringComparer.Ordinal);
        var result = ShadcnDataTableEngine.Evaluate(Rows.Reverse(), Columns, new ShadcnDataTableState
        {
            Query = "success",
            Sorts = [new("email", ShadcnSortDirection.Ascending)],
            PageSize = 1,
            SelectedKeys = selected
        }, row => row.Id);

        Assert.Equal(["1", "3"], result.SelectedKeys.Order());
        Assert.Equal(1, result.FilteredSelectedCount);
        Assert.False(result.AreAllPageRowsSelected);
        Assert.False(result.AreSomePageRowsSelected);
    }

    [Fact]
    public void SelectPageSkipsDisabledRowsAndProducesIndeterminateState()
    {
        var selected = ShadcnDataTableEngine.TogglePageSelection(
            Rows,
            new HashSet<string>(["1"], StringComparer.Ordinal),
            row => row.Id,
            row => row.Status != "failed");
        Assert.Equal(["1", "2", "3"], selected.Order());

        var partial = ShadcnDataTableEngine.GetPageSelectionState(
            Rows,
            new HashSet<string>(["1"], StringComparer.Ordinal),
            row => row.Id,
            row => row.Status != "failed");
        Assert.Equal(ShadcnSelectionState.Indeterminate, partial);
    }

    [Fact]
    public void DefinitionsRejectInvalidAndDuplicateContracts()
    {
        Assert.Throws<ArgumentException>(() => new ShadcnDataTableColumn<Payment>("", "Email", row => row.Email));
        Assert.Throws<ArgumentException>(() => ShadcnDataTableEngine.ValidateColumns<Payment>(
        [
            new("email", "Email", row => row.Email),
            new("email", "Duplicate", row => row.Email)
        ]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShadcnDataTableEngine.Evaluate(Rows, Columns, new ShadcnDataTableState { PageSize = 0 }, row => row.Id));
        Assert.Throws<ArgumentException>(() => ShadcnDataTableEngine.Evaluate(Rows, Columns, new ShadcnDataTableState { Sorts = [new("missing", ShadcnSortDirection.Ascending)] }, row => row.Id));
    }

    [Fact]
    public void ManualRequestPreservesEveryCallerOwnedQueryDimension()
    {
        var state = new ShadcnDataTableState { Query = "งาน", PageIndex = 2, PageSize = 25, Sorts = [new("email", ShadcnSortDirection.Descending)], ColumnFilters = new Dictionary<string, string> { ["status"] = "success" }, HiddenColumnKeys = new HashSet<string>(["amount"]) };
        var request = ShadcnDataTableRequest.FromState(state);
        Assert.Equal("งาน", request.Query);
        Assert.Equal(2, request.PageIndex);
        Assert.Equal(25, request.PageSize);
        Assert.Equal("email", Assert.Single(request.Sorts).ColumnKey);
        Assert.Equal("success", request.ColumnFilters["status"]);
        Assert.Contains("amount", request.HiddenColumnKeys);
    }

    [Fact]
    public void ReconcileClampsPageAndDropsOnlyMissingKeysWhilePreservingMultiSort()
    {
        var state = new ShadcnDataTableState { PageIndex = 9, PageSize = 2, Sorts = [new("status", ShadcnSortDirection.Ascending), new("amount", ShadcnSortDirection.Descending)], SelectedKeys = new HashSet<string>(["1", "missing"]) };
        var reconciled = ShadcnDataTableEngine.Reconcile(Rows.Take(2), Columns, state, row => row.Id);
        Assert.Equal(0, reconciled.PageIndex);
        Assert.Equal(2, reconciled.Sorts.Count);
        Assert.Equal("1", Assert.Single(reconciled.SelectedKeys));
    }

    private sealed record Payment(string Id, string? Email, string Status, decimal Amount);
}
