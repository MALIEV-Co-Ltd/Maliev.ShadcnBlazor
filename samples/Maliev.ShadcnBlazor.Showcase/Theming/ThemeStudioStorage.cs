using Maliev.ShadcnBlazor.Theming;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public interface IThemeStudioStorage
{
    ValueTask<ThemeStudioStorageResult> LoadAsync();
    ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document);
}

public sealed record ThemeStudioStorageResult(bool Succeeded, ShadcnThemeDocument? Document, string? Diagnostic)
{
    public static ThemeStudioStorageResult Success(ShadcnThemeDocument? document) => new(true, document, null);
    public static ThemeStudioStorageResult Failure(string diagnostic) => new(false, null, diagnostic);
}

public sealed class ThemeStudioStorage(IJSRuntime jsRuntime) : IThemeStudioStorage
{
    public const string StorageKey = "maliev.shadcn.theme-studio.document.v2";
    public const string LegacyStorageKey = "maliev.shadcn.theme-studio.v1";

    public async ValueTask<ThemeStudioStorageResult> LoadAsync()
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            var migrated = false;
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", LegacyStorageKey);
                migrated = !string.IsNullOrWhiteSpace(json);
            }
            if (string.IsNullOrWhiteSpace(json))
                return ThemeStudioStorageResult.Success(null);

            var document = ShadcnThemeDocumentSerializer.Deserialize(json);
            if (migrated)
            {
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, ShadcnThemeDocumentSerializer.Serialize(document));
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", LegacyStorageKey);
            }
            return ThemeStudioStorageResult.Success(document);
        }
        catch (Exception exception) when (exception is JSException or InvalidOperationException or FormatException or System.Text.Json.JsonException or NotSupportedException)
        {
            return ThemeStudioStorageResult.Failure($"Stored theme could not be restored: {exception.Message}");
        }
    }

    public async ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnThemeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        try
        {
            var json = ShadcnThemeDocumentSerializer.Serialize(document);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            return ThemeStudioStorageResult.Success(document);
        }
        catch (Exception exception) when (exception is JSException or InvalidOperationException or FormatException)
        {
            return ThemeStudioStorageResult.Failure($"Theme could not be saved locally: {exception.Message}");
        }
    }
}
