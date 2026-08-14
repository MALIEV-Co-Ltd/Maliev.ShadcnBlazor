using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Components.DataDisplay;

/// <summary>Defines the direction applied by a data-table sort descriptor.</summary>
public enum ShadcnSortDirection { Ascending, Descending }

/// <summary>Defines the aggregate state of selectable rows on the current page.</summary>
public enum ShadcnSelectionState { Unchecked, Indeterminate, Checked }

/// <summary>Defines logical cell alignment that follows the document direction.</summary>
public enum ShadcnTableAlignment { Start, Center, End }

/// <summary>Identifies one ordered sort operation.</summary>
public sealed record ShadcnDataTableSort(string ColumnKey, ShadcnSortDirection Direction);

/// <summary>Describes a typed data-table column without coupling consumers to a JavaScript grid engine.</summary>
public sealed class ShadcnDataTableColumn<TItem>
{
    /// <summary>Creates a column with a stable key, localized label, and typed value accessor.</summary>
    public ShadcnDataTableColumn(string key, string label, Func<TItem, object?> value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Column key is required.", nameof(key));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Column label is required.", nameof(label));
        Key = key.Trim();
        Label = label;
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Gets the stable column key.</summary>
    public string Key { get; }
    /// <summary>Gets the caller-localized column label.</summary>
    public string Label { get; }
    /// <summary>Gets the value accessor used by default rendering, sorting, and filtering.</summary>
    public Func<TItem, object?> Value { get; }
    /// <summary>Gets or sets whether the column may be sorted.</summary>
    public bool Sortable { get; init; }
    /// <summary>Gets or sets whether the global or column filter searches this column.</summary>
    public bool Filterable { get; init; }
    /// <summary>Gets or sets whether the consumer may hide this column.</summary>
    public bool Hideable { get; init; } = true;
    /// <summary>Gets or sets an optional comparer for column values.</summary>
    public IComparer<object?>? Comparer { get; init; }
    /// <summary>Gets or sets an optional column-filter predicate.</summary>
    public Func<TItem, string, bool>? Filter { get; init; }
    /// <summary>Gets or sets custom header content.</summary>
    public RenderFragment? HeaderTemplate { get; init; }
    /// <summary>Gets or sets custom typed cell content.</summary>
    public RenderFragment<TItem>? CellTemplate { get; init; }
    /// <summary>Gets the stable display order. Lower values render first.</summary>
    public int Order { get; init; }
    /// <summary>Gets an optional CSS width for the column.</summary>
    public string? Width { get; init; }
    /// <summary>Gets an optional CSS minimum width for the column.</summary>
    public string? MinWidth { get; init; }
    /// <summary>Gets an optional CSS maximum width for the column.</summary>
    public string? MaxWidth { get; init; }
    /// <summary>Gets the logical cell alignment.</summary>
    public ShadcnTableAlignment Alignment { get; init; } = ShadcnTableAlignment.Start;
    /// <summary>Gets an optional localized per-column filter label.</summary>
    public string? FilterPlaceholder { get; init; }
}

/// <summary>Represents caller-controllable data-table state.</summary>
public sealed record ShadcnDataTableState
{
    /// <summary>Gets the global filter query.</summary>
    public string Query { get; init; } = string.Empty;
    /// <summary>Gets ordered sort descriptors.</summary>
    public IReadOnlyList<ShadcnDataTableSort> Sorts { get; init; } = [];
    /// <summary>Gets per-column filter queries.</summary>
    public IReadOnlyDictionary<string, string> ColumnFilters { get; init; } = new Dictionary<string, string>();
    /// <summary>Gets stable hidden-column keys.</summary>
    public IReadOnlySet<string> HiddenColumnKeys { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    /// <summary>Gets stable selected-row keys.</summary>
    public IReadOnlySet<string> SelectedKeys { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    /// <summary>Gets the zero-based page index.</summary>
    public int PageIndex { get; init; }
    /// <summary>Gets the number of rows per page.</summary>
    public int PageSize { get; init; } = 10;
}

/// <summary>Represents the complete typed query contract supplied to a manual data source.</summary>
public sealed record ShadcnDataTableRequest(
    string Query,
    IReadOnlyList<ShadcnDataTableSort> Sorts,
    IReadOnlyDictionary<string, string> ColumnFilters,
    IReadOnlySet<string> HiddenColumnKeys,
    int PageIndex,
    int PageSize)
{
    /// <summary>Creates an immutable request snapshot from caller-owned state.</summary>
    public static ShadcnDataTableRequest FromState(ShadcnDataTableState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return new(state.Query, state.Sorts.ToArray(), new Dictionary<string, string>(state.ColumnFilters, StringComparer.Ordinal), new HashSet<string>(state.HiddenColumnKeys, StringComparer.Ordinal), state.PageIndex, state.PageSize);
    }
}

/// <summary>Contains a deterministic data-table projection.</summary>
public sealed record ShadcnDataTableResult<TItem>(
    IReadOnlyList<TItem> FilteredRows,
    IReadOnlyList<TItem> PageRows,
    int TotalCount,
    int PageCount,
    int PageIndex,
    IReadOnlySet<string> SelectedKeys,
    int FilteredSelectedCount,
    bool AreAllPageRowsSelected,
    bool AreSomePageRowsSelected);

/// <summary>Applies the package's deterministic in-memory data-table state pipeline.</summary>
public static class ShadcnDataTableEngine
{
    /// <summary>Reconciles state against the current rows and columns without discarding valid multi-sort intent.</summary>
    public static ShadcnDataTableState Reconcile<TItem>(IEnumerable<TItem> rows, IReadOnlyList<ShadcnDataTableColumn<TItem>> columns, ShadcnDataTableState state, Func<TItem, string> rowKey)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(rowKey);
        ValidateState(state);
        ValidateColumns(columns);
        var source = rows.ToList();
        ValidateRowKeys(source, rowKey);
        var keys = source.Select(rowKey).ToHashSet(StringComparer.Ordinal);
        var selected = state.SelectedKeys.Where(keys.Contains).ToHashSet(StringComparer.Ordinal);
        var pageCount = Math.Max(1, (int)Math.Ceiling(source.Count / (double)state.PageSize));
        return state with { PageIndex = Math.Min(state.PageIndex, pageCount - 1), SelectedKeys = selected };
    }

    /// <summary>Validates stable column definitions.</summary>
    public static void ValidateColumns<TItem>(IReadOnlyList<ShadcnDataTableColumn<TItem>> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);
        if (columns.Count == 0) throw new ArgumentException("At least one data-table column is required.", nameof(columns));
        var duplicate = columns.GroupBy(column => column.Key, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null) throw new ArgumentException($"Duplicate data-table column key '{duplicate.Key}'.", nameof(columns));
    }

