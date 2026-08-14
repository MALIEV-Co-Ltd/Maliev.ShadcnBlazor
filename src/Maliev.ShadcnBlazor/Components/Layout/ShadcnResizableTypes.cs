namespace Maliev.ShadcnBlazor.Components.Layout;
/// <summary>Specifies the axis along which resizable panels are arranged.</summary>
public enum ShadcnResizableDirection { Horizontal, Vertical }

/// <summary>Persists percentage-based resizable layouts without coupling the package to browser storage.</summary>
public interface IShadcnResizableStateStore
{
    /// <summary>Loads a previously saved layout.</summary>
    ValueTask<IReadOnlyList<double>?> LoadAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>Saves a normalized layout.</summary>
    ValueTask SaveAsync(string key, IReadOnlyList<double> sizes, CancellationToken cancellationToken = default);
}
