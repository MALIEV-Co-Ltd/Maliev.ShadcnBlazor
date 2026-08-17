namespace Maliev.ShadcnBlazor.Components.Content;

internal sealed class ShadcnAvatarContext(Func<bool, Task> imageStateChanged)
{
    private ShadcnAvatarImageState _imageState = ShadcnAvatarImageState.Loading;

    public bool ImageLoaded => _imageState == ShadcnAvatarImageState.Loaded;

    public string ImageState => _imageState switch
    {
        ShadcnAvatarImageState.Loaded => "loaded",
        ShadcnAvatarImageState.Error => "error",
        _ => "loading"
    };

    public async Task SetImageLoadedAsync(bool value)
    {
        _imageState = value ? ShadcnAvatarImageState.Loaded : ShadcnAvatarImageState.Error;
        await imageStateChanged(value);
    }

    public void ResetImage() => _imageState = ShadcnAvatarImageState.Loading;
}

internal enum ShadcnAvatarImageState
{
    Loading,
    Loaded,
    Error
}