    /// <summary>Filters, stably sorts, and pages caller rows in that order.</summary>
    public static ShadcnDataTableResult<TItem> Evaluate<TItem>(
        IEnumerable<TItem> rows,
        IReadOnlyList<ShadcnDataTableColumn<TItem>> columns,
        ShadcnDataTableState state,
        Func<TItem, string> rowKey,
        Func<TItem, bool>? predicate = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(rowKey);
        ValidateState(state);
        ValidateColumns(columns);
        var byKey = columns.ToDictionary(column => column.Key, StringComparer.Ordinal);
        ValidateDescriptors(state, byKey);
        var source = rows.Select((item, index) => new Indexed<TItem>(item, index)).ToList();
        ValidateRowKeys(source.Select(entry => entry.Item), rowKey);
        var query = Normalize(state.Query);
        IEnumerable<Indexed<TItem>> filtered = source;
        if (predicate is not null) filtered = filtered.Where(entry => predicate(entry.Item));
        if (query.Length > 0)
            filtered = filtered.Where(entry => columns.Where(column => column.Filterable)
                .Any(column => Normalize(column.Value(entry.Item)?.ToString()).Contains(query, StringComparison.Ordinal)));
        foreach (var filter in state.ColumnFilters.Where(pair => !string.IsNullOrWhiteSpace(pair.Value)))
        {
            var column = byKey[filter.Key];
            var needle = Normalize(filter.Value);
            filtered = filtered.Where(entry => column.Filter is not null
                ? column.Filter(entry.Item, filter.Value)
                : Normalize(column.Value(entry.Item)?.ToString()).Contains(needle, StringComparison.Ordinal));
        }
        var projected = filtered.ToList();
        projected.Sort((left, right) => Compare(left, right, state.Sorts, byKey));
        return BuildResult(projected.Select(entry => entry.Item).ToList(), state, rowKey);
    }

