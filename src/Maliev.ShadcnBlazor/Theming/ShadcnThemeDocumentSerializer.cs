using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.ShadcnBlazor.Theming.Internal;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Serializes, validates, and migrates portable theme documents.</summary>
public static class ShadcnThemeDocumentSerializer
{
    private static readonly System.Text.UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 32,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    /// <summary>Serializes a validated canonical schema-version-2 document.</summary>
    /// <param name="document">The document to serialize.</param>
    /// <returns>Canonical UTF-16 JSON with LF line endings.</returns>
    public static string Serialize(ShadcnThemeDocument document)
    {
        EnsureValid(document);
        var json = JsonSerializer.Serialize(document, Options).Replace("\r\n", "\n", StringComparison.Ordinal);
        return json.EndsWith('\n') ? json : json + "\n";
    }

    /// <summary>Deserializes a canonical or supported legacy JSON document.</summary>
    /// <param name="json">The JSON document.</param>
    /// <returns>The canonical schema-version-2 document.</returns>
    public static ShadcnThemeDocument Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        try
        {
            return Deserialize(StrictUtf8.GetBytes(json));
        }
        catch (System.Text.EncoderFallbackException exception)
        {
            throw new JsonException("Theme document must contain valid Unicode text.", exception);
        }
    }

    /// <summary>Deserializes canonical or supported legacy UTF-8 JSON.</summary>
    /// <param name="utf8Json">The UTF-8 JSON bytes.</param>
    /// <returns>The canonical schema-version-2 document.</returns>
    public static ShadcnThemeDocument Deserialize(ReadOnlySpan<byte> utf8Json)
    {
        if (utf8Json.IsEmpty)
            throw new JsonException("Theme document JSON is empty.");
        EnsureNoDuplicateProperties(utf8Json);
        using var parsed = JsonDocument.Parse(utf8Json.ToArray(), new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 32
        });
        var root = parsed.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("A theme document must be a JSON object.");

        var hasTheme = root.TryGetProperty("theme", out _);
        var hasRawTheme = root.TryGetProperty("light", out _) || root.TryGetProperty("dark", out _) || root.TryGetProperty("metrics", out _);
        if (hasTheme && hasRawTheme)
            throw new JsonException("Theme document shape is ambiguous because it contains both nested and raw theme members.");

        var schemaVersion = ReadSchemaVersion(root);
        ShadcnThemeDocument document;
        if (schemaVersion == ShadcnThemeDocument.CurrentSchemaVersion)
        {
            if (!hasTheme)
                throw new JsonException("Canonical theme document must contain theme.");
            document = root.Deserialize<ShadcnThemeDocument>(Options)
                       ?? throw new JsonException("Theme document JSON produced no value.");
        }
        else if (hasTheme)
        {
            if (schemaVersion != 1)
                throw new NotSupportedException($"Theme Studio generator schema version {schemaVersion} is not supported.");
            document = ShadcnThemeDocumentMigrator.FromGeneratorConfigV1(root, Options);
        }
        else if (hasRawTheme)
        {
            var rawTheme = ShadcnThemeSerializer.Deserialize(root.GetRawText());
            document = ShadcnThemeDocumentMigrator.FromTheme(rawTheme);
        }
        else
        {
            throw new NotSupportedException($"Theme document schema version {schemaVersion} is not supported.");
        }

        EnsureValid(document);
        return document;
    }

    private static int ReadSchemaVersion(JsonElement root)
    {
        if (!root.TryGetProperty("schemaVersion", out var version))
            return 0;
        if (version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var value))
            throw new JsonException("schemaVersion must be an integer.");
        return value;
    }

    private static void EnsureValid(ShadcnThemeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var result = ShadcnThemeDocumentValidator.Validate(document);
        if (!result.IsValid)
            throw new JsonException("Theme document is invalid: " +
                string.Join("; ", result.Errors.Select(error => $"{error.Code} at {error.Path}: {error.Message}")));
    }

    private static void EnsureNoDuplicateProperties(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json, new JsonReaderOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 32
        });
        var scopes = new Stack<HashSet<string>?>();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    scopes.Push(new HashSet<string>(StringComparer.Ordinal));
                    break;
                case JsonTokenType.StartArray:
                    scopes.Push(null);
                    break;
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                    scopes.Pop();
                    break;
                case JsonTokenType.PropertyName:
                    var properties = scopes.Peek();
                    var propertyName = reader.GetString()!;
                    if (properties is not null && !properties.Add(propertyName))
                        throw new JsonException($"Theme document contains duplicate property '{propertyName}'.");
                    break;
            }
        }
    }
}
