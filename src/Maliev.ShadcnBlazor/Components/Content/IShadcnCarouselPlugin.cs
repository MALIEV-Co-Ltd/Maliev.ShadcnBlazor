namespace Maliev.ShadcnBlazor.Components.Content;

/// <summary>Extends the package-owned carousel engine without exposing JavaScript implementation types.</summary>
public interface IShadcnCarouselPlugin : IAsyncDisposable
{
    void Initialize(IShadcnCarouselApi api);
    void Selected(int index);
}
