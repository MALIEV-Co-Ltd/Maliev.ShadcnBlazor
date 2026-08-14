using System.Text;
using System.Text.Json;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Export;

public sealed class ThemeImportService
{
    public const int MaxImportBytes = 1_048_576;

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "text/json"
    };

    public ThemeImportResult Import(ReadOnlySpan<byte> bytes, string fileName, string contentType)
    {
        if (bytes.Length > MaxImportBytes)
            return ThemeImportResult.Failure($"Theme JSON exceeds the maximum import size of {MaxImportBytes} bytes.");

        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            !string.Equals(Path.GetExtension(fileName), ".json", StringComparison.OrdinalIgnoreCase))
            return ThemeImportResult.Failure("Theme import has the wrong file extension; select a .json file.");

        var normalizedContentType = (contentType ?? string.Empty).Split(';', 2)[0].Trim();
        if (!SupportedContentTypes.Contains(normalizedContentType))
            return ThemeImportResult.Failure("Theme import content type must be application/json or text/json.");

        string json;
        try
        {
            json = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return ThemeImportResult.Failure("Theme import must contain valid UTF-8 text.");
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var legacy = document.RootElement.ValueKind == JsonValueKind.Object &&
                         !document.RootElement.TryGetProperty("schemaVersion", out _);
            var theme = ShadcnThemeSerializer.Deserialize(json);
            return legacy
                ? ThemeImportResult.Success(theme, $"Legacy theme schema 0 was migrated to schema {ShadcnTheme.CurrentSchemaVersion}.")
                : ThemeImportResult.Success(theme, $"Theme schema {theme.SchemaVersion} passed parsing and validation.");
        }
        catch (NotSupportedException exception)
        {
            return ThemeImportResult.Failure(exception.Message);
        }
        catch (JsonException exception) when (
            exception.Message.Contains("could not be mapped", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("unmapped", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeImportResult.Failure($"Theme JSON contains an unmapped field. {exception.Message}");
        }
        catch (JsonException exception) when (exception.Message.Contains("is invalid", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeImportResult.Failure(exception.Message);
        }
        catch (JsonException exception)
        {
            return ThemeImportResult.Failure($"Theme import contains malformed JSON. {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            return ThemeImportResult.Failure($"Theme import is invalid. {exception.Message}");
        }
    }
}
