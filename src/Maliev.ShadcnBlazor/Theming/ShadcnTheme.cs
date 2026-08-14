using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Theming;

public sealed record ShadcnTheme
{
    public const int CurrentSchemaVersion = 1;

    [JsonPropertyOrder(0)]
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    [JsonPropertyOrder(1)]
    public required string Name { get; init; }

    [JsonPropertyOrder(2)]
    public required ShadcnColorScheme Light { get; init; }

    [JsonPropertyOrder(3)]
    public required ShadcnColorScheme Dark { get; init; }

    [JsonPropertyOrder(4)]
    public required ShadcnThemeMetrics Metrics { get; init; }

    internal ShadcnTheme DeepClone() => this with
    {
        Light = Light with { },
        Dark = Dark with { },
        Metrics = Metrics with { }
    };
}
