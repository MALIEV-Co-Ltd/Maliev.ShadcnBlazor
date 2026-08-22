using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Fonts;

internal sealed class GoogleFontCatalogService(HttpClient httpClient)
{
    private readonly object _gate = new();
    private Task<GoogleFontCatalog>? _catalog;

    public Task<GoogleFontCatalog> GetAsync(CancellationToken cancellationToken = default)
    {
        Task<GoogleFontCatalog> catalog;
        lock (_gate)
            catalog = _catalog ??= GoogleFontCatalog.LoadAsync(httpClient, CancellationToken.None).AsTask();
        return catalog.WaitAsync(cancellationToken);
    }

    public async ValueTask<string?> CreateStylesheetAsync(
        ShadcnTypographyScale typography,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typography);
        var ids = new[] { typography.Body.GoogleFontsId, typography.ThaiFallback.GoogleFontsId, typography.Code.GoogleFontsId }
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (ids.Length == 0)
            return null;

        var catalog = await GetAsync(cancellationToken);
        var byId = catalog.Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        var queries = ids
            .Select(id => byId.TryGetValue(id, out var entry) && !entry.IsBundled ? entry.Css2FamilyQuery : null)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();
        return queries.Length == 0
            ? null
            : $"https://fonts.googleapis.com/css2?family={string.Join("&family=", queries)}&display=swap";
    }
}
