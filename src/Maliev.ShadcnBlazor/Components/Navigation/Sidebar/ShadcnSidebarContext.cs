namespace Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
internal sealed record ShadcnSidebarContext(ShadcnSidebarProvider Owner)
{
    internal bool IsMobile => Owner.IsMobile;
    internal bool Open => Owner.EffectiveOpen;
    internal bool MobileOpen => Owner.EffectiveMobileOpen;
}
