using Maliev.ShadcnBlazor.Theming;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public interface IThemeStudioStorage
{
    ValueTask<ThemeStudioStorageResult> LoadAsync();
    ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnTheme theme);
}

public sealed record ThemeStudioStorageResult(bool Succeeded, ShadcnTheme? Theme, string? Diagnostic)
{
    public static ThemeStudioStorageResult Success(ShadcnTheme? theme) => new(true, theme, null);
    public static ThemeStudioStorageResult Failure(string diagnostic) => new(false, null, diagnostic);
}

public sealed class ThemeStudioStorage(IJSRuntime jsRuntime) : IThemeStudioStorage
{
    public const string StorageKey = "maliev.shadcn.theme-studio.v1";

    public async ValueTask<ThemeStudioStorageResult> LoadAsync()
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
                return ThemeStudioStorageResult.Success(null);

            return ThemeStudioStorageResult.Success(ShadcnThemeSerializer.Deserialize(json));
        }
        catch (Exception exception) when (exception is JSException or InvalidOperationException or FormatException or System.Text.Json.JsonException or NotSupportedException)
        {
            return ThemeStudioStorageResult.Failure($"Stored theme could not be restored: {exception.Message}");
        }
    }

    public async ValueTask<ThemeStudioStorageResult> SaveAsync(ShadcnTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        try
        {
            var json = ShadcnThemeSerializer.Serialize(theme);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            return ThemeStudioStorageResult.Success(theme);
        }
        catch (Exception exception) when (exception is JSException or InvalidOperationException or FormatException)
        {
            return ThemeStudioStorageResult.Failure($"Theme could not be saved locally: {exception.Message}");
        }
    }
}
