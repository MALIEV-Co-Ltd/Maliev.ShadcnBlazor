using System.Net;
using System.Text;
using Maliev.ShadcnBlazor.Showcase.Theming.Fonts;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class GoogleFontCatalogTests
{
    [Fact]
    public async Task CheckedInSnapshotIsBroadDeterministicAndContainsNoRuntimeCredential()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "samples",
            "Maliev.ShadcnBlazor.Showcase",
            "wwwroot",
            "data",
            "google-fonts-catalog.json");
        var json = await File.ReadAllTextAsync(path);
        using var client = ClientReturning(json);

        var catalog = await GoogleFontCatalog.LoadAsync(client, CancellationToken.None);

        Assert.True(
            catalog.Source == GoogleFontCatalogSource.CheckedInSnapshot,
            catalog.Diagnostic ?? $"Expected {GoogleFontCatalogSource.CheckedInSnapshot}, found {catalog.Source}.");
        Assert.True(catalog.SourceTimestamp > DateTimeOffset.UnixEpoch);
        Assert.True(catalog.Entries.Count >= 100, $"Expected a broad catalog, found {catalog.Entries.Count} entries.");
        Assert.True(catalog.Entries.Count(entry => entry.Subsets.Contains("thai", StringComparer.Ordinal)) >= 10);
        Assert.Equal(
            ["display", "handwriting", "monospace", "sans-serif", "serif"],
            catalog.Entries.Select(entry => entry.Category).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        Assert.Equal(
            catalog.Entries.OrderBy(entry => entry.Family, StringComparer.Ordinal).Select(entry => entry.Id),
            catalog.Entries.Select(entry => entry.Id));
        Assert.All(catalog.Entries, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Id));
            Assert.False(string.IsNullOrWhiteSpace(entry.Family));
            Assert.False(string.IsNullOrWhiteSpace(entry.Css2FamilyQuery));
            Assert.NotEmpty(entry.Subsets);
            Assert.True(entry.Weights.Count > 0 || entry.Axes.Count > 0);
            Assert.DoesNotContain("http", entry.Css2FamilyQuery, StringComparison.OrdinalIgnoreCase);
        });
        Assert.DoesNotContain("api_key", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("apikey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fonts.googleapis.com", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("@font-face", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchIsCaseAndDiacriticInsensitiveAndSupportsSubsetAndVariableFilters()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "source": "google-webfonts-developer-api",
          "sourceTimestamp": "2026-08-22T00:00:00Z",
          "families": [
            {
              "id": "archivo",
              "family": "Archivo",
              "category": "sans-serif",
              "subsets": ["latin"],
              "weights": [400, 700],
              "axes": [],
              "css2FamilyQuery": "Archivo:wght@400;700"
            },
            {
              "id": "noto-serif-thai",
              "family": "Noto Sérif Thai",
              "category": "serif",
              "subsets": ["latin", "thai"],
              "weights": [400, 700],
              "axes": [{ "tag": "wght", "minimum": 100, "maximum": 900 }],
              "css2FamilyQuery": "Noto+Serif+Thai:wght@100..900"
            }
          ]
        }
        """;
        using var client = ClientReturning(json);
        var catalog = await GoogleFontCatalog.LoadAsync(client, CancellationToken.None);

        Assert.Contains(catalog.Search("serif", null, false), entry => entry.Id == "noto-serif-thai");
        Assert.Contains(catalog.Search("SERIF", null, false), entry => entry.Id == "noto-serif-thai");
        Assert.Contains(catalog.Search("Sérif", "thai", true), entry => entry.Id == "noto-serif-thai");
        Assert.DoesNotContain(catalog.Search(string.Empty, "thai", false), entry => entry.Id == "archivo");
        Assert.DoesNotContain(catalog.Search(string.Empty, null, true), entry => entry.Id == "archivo");
    }

    [Fact]
    public async Task NetworkAndSnapshotFailuresReturnBundledOfflineDefaults()
    {
        foreach (var handler in new HttpMessageHandler[]
        {
            new ThrowingHandler(new HttpRequestException("offline")),
            new StaticHandler("{not-json")
        })
        {
            using var client = new HttpClient(handler) { BaseAddress = new Uri("https://showcase.invalid/") };

            var catalog = await GoogleFontCatalog.LoadAsync(client, CancellationToken.None);

            Assert.Equal(GoogleFontCatalogSource.BundledFallback, catalog.Source);
            Assert.Equal(["geist", "jetbrains-mono", "noto-sans-thai"], catalog.Entries.Select(entry => entry.Id).Order(StringComparer.Ordinal));
            Assert.All(catalog.Entries, entry => Assert.True(entry.IsBundled));
            Assert.False(string.IsNullOrWhiteSpace(catalog.Diagnostic));
        }
    }

    [Fact]
    public async Task CallerCancellationIsNeverConvertedIntoFallbackState()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        using var client = new HttpClient(new ThrowingHandler(new OperationCanceledException(source.Token)))
        {
            BaseAddress = new Uri("https://showcase.invalid/")
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            GoogleFontCatalog.LoadAsync(client, source.Token).AsTask());
    }

    [Fact]
    public async Task CatalogServiceCachesTheSnapshotAndBuildsOnlyAValidatedSelectedCss2Request()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "source": "google-webfonts-developer-api",
          "sourceTimestamp": "2026-08-22T00:00:00Z",
          "families": [
            { "id": "dm-sans", "family": "DM Sans", "category": "sans-serif", "subsets": ["latin"], "weights": [400, 700], "axes": [], "css2FamilyQuery": "DM+Sans:wght@400;700" },
            { "id": "noto-sans-thai", "family": "Noto Sans Thai", "category": "sans-serif", "subsets": ["thai"], "weights": [400, 700], "axes": [], "css2FamilyQuery": "Noto+Sans+Thai:wght@400;700" }
          ]
        }
        """;
        var handler = new StaticHandler(json);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://showcase.invalid/") };
        var service = new GoogleFontCatalogService(client);
        var document = ShadcnThemeDocumentSerializer.Deserialize(
            ShadcnThemeSerializer.Serialize(ShadcnThemePresets.BaseVegaNeutral.CreateTheme()));
        var typography = document.Typography with
        {
            Body = new ShadcnFontSelection("'DM Sans', sans-serif", "sans-serif", "dm-sans"),
            ThaiFallback = new ShadcnFontSelection("'Noto Sans Thai', sans-serif", "sans-serif", "noto-sans-thai"),
        };

        var first = await service.CreateStylesheetAsync(typography);
        var second = await service.CreateStylesheetAsync(typography);

        Assert.Equal(first, second);
        Assert.Equal("https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;700&display=swap", first);
        Assert.Equal(1, handler.RequestCount);
        Assert.DoesNotContain("key", first, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FontLoaderAllowsOnlyCss2GoogleStylesheetsAndBoundsFailureTime()
    {
        var script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "samples",
            "Maliev.ShadcnBlazor.Showcase",
            "wwwroot",
            "js",
            "theme-studio.js"));

        Assert.Contains("url.protocol !== \"https:\"", script, StringComparison.Ordinal);
        Assert.Contains("url.hostname !== \"fonts.googleapis.com\"", script, StringComparison.Ordinal);
        Assert.Contains("url.pathname !== \"/css2\"", script, StringComparison.Ordinal);
        Assert.Contains("Math.max(1000, Math.min(Number(timeoutMs) || 5000, 10000))", script, StringComparison.Ordinal);
        Assert.Contains("existing?.remove()", script, StringComparison.Ordinal);
        Assert.DoesNotContain("api_key", script, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpClient ClientReturning(string json) => new(new StaticHandler(json))
    {
        BaseAddress = new Uri("https://showcase.invalid/")
    };

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Maliev.ShadcnBlazor.slnx")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private sealed class StaticHandler(string content) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(CreateResponse());

        private HttpResponseMessage CreateResponse()
        {
            RequestCount++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(exception);
    }
}
