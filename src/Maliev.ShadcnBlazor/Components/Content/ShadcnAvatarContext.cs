namespace Maliev.ShadcnBlazor.Components.Content;

internal sealed class ShadcnAvatarContext(Func<bool, Task> imageStateChanged)
{
    public bool ImageLoaded { get; private set; }

    public async Task SetImageLoadedAsync(bool value)
    {
        ImageLoaded = value;
        await imageStateChanged(value);
    }

    public void ResetImage() => ImageLoaded = false;
}
