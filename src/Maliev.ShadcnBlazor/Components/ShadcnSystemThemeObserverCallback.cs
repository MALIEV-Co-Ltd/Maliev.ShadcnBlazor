using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Components;

internal sealed class ShadcnSystemThemeObserverCallback(Func<bool, Task> notify)
{
    [JSInvokable]
    public Task NotifySystemDarkModeChanged(bool isDarkMode) => notify(isDarkMode);
}
