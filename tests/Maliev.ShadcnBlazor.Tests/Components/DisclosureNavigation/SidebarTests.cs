using Bunit;
using Maliev.ShadcnBlazor.Components.Navigation.Sidebar;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.DisclosureNavigation;

public sealed class SidebarTests : BunitContext
{
    public SidebarTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-disclosure-navigation.js");
        module.SetupVoid("attachSidebarProvider", _ => true);
        module.SetupVoid("detachSidebarProvider", _ => true);
        module.SetupVoid("attachSidebarOverlay", _ => true);
        module.SetupVoid("detachSidebarOverlay", _ => true);
    }

    [Fact]
    public void SidebarProviderAndCompositionExposeDesktopStateAndNativeLandmarks()
    {
        var cut = RenderSidebar(open: true);
        var provider = cut.Find("[data-slot='sidebar-wrapper']");
        var sidebar = cut.Find("aside[data-slot='sidebar']");

        Assert.Equal("expanded", provider.GetAttribute("data-state"));
        Assert.Equal("desktop", provider.GetAttribute("data-device"));
        Assert.Equal("left", sidebar.GetAttribute("data-side"));
        Assert.Equal("floating", sidebar.GetAttribute("data-variant"));
        Assert.Equal("icon", sidebar.GetAttribute("data-collapsible"));
        Assert.Equal("เมนูพนักงาน", sidebar.GetAttribute("aria-label"));
        Assert.NotNull(cut.Find("main[data-slot='sidebar-inset']"));
        Assert.Equal("page", cut.Find("a[data-slot='sidebar-menu-button']").GetAttribute("aria-current"));
    }

    [Fact]
    public void SidebarInsetAllowsARegionLandmarkWhenNestedInsideAnApplicationMain()
    {
        var cut = Render<ShadcnSidebarInset>(parameters => parameters
            .Add(component => component.Role, "region")
            .AddUnmatched("aria-label", "Workspace content")
            .AddChildContent("Workspace"));

        var region = cut.Find("div[data-slot='sidebar-inset']");
        Assert.Equal("region", region.GetAttribute("role"));
        Assert.Equal("Workspace content", region.GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("main"));
    }

    [Fact]
    public void DesktopTriggerRequestsControlledStateAndDisabledMenuSuppressesCallbacks()
    {
        bool? requested = null;
        var calls = 0;
        var cut = RenderSidebar(open: true, changed: value => requested = value, menuClick: () => calls++);
        cut.Find("button[data-slot='sidebar-trigger']").Click();
        Assert.False(requested);
        cut.Find("button[data-testid='disabled-menu']").Click();
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task MobileStateRendersModalSheetBackdropAndEscapeContract()
    {
        var cut = RenderSidebar(open: true, mobileOpen: true);
        await cut.Instance.SetMobileAsync(true);
        cut.Render();

        var sheet = cut.Find("aside[data-mobile='true']");
        Assert.Equal("dialog", sheet.GetAttribute("role"));
        Assert.Equal("true", sheet.GetAttribute("aria-modal"));
        Assert.NotNull(cut.Find("button[data-slot='sidebar-backdrop']"));
        Assert.Equal("ปิดเมนู", cut.Find("button[data-slot='sidebar-backdrop']").GetAttribute("aria-label"));
    }

    [Fact]
    public void SidebarValidatesEnumsWidthsShortcutAndPersistencePair()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSidebar>(p => p.Add(x => x.Side, (ShadcnSidebarSide)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSidebarProvider>(p => p.Add(x => x.Width, "bad; color:red")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSidebarProvider>(p => p.Add(x => x.Shortcut, "bb")));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSidebarProvider>(p => p.Add(x => x.StateKey, "saved").Add(x => x.StateStore, null)));
    }

    [Fact]
    public void SidebarMenuButtonSupportsLinkButtonSizesTooltipAndActiveState()
    {
        var link = Render<ShadcnSidebarMenuButton>(p => p.Add(x => x.Href, "/orders").Add(x => x.Active, true).Add(x => x.Tooltip, "ใบสั่งซื้อ").Add(x => x.Size, ShadcnSidebarMenuButtonSize.Large).AddChildContent("Orders"));
        Assert.Equal("a", link.Find("[data-slot='sidebar-menu-button']").LocalName);
        Assert.Equal("lg", link.Find("a").GetAttribute("data-size"));
        Assert.Equal("ใบสั่งซื้อ", link.Find("a").GetAttribute("title"));
        Assert.Equal("tooltip", link.Find("[role='tooltip']").GetAttribute("role"));
        Assert.Equal(link.Find("[role='tooltip']").Id, link.Find("a").GetAttribute("aria-describedby"));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSidebarMenuButton>(p => p.Add(x => x.Size, (ShadcnSidebarMenuButtonSize)999)));
    }

    [Fact]
    public void SidebarNoneModeCannotToggleAndMenuButtonExposesVariants()
    {
        bool? requested = null;
        var cut = Render<ShadcnSidebarProvider>(p => p.Add(x => x.Open, true).Add(x => x.OpenChanged, EventCallback.Factory.Create<bool>(this, value => requested = value)).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnSidebar>(0); builder.AddAttribute(1, nameof(ShadcnSidebar.Collapsible), ShadcnSidebarCollapsible.None); builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebarTrigger>(2); builder.CloseComponent();
        }));
        var trigger = cut.Find("[data-slot='sidebar-trigger']");
        Assert.True(trigger.HasAttribute("disabled"));
        trigger.Click(); Assert.Null(requested);

        var outlined = Render<ShadcnSidebarMenuButton>(p => p.Add(x => x.Variant, ShadcnSidebarMenuButtonVariant.Outline).AddChildContent("Outlined"));
        Assert.Equal("outline", outlined.Find("[data-slot='sidebar-menu-button']").GetAttribute("data-variant"));
    }

    [Fact]
    public void SidebarTriggersResolveCollapsibilityByTypedTargetId()
    {
        bool? requested = null;
        var cut = Render<ShadcnSidebarProvider>(p => p.Add(x => x.Open, true).Add(x => x.OpenChanged, EventCallback.Factory.Create<bool>(this, value => requested = value)).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnSidebar>(0); builder.AddAttribute(1, nameof(ShadcnSidebar.Id), "fixed"); builder.AddAttribute(2, nameof(ShadcnSidebar.Collapsible), ShadcnSidebarCollapsible.None); builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebar>(10); builder.AddAttribute(11, nameof(ShadcnSidebar.Id), "tools"); builder.AddAttribute(12, nameof(ShadcnSidebar.Collapsible), ShadcnSidebarCollapsible.Icon); builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebarTrigger>(20); builder.AddAttribute(21, nameof(ShadcnSidebarTrigger.TargetId), "fixed"); builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebarTrigger>(30); builder.AddAttribute(31, nameof(ShadcnSidebarTrigger.TargetId), "tools"); builder.CloseComponent();
        }));

        var triggers = cut.FindAll("[data-slot='sidebar-trigger']");
        Assert.True(triggers[0].HasAttribute("disabled"));
        Assert.False(triggers[1].HasAttribute("disabled"));
        triggers[1].Click();
        Assert.False(requested);
    }

    [Fact]
    public void SidebarRejectsDuplicateTargetIds()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnSidebarProvider>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnSidebar>(0); builder.AddAttribute(1, nameof(ShadcnSidebar.Id), "duplicate"); builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebar>(10); builder.AddAttribute(11, nameof(ShadcnSidebar.Id), "duplicate"); builder.CloseComponent();
        })));
    }

    [Fact]
    public void SidebarOverlayScriptInertsBackgroundAndRestoresPriorState()
    {
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-disclosure-navigation.js"));
        Assert.Contains("element.inert = true", script, StringComparison.Ordinal);
        Assert.Contains("element.inert = inert", script, StringComparison.Ordinal);
        Assert.Contains("while (branch && branch !== document.body)", script, StringComparison.Ordinal);
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-disclosure-navigation.css"));
        Assert.Contains("left: 0", css, StringComparison.Ordinal);
        Assert.Contains("right: 0", css, StringComparison.Ordinal);
    }

    private IRenderedComponent<ShadcnSidebarProvider> RenderSidebar(bool open, Action<bool>? changed = null, bool mobileOpen = false, Action? menuClick = null) => Render<ShadcnSidebarProvider>(p => p
        .Add(x => x.Open, open)
        .Add(x => x.OpenChanged, changed is null ? default : EventCallback.Factory.Create(this, changed))
        .Add(x => x.MobileOpen, mobileOpen)
        .Add(x => x.CloseLabel, "ปิดเมนู")
        .AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnSidebar>(0);
            builder.AddAttribute(1, nameof(ShadcnSidebar.Label), "เมนูพนักงาน");
            builder.AddAttribute(2, nameof(ShadcnSidebar.Variant), ShadcnSidebarVariant.Floating);
            builder.AddAttribute(3, nameof(ShadcnSidebar.Collapsible), ShadcnSidebarCollapsible.Icon);
            builder.AddAttribute(4, nameof(ShadcnSidebar.ChildContent), (RenderFragment)(sidebar =>
            {
                sidebar.OpenComponent<ShadcnSidebarHeader>(0); sidebar.AddAttribute(1, nameof(ShadcnSidebarHeader.ChildContent), (RenderFragment)(header => header.AddContent(0, "MALIEV"))); sidebar.CloseComponent();
                sidebar.OpenComponent<ShadcnSidebarContent>(2); sidebar.AddAttribute(3, nameof(ShadcnSidebarContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnSidebarGroup>(0); content.AddAttribute(1, nameof(ShadcnSidebarGroup.ChildContent), (RenderFragment)(group =>
                    {
                        group.OpenComponent<ShadcnSidebarGroupLabel>(0); group.AddAttribute(1, nameof(ShadcnSidebarGroupLabel.ChildContent), (RenderFragment)(label => label.AddContent(0, "งาน"))); group.CloseComponent();
                        group.OpenComponent<ShadcnSidebarGroupContent>(2); group.AddAttribute(3, nameof(ShadcnSidebarGroupContent.ChildContent), (RenderFragment)(groupContent =>
                        {
                            groupContent.OpenComponent<ShadcnSidebarMenu>(0); groupContent.AddAttribute(1, nameof(ShadcnSidebarMenu.ChildContent), (RenderFragment)(menu =>
                            {
                                menu.OpenComponent<ShadcnSidebarMenuItem>(0); menu.AddAttribute(1, nameof(ShadcnSidebarMenuItem.ChildContent), (RenderFragment)(item => { item.OpenComponent<ShadcnSidebarMenuButton>(0); item.AddAttribute(1, nameof(ShadcnSidebarMenuButton.Href), "/orders"); item.AddAttribute(2, nameof(ShadcnSidebarMenuButton.Active), true); item.AddAttribute(3, nameof(ShadcnSidebarMenuButton.ChildContent), (RenderFragment)(button => button.AddContent(0, "Orders"))); item.CloseComponent(); })); menu.CloseComponent();
                                menu.OpenComponent<ShadcnSidebarMenuItem>(2); menu.AddAttribute(3, nameof(ShadcnSidebarMenuItem.ChildContent), (RenderFragment)(item => { item.OpenComponent<ShadcnSidebarMenuButton>(0); item.AddAttribute(1, nameof(ShadcnSidebarMenuButton.Disabled), true); item.AddAttribute(2, "data-testid", "disabled-menu"); item.AddAttribute(3, nameof(ShadcnSidebarMenuButton.OnClick), EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, _ => (menuClick ?? (() => { }))())); item.CloseComponent(); })); menu.CloseComponent();
                            })); groupContent.CloseComponent();
                        })); group.CloseComponent();
                    })); content.CloseComponent();
                })); sidebar.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebarTrigger>(10); builder.CloseComponent();
            builder.OpenComponent<ShadcnSidebarInset>(20); builder.AddAttribute(21, nameof(ShadcnSidebarInset.ChildContent), (RenderFragment)(main => main.AddContent(0, "Workspace"))); builder.CloseComponent();
        }));

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
