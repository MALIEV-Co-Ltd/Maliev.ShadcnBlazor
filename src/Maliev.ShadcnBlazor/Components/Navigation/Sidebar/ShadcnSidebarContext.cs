namespace Maliev.ShadcnBlazor.Components.Navigation.Sidebar;

internal sealed class ShadcnSidebarContext(ShadcnSidebarProvider owner)
{
    internal ShadcnSidebarProvider Owner { get; } = owner;
    internal bool IsMobile => Owner.IsMobile;
    internal bool Open => Owner.EffectiveOpen;
    internal bool MobileOpen => Owner.EffectiveMobileOpen;
}
