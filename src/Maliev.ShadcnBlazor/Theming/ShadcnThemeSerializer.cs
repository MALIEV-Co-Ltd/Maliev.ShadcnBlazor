using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Theming;

public static class ShadcnThemeSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    public static string Serialize(ShadcnTheme theme)
    {
        ShadcnThemeCssWriter.EnsureValid(theme);
        var json = JsonSerializer.Serialize(theme, Options).Replace("\r\n", "\n", StringComparison.Ordinal);
        return json.EndsWith('\n') ? json : json + "\n";
    }

    public static ShadcnTheme Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("A Shadcn theme must be a JSON object.");

        var schemaVersion = ReadSchemaVersion(document.RootElement);
        if (schemaVersion is not (0 or ShadcnTheme.CurrentSchemaVersion))
            throw new NotSupportedException($"Theme schema version {schemaVersion} is not supported.");

        var theme = JsonSerializer.Deserialize<ShadcnTheme>(json, Options)
                    ?? throw new JsonException("Theme JSON produced no value.");
        if (schemaVersion == 0)
            theme = theme with { SchemaVersion = ShadcnTheme.CurrentSchemaVersion };

        var validation = ShadcnThemeValidator.Validate(theme);
        if (!validation.IsValid)
        {
            throw new JsonException(
                "Theme JSON is invalid: " +
                string.Join("; ", validation.Errors.Select(error => $"{error.Path}: {error.Message}")));
        }

        return theme;
    }

    private static int ReadSchemaVersion(JsonElement root)
    {
        if (!root.TryGetProperty("schemaVersion", out var property))
            return 0;
        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out var version))
            throw new JsonException("schemaVersion must be an integer.");
        return version;
    }
}