    /// <summary>Projects an already queried server page without applying client filtering or sorting.</summary>
    public static ShadcnDataTableResult<TItem> EvaluateManual<TItem>(
        IEnumerable<TItem> pageRows,
        ShadcnDataTableState state,
        int totalCount,
        Func<TItem, string> rowKey)
    {
        ArgumentNullException.ThrowIfNull(pageRows);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(rowKey);
        ValidateState(state);
        if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount));
        var page = pageRows.ToList();
        ValidateRowKeys(page, rowKey);
        var pages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)state.PageSize));
        if (state.PageIndex >= pages) throw new ArgumentOutOfRangeException(nameof(state), "Manual page index must address the supplied total count.");
        var selected = new HashSet<string>(state.SelectedKeys, StringComparer.Ordinal);
        var pageSelected = page.Count(item => selected.Contains(rowKey(item)));
        return new(page, page, totalCount, pages, state.PageIndex, selected, selected.Count, page.Count > 0 && pageSelected == page.Count, pageSelected > 0 && pageSelected < page.Count);
    }

    /// <summary>Toggles every selectable row on the supplied page.</summary>
    public static IReadOnlySet<string> TogglePageSelection<TItem>(IEnumerable<TItem> pageRows, IReadOnlySet<string> selectedKeys, Func<TItem, string> rowKey, Func<TItem, bool>? canSelect = null)
    {
        var eligible = pageRows.Where(item => canSelect?.Invoke(item) != false).Select(rowKey).ToArray();
        var result = new HashSet<string>(selectedKeys, StringComparer.Ordinal);
        var all = eligible.Length > 0 && eligible.All(result.Contains);
        foreach (var key in eligible)
            if (all) result.Remove(key); else result.Add(key);
        return result;
    }

    /// <summary>Returns checked, unchecked, or indeterminate state for selectable page rows.</summary>
    public static ShadcnSelectionState GetPageSelectionState<TItem>(IEnumerable<TItem> pageRows, IReadOnlySet<string> selectedKeys, Func<TItem, string> rowKey, Func<TItem, bool>? canSelect = null)
    {
        var eligible = pageRows.Where(item => canSelect?.Invoke(item) != false).Select(rowKey).ToArray();
        var count = eligible.Count(selectedKeys.Contains);
        return count == 0 ? ShadcnSelectionState.Unchecked : count == eligible.Length ? ShadcnSelectionState.Checked : ShadcnSelectionState.Indeterminate;
    }

    private static ShadcnDataTableResult<TItem> BuildResult<TItem>(IReadOnlyList<TItem> rows, ShadcnDataTableState state, Func<TItem, string> rowKey)
    {
        var pageCount = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)state.PageSize));
        var pageIndex = Math.Min(state.PageIndex, pageCount - 1);
        var page = rows.Skip(pageIndex * state.PageSize).Take(state.PageSize).ToList();
        return SelectionResult(rows, page, rows.Count, pageCount, pageIndex, state.SelectedKeys, rowKey);
    }

    private static ShadcnDataTableResult<TItem> SelectionResult<TItem>(IReadOnlyList<TItem> filtered, IReadOnlyList<TItem> page, int total, int pageCount, int pageIndex, IReadOnlySet<string> selected, Func<TItem, string> rowKey)
    {
        var keys = new HashSet<string>(selected, StringComparer.Ordinal);
        var pageSelected = page.Count(item => keys.Contains(rowKey(item)));
        return new(filtered, page, total, pageCount, pageIndex, keys,
            filtered.Count(item => keys.Contains(rowKey(item))),
            page.Count > 0 && pageSelected == page.Count,
            pageSelected > 0 && pageSelected < page.Count);
    }

    private static int Compare<TItem>(Indexed<TItem> left, Indexed<TItem> right, IReadOnlyList<ShadcnDataTableSort> sorts, IReadOnlyDictionary<string, ShadcnDataTableColumn<TItem>> columns)
    {
        foreach (var sort in sorts)
        {
            var column = columns[sort.ColumnKey];
            var comparison = (column.Comparer ?? ValueComparer.Instance).Compare(column.Value(left.Item), column.Value(right.Item));
            if (comparison != 0) return sort.Direction == ShadcnSortDirection.Ascending ? comparison : -comparison;
        }
        return left.Index.CompareTo(right.Index);
    }

    private static void ValidateDescriptors<TItem>(ShadcnDataTableState state, IReadOnlyDictionary<string, ShadcnDataTableColumn<TItem>> columns)
    {
        foreach (var sort in state.Sorts)
        {
            if (!columns.TryGetValue(sort.ColumnKey, out var column) || !column.Sortable)
                throw new ArgumentException($"Column '{sort.ColumnKey}' is not sortable.", nameof(state));
            if (!Enum.IsDefined(sort.Direction)) throw new ArgumentOutOfRangeException(nameof(state), "Unknown sort direction.");
        }
        foreach (var key in state.ColumnFilters.Keys)
            if (!columns.TryGetValue(key, out var column) || !column.Filterable)
                throw new ArgumentException($"Column '{key}' is not filterable.", nameof(state));
        foreach (var key in state.HiddenColumnKeys)
            if (!columns.TryGetValue(key, out var column) || !column.Hideable)
                throw new ArgumentException($"Column '{key}' cannot be hidden.", nameof(state));
    }

    private static void ValidateState(ShadcnDataTableState state)
    {
        if (state.PageSize < 1 || state.PageSize > 10_000) throw new ArgumentOutOfRangeException(nameof(state.PageSize), state.PageSize, "Page size must be between 1 and 10000.");
        if (state.PageIndex < 0) throw new ArgumentOutOfRangeException(nameof(state.PageIndex));
    }

    private static void ValidateRowKeys<TItem>(IEnumerable<TItem> rows, Func<TItem, string> rowKey)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var key = rowKey(row);
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Every data-table row key must be non-empty.", nameof(rowKey));
            if (!keys.Add(key)) throw new ArgumentException($"Duplicate data-table row key '{key}'.", nameof(rowKey));
        }
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    private sealed record Indexed<TItem>(TItem Item, int Index);
    private sealed class ValueComparer : IComparer<object?>
    {
        internal static readonly ValueComparer Instance = new();
        public int Compare(object? left, object? right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left is null) return 1;
            if (right is null) return -1;
            if (left is string a && right is string b) return StringComparer.OrdinalIgnoreCase.Compare(a, b);
            if (left is IComparable comparable && left.GetType() == right.GetType()) return comparable.CompareTo(right);
            return StringComparer.OrdinalIgnoreCase.Compare(Convert.ToString(left, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture));
        }
    }
}
