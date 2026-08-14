namespace Maliev.ShadcnBlazor.Components.Navigation.Sidebar;

/// <summary>Specifies the physical edge occupied by a sidebar.</summary>
public enum ShadcnSidebarSide { Left, Right }
/// <summary>Specifies sidebar surface geometry.</summary>
public enum ShadcnSidebarVariant { Sidebar, Floating, Inset }
/// <summary>Specifies desktop collapse behavior.</summary>
public enum ShadcnSidebarCollapsible { OffCanvas, Icon, None }
/// <summary>Specifies a sidebar menu button's height.</summary>
public enum ShadcnSidebarMenuButtonSize { Small, Default, Large }
/// <summary>Specifies a sidebar menu button surface.</summary>
public enum ShadcnSidebarMenuButtonVariant { Default, Outline }

/// <summary>Persists sidebar expanded state without choosing storage for the consumer.</summary>
public interface IShadcnSidebarStateStore
{
    /// <summary>Loads the expanded state.</summary>
    ValueTask<bool?> LoadAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>Saves the expanded state.</summary>
    ValueTask SaveAsync(string key, bool open, CancellationToken cancellationToken = default);
}
