namespace Maliev.ShadcnBlazor.Showcase.Documentation;

public sealed class DocumentationNavigationState
{
    private string _query = string.Empty;
    private string? _category;
    private ComponentDocumentationStatus? _status;
    private bool _catalogOpen;
    private bool _themeDockOpen;

    public event EventHandler? Changed;

    public string Query
    {
        get => _query;
        set => SetValue(ref _query, NormalizeWhitespace(value));
    }

    public string? Category
    {
        get => _category;
        set => SetValue(ref _category, NormalizeOptional(value));
    }

    public ComponentDocumentationStatus? Status
    {
        get => _status;
        set
        {
            if (value is not null && !Enum.IsDefined(value.Value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown documentation status.");
            SetValue(ref _status, value);
        }
    }

    public bool CatalogOpen
    {
        get => _catalogOpen;
        set
        {
            if (_catalogOpen == value && (!value || !_themeDockOpen))
                return;

            _catalogOpen = value;
            if (value)
                _themeDockOpen = false;
            OnChanged();
        }
    }

    public bool ThemeDockOpen
    {
        get => _themeDockOpen;
        set
        {
            if (_themeDockOpen == value && (!value || !_catalogOpen))
                return;

            _themeDockOpen = value;
            if (value)
                _catalogOpen = false;
            OnChanged();
        }
    }

    public bool CloseDrawers()
    {
        if (!_catalogOpen && !_themeDockOpen)
            return false;

        _catalogOpen = false;
        _themeDockOpen = false;
        OnChanged();
        return true;
    }

    private void SetValue<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnChanged();
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);

    private static string NormalizeWhitespace(string? value) => string.Join(' ', (value ?? string.Empty)
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string? NormalizeOptional(string? value)
    {
        var normalized = NormalizeWhitespace(value);
        return normalized.Length == 0 ? null : normalized;
    }
}
