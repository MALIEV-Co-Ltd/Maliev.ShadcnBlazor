namespace Maliev.ShadcnBlazor.Components.Content;

/// <summary>Provides framework-neutral carousel control.</summary>
public interface IShadcnCarouselApi
{
    int Count { get; }
    int SelectedIndex { get; }
    ValueTask PreviousAsync();
    ValueTask NextAsync();
    ValueTask GoToAsync(int index);
    ValueTask ReinitializeAsync();
    ValueTask DispatchAsync(Func<ValueTask> callback);
}
