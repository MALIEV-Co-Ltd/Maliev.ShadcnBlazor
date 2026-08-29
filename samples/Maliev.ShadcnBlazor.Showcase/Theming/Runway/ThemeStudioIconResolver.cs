using Maliev.ShadcnBlazor.Components.Icons;
using Maliev.ShadcnBlazor.Icons.Hugeicons;
using Maliev.ShadcnBlazor.Icons.Lucide;
using Maliev.ShadcnBlazor.Icons.Phosphor;
using Maliev.ShadcnBlazor.Icons.Tabler;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

/// <summary>Resolves stable workflow meanings through the icon package selected in Theme Studio.</summary>
public static class ThemeStudioIconResolver
{
    private static readonly ThemeStudioIconCatalogCache Catalogs = new();

    private static readonly IReadOnlyDictionary<string, string[]> Names = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["analytics"] = ["chart-bar", "chart-bar", "chart-bar", "bar-chart"],
        ["alert"] = ["circle-alert", "warning-circle", "alert-circle", "alert-circle"],
        ["camera"] = ["camera", "camera", "camera", "camera-01"],
        ["credentials"] = ["key-round", "key", "key", "key-01"],
        ["data"] = ["database", "database", "database", "database"],
        ["delete"] = ["trash-2", "trash", "trash", "delete-02"],
        ["download"] = ["download", "download-simple", "download", "download-01"],
        ["file"] = ["file", "file", "file", "file-01"],
        ["machine"] = ["factory", "factory", "building-factory", "factory"],
        ["message"] = ["message-circle", "chat-circle", "message-circle", "message-01"],
        ["profile"] = ["user", "user", "user", "user"],
        ["review"] = ["clipboard-check", "clipboard", "clipboard-check", "clipboard"],
        ["shipping"] = ["truck", "truck", "truck-delivery", "truck-delivery"],
        ["close"] = ["x", "x", "x", "cancel-01"],
        ["send"] = ["send", "paper-plane", "send", "sent"],
        ["team"] = ["users", "users-three", "users", "user-group"],
        ["upload"] = ["upload", "upload-simple", "file-upload", "file-upload"]
    };

    /// <summary>Gets an icon for a curated workflow without leaking package-specific names to cards.</summary>
    public static ShadcnIconData Resolve(ThemeStudioIconLibrary library, string workflowId)
    {
        return ResolveSemantic(library, SemanticFor(workflowId));
    }

    /// <summary>Gets an icon for a semantic UI action from the selected companion package.</summary>
    public static ShadcnIconData ResolveSemantic(ThemeStudioIconLibrary library, string semantic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(semantic);
        if (!Names.TryGetValue(semantic, out var names))
        {
            throw new ArgumentOutOfRangeException(nameof(semantic), semantic, "Unknown Theme Studio icon semantic.");
        }

        var packageIndex = library switch
        {
            ThemeStudioIconLibrary.Phosphor => 1,
            ThemeStudioIconLibrary.Tabler => 2,
            ThemeStudioIconLibrary.Hugeicons => 3,
            _ => 0
        };
        return Catalogs.Get(library).Get(names[packageIndex]);
    }

    private static string SemanticFor(string workflowId) => workflowId switch
    {
        "production-analytics" or "quality-trend" or "order-mix" => "analytics",
        "inspection-alerts" or "quality-alert" or "issue-report" => "alert",
        "inspection-camera" => "camera",
        "api-credentials" or "machine-password" or "webhook-secret" => "credentials",
        "inspection-table" or "quotation-data-table" => "data",
        "drawing-attachment" or "file-context" or "drawing-preview" or "customer-proof" => "file",
        "production-capacity" or "machine-cell" or "production-planning-suite" => "machine",
        "conversation-marker" or "assistant-conversation" => "message",
        "operator-profile" or "reviewer-details" or "contact-dialog" => "profile",
        "project-questionnaire" or "quality-release-suite" or "inspection-guidance" => "review",
        "shipping-handoff" or "dispatch-confirmation" or "dispatch-drawer" or "delivery-sheet" or "customer-handoff-suite" => "shipping",
        "assigned-reviewers" => "team",
        "quotation-files" => "upload",
        _ => "review"
    };
}

internal sealed class ThemeStudioIconCatalogCache
{
    private readonly IReadOnlyDictionary<ThemeStudioIconLibrary, Lazy<IShadcnIconCatalog>> catalogs;

    public ThemeStudioIconCatalogCache()
        : this(new Dictionary<ThemeStudioIconLibrary, Func<IShadcnIconCatalog>>
        {
            [ThemeStudioIconLibrary.Lucide] = () => LucideIconCatalog.Instance,
            [ThemeStudioIconLibrary.Tabler] = () => TablerIconCatalog.Instance,
            [ThemeStudioIconLibrary.Phosphor] = () => PhosphorIconCatalog.Instance,
            [ThemeStudioIconLibrary.Hugeicons] = () => HugeiconsIconCatalog.Instance
        })
    {
    }

    internal ThemeStudioIconCatalogCache(IReadOnlyDictionary<ThemeStudioIconLibrary, Func<IShadcnIconCatalog>> factories)
    {
        ArgumentNullException.ThrowIfNull(factories);
        catalogs = factories.ToDictionary(
            pair => pair.Key,
            pair => new Lazy<IShadcnIconCatalog>(pair.Value, LazyThreadSafetyMode.ExecutionAndPublication));
    }

    internal IReadOnlyList<ThemeStudioIconLibrary> LoadedLibraries => catalogs
        .Where(pair => pair.Value.IsValueCreated)
        .Select(pair => pair.Key)
        .ToArray();

    public IShadcnIconCatalog Get(ThemeStudioIconLibrary library)
    {
        var effectiveLibrary = catalogs.ContainsKey(library) ? library : ThemeStudioIconLibrary.Lucide;
        return catalogs[effectiveLibrary].Value;
    }
}
