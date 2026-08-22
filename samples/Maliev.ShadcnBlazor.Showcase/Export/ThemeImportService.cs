using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Export;

public sealed class ThemeImportService
{
    public const int MaxImportBytes = ShadcnThemeDocumentLoader.MaxDocumentBytes;

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

        try
        {
            using var stream = new MemoryStream(bytes.ToArray(), writable: false);
            var document = ShadcnThemeDocumentLoader.Load(stream);
            return ThemeImportResult.Success(document, $"Theme document schema {document.SchemaVersion} passed parsing and validation.");
        }
        catch (NotSupportedException exception)
        {
            return ThemeImportResult.Failure(exception.Message);
        }
        catch (System.Text.Json.JsonException exception) when (
            exception.Message.Contains("could not be mapped", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("unmapped", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeImportResult.Failure($"Theme JSON contains an unmapped field. {exception.Message}");
        }
        catch (System.Text.Json.JsonException exception) when (exception.Message.Contains("is invalid", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeImportResult.Failure(exception.Message);
        }
        catch (System.Text.Json.JsonException exception)
        {
            return ThemeImportResult.Failure($"Theme import contains malformed JSON. {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            return ThemeImportResult.Failure($"Theme import is invalid. {exception.Message}");
        }
        catch (InvalidDataException exception)
        {
            return ThemeImportResult.Failure(exception.Message);
        }
    }
}
