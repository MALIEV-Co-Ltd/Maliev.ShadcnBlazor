using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Fonts;

internal enum GoogleFontCatalogSource
{
    CheckedInSnapshot,
    BundledFallback
}

internal sealed class GoogleFontCatalog
{
    private const string SnapshotPath = "data/google-fonts-catalog.json";
    private const int CurrentSchemaVersion = 1;
    private static readonly string[] Categories = ["display", "handwriting", "monospace", "sans-serif", "serif"];
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    private GoogleFontCatalog(
        GoogleFontCatalogSource source,
        DateTimeOffset sourceTimestamp,
        IReadOnlyList<GoogleFontCatalogEntry> entries,
        string? diagnostic)
    {
        Source = source;
        SourceTimestamp = sourceTimestamp;
        Entries = entries;
        Diagnostic = diagnostic;
    }

    public GoogleFontCatalogSource Source { get; }
    public DateTimeOffset SourceTimestamp { get; }
    public IReadOnlyList<GoogleFontCatalogEntry> Entries { get; }
    public string? Diagnostic { get; }

    public static async ValueTask<GoogleFontCatalog> LoadAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        try
        {
            using var response = await httpClient.GetAsync(SnapshotPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var snapshot = await JsonSerializer.DeserializeAsync<GoogleFontCatalogSnapshot>(stream, JsonOptions, cancellationToken)
                           ?? throw new JsonException("Google Fonts catalog snapshot produced no value.");
            Validate(snapshot);
            return new(
                GoogleFontCatalogSource.CheckedInSnapshot,
                snapshot.SourceTimestamp,
                MergeBundledDefaults(snapshot.Families),
                null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException or OperationCanceledException)
        {
            return new(
                GoogleFontCatalogSource.BundledFallback,
                DateTimeOffset.UnixEpoch,
                BundledDefaults,
                $"The checked-in font catalog is unavailable; bundled fonts remain active. {exception.Message}");
        }
    }

    public IReadOnlyList<GoogleFontCatalogEntry> Search(string query, string? subset, bool variableOnly)
    {
        var normalizedQuery = Normalize(query);
        return Entries
            .Where(entry => normalizedQuery.Length == 0 ||
                Normalize(entry.Family).Contains(normalizedQuery, StringComparison.Ordinal) ||
                Normalize(entry.Id).Contains(normalizedQuery, StringComparison.Ordinal))
            .Where(entry => string.IsNullOrWhiteSpace(subset) || entry.Subsets.Contains(subset, StringComparer.OrdinalIgnoreCase))
            .Where(entry => !variableOnly || entry.Axes.Count > 0)
            .ToArray();
    }

    private static IReadOnlyList<GoogleFontCatalogEntry> MergeBundledDefaults(IEnumerable<GoogleFontCatalogEntry> entries)
    {
        var byId = entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        foreach (var bundled in BundledDefaults)
            byId[bundled.Id] = bundled;
        return byId.Values.OrderBy(entry => entry.Family, StringComparer.Ordinal).ToArray();
    }

    private static void Validate(GoogleFontCatalogSnapshot snapshot)
    {
        if (snapshot.SchemaVersion != CurrentSchemaVersion)
            throw new NotSupportedException($"Google Fonts catalog schema version {snapshot.SchemaVersion} is unsupported.");
        if (snapshot.Source is not ("google-webfonts-developer-api" or "google-fonts-public-metadata"))
            throw new JsonException("Google Fonts catalog source is invalid.");
        if (snapshot.SourceTimestamp <= DateTimeOffset.UnixEpoch)
            throw new JsonException("Google Fonts catalog source timestamp is invalid.");
        if (snapshot.Families is null)
            throw new JsonException("Google Fonts catalog families are required.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in snapshot.Families)
        {
            if (!SafeIdentifier(entry.Id) || !ids.Add(entry.Id) || !SafeText(entry.Family) ||
                !Categories.Contains(entry.Category, StringComparer.Ordinal) || entry.Subsets.Count == 0 ||
                entry.Subsets.Any(subset => !SafeIdentifier(subset)) ||
                entry.Weights.Any(weight => weight is < 100 or > 900 || weight % 100 != 0) ||
                entry.Axes.Any(axis => !SafeAxis(axis)) ||
                entry.Weights.Count == 0 && entry.Axes.Count == 0 ||
                !SafeCss2Query(entry.Css2FamilyQuery))
                throw new JsonException($"Google Fonts catalog entry '{entry.Id}' is invalid.");
        }

        var orderedIds = snapshot.Families.OrderBy(entry => entry.Family, StringComparer.Ordinal).Select(entry => entry.Id);
        if (!snapshot.Families.Select(entry => entry.Id).SequenceEqual(orderedIds, StringComparer.Ordinal))
            throw new JsonException("Google Fonts catalog entries must be sorted by family.");
    }

    private static bool SafeAxis(GoogleFontAxis axis) =>
        axis.Tag.Length == 4 && axis.Tag.All(char.IsAsciiLetterOrDigit) &&
        double.IsFinite(axis.Minimum) && double.IsFinite(axis.Maximum) && axis.Minimum <= axis.Maximum;

    private static bool SafeCss2Query(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 300 && !value.Any(char.IsControl) &&
        !value.Contains("http", StringComparison.OrdinalIgnoreCase) &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '+' or ':' or '@' or ';' or '.' or ',');

    private static bool SafeIdentifier(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 100 &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');

    private static bool SafeText(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 200 && !value.Any(char.IsControl) &&
        value.IndexOfAny(['<', '>', '{', '}', ';']) < 0;

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(character));
        }
        return builder.ToString().Normalize(NormalizationForm.FormC).Trim();
    }

    private static IReadOnlyList<GoogleFontCatalogEntry> BundledDefaults { get; } =
    [
        new("geist", "Geist", "sans-serif", ["latin"], [400, 500, 600, 700], [new("wght", 100, 900)], "Geist:wght@100..900", true),
        new("jetbrains-mono", "JetBrains Mono", "monospace", ["latin"], [400, 500, 600, 700], [new("wght", 100, 800)], "JetBrains+Mono:wght@100..800", true),
        new("noto-sans-thai", "Noto Sans Thai", "sans-serif", ["latin", "thai"], [400, 500, 600, 700], [new("wght", 100, 900)], "Noto+Sans+Thai:wght@100..900", true)
    ];

    private sealed record GoogleFontCatalogSnapshot(
        int SchemaVersion,
        string Source,
        DateTimeOffset SourceTimestamp,
        IReadOnlyList<GoogleFontCatalogEntry> Families);
}
