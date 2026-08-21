using Bunit;
using Maliev.ShadcnBlazor.Components.Navigation;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Components.DisclosureNavigation;

public sealed class TabsNavigationMenuTests : BunitContext
{
    public TabsNavigationMenuTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-disclosure-navigation.js");
        module.SetupVoid("focusById", _ => true);
        module.SetupVoid("attachKeyGuard", _ => true);
        module.SetupVoid("detachKeyGuard", _ => true);
        module.SetupVoid("focusFirstInId", _ => true);
        module.SetupVoid("attachNavigationViewport", _ => true).SetVoidResult();
        module.SetupVoid("detachNavigationViewport", _ => true).SetVoidResult();
    }

    [Fact]
    public void TabsRenderStableNativeRelationshipsAndPreserveInactivePanels()
    {
        var cut = RenderTabs("overview", forceMount: true);
        var triggers = cut.FindAll("[role='tab']");
        var panels = cut.FindAll("[role='tabpanel']");

        Assert.Equal("true", triggers[0].GetAttribute("aria-selected"));
        Assert.Equal("0", triggers[0].GetAttribute("tabindex"));
        Assert.Equal(panels[0].Id, triggers[0].GetAttribute("aria-controls"));
        Assert.Equal(triggers[0].Id, panels[0].GetAttribute("aria-labelledby"));
        Assert.True(panels[1].HasAttribute("hidden"));
        Assert.Contains("ประวัติที่ยังอยู่ใน DOM", panels[1].TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void TabsControlledClickRequestsValueAndDisabledSuppressesCallbacks()
    {
        string? requested = null;
        var cut = RenderTabs("overview", value => requested = value);
        cut.FindAll("[role='tab']")[1].Click();
        Assert.Equal("history", requested);

        requested = null;
        var disabled = RenderTabs("overview", value => requested = value, rootDisabled: true);
        disabled.Find("[role='tab']").Click();
        Assert.Null(requested);
        Assert.True(disabled.Find("[role='tab']").HasAttribute("disabled"));
    }

    [Fact]
    public void TabsExposeAutomaticManualOrientationAndLoopContracts()
    {
        var cut = RenderTabs("overview", activation: ShadcnTabsActivationMode.Manual, orientation: ShadcnTabsOrientation.Vertical);
        Assert.Equal("vertical", cut.Find("[role='tablist']").GetAttribute("aria-orientation"));
        Assert.Equal("manual", cut.Find("[data-slot='tabs']").GetAttribute("data-activation"));
        Assert.Equal("true", cut.Find("[data-slot='tabs']").GetAttribute("data-loop"));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnTabs>(p => p.Add(x => x.Orientation, (ShadcnTabsOrientation)999)));
    }

    [Fact]
    public void TabsRecoverInvalidAndDisabledSelectionsToFirstEnabledTab()
    {
        string? requested = null;
        var invalid = RenderTabs("missing", value => requested = value);
        invalid.WaitForAssertion(() => Assert.Equal("overview", requested));

        requested = null;
        var disabled = RenderTabs("overview", value => requested = value, firstDisabled: true);
        disabled.WaitForAssertion(() => Assert.Equal("history", requested));
    }

    [Fact]
    public void TabsSupportKeepMountedAndManualVerticalKeyboardContracts()
    {
        var cut = RenderTabs("overview", activation: ShadcnTabsActivationMode.Manual, orientation: ShadcnTabsOrientation.Vertical, forceMount: true);
        var inactive = cut.FindAll("[role='tabpanel']")[1];
        Assert.True(inactive.HasAttribute("hidden"));
        Assert.True(inactive.HasAttribute("inert"));
        Assert.Equal("inactive", inactive.GetAttribute("data-state"));

        cut.FindAll("[role='tab']")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal("overview", cut.Find("[role='tab'][aria-selected='true']").GetAttribute("data-value"));
        cut.FindAll("[role='tab']")[1].Click();
        Assert.Equal("history", cut.Find("[role='tab'][aria-selected='true']").GetAttribute("data-value"));
    }

    [Fact]
    public void ManualTabsMoveTheRovingTabStopWithoutActivatingTheFocusedTab()
    {
        var cut = RenderTabs(
            "overview",
            activation: ShadcnTabsActivationMode.Manual,
            orientation: ShadcnTabsOrientation.Vertical,
            forceMount: true);
        var tabs = cut.FindAll("[role='tab']");

        tabs[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        tabs = cut.FindAll("[role='tab']");

        Assert.Equal("-1", tabs[0].GetAttribute("tabindex"));
        Assert.Equal("0", tabs[1].GetAttribute("tabindex"));
        Assert.Equal("true", tabs[0].GetAttribute("aria-selected"));
        Assert.Equal("false", tabs[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void TabsListExposesDefaultAndLineVariantsToConsumers()
    {
        var cut = Render<ShadcnTabs>(parameters => parameters
            .Add(component => component.Value, "overview")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnTabsList>(0);
                builder.AddAttribute(1, nameof(ShadcnTabsList.Variant), ShadcnTabsListVariant.Line);
                builder.AddAttribute(2, nameof(ShadcnTabsList.ChildContent), (RenderFragment)(content =>
                    AddTabTrigger(content, 0, "overview", "Overview", false)));
                builder.CloseComponent();
                AddTabContent(builder, 10, "overview", "Overview panel");
            }));

        Assert.Equal("line", cut.Find("[data-slot='tabs-list']").GetAttribute("data-variant"));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnTabs>(parameters => parameters
            .Add(component => component.Value, "overview")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnTabsList>(0);
                builder.AddAttribute(1, nameof(ShadcnTabsList.Variant), (ShadcnTabsListVariant)999);
                builder.CloseComponent();
            })));
    }

    [Fact]
    public void TabsStylesPreservePinnedVerticalLineAndForcedColorTreatments()
    {
        var css = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-disclosure-navigation.css"));

        Assert.Contains(".shadcn-tabs[data-orientation=\"vertical\"] .shadcn-tabs-list { flex-direction: column;", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-tabs-list[data-variant=\"line\"]", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void NavigationMenuUsesControlledOpenStateAndExactTriggerContentSemantics()
    {
        string? requested = null;
        var cut = RenderNavigationMenu("products", value => requested = value);
        var trigger = cut.Find("[data-slot='navigation-menu-trigger']");
        var content = cut.Find("[data-slot='navigation-menu-content']");

        Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
        Assert.Equal(content.Id, trigger.GetAttribute("aria-controls"));
        Assert.Equal(trigger.Id, content.GetAttribute("aria-labelledby"));
        Assert.False(content.HasAttribute("hidden"));
        Assert.Equal("navigation-menu-viewport", content.ParentElement?.GetAttribute("data-slot"));
        Assert.Empty(cut.FindAll("[data-slot='navigation-menu-content-source'] [data-slot='navigation-menu-link']"));
        Assert.Single(cut.FindAll("[data-slot='navigation-menu-viewport']"));
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "attachNavigationViewport");
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-disclosure-navigation.js"));
        Assert.DoesNotContain("viewport.append(content)", script, StringComparison.Ordinal);
        Assert.Contains("data-side", script, StringComparison.Ordinal);
        trigger.Click();
        Assert.Null(requested);
    }

    [Fact]
    public void NavigationMenuUsesDelayedHoverAndVerticalRovingOrder()
    {
        string? requested = null;
        var hover = RenderNavigationMenu(null, value => requested = value, openDelay: 1, closeDelay: 1);
        hover.FindAll("[data-slot='navigation-menu-trigger']")[0].MouseEnter();
        hover.WaitForAssertion(() => Assert.Equal("products", requested));

        var vertical = RenderNavigationMenu(null, _ => { }, orientation: ShadcnNavigationMenuOrientation.Vertical, includeSecond: true);
        vertical.FindAll("[data-slot='navigation-menu-trigger']")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        var focus = JSInterop.Invocations.Last(invocation => invocation.Identifier == "focusById");
        Assert.Equal(vertical.FindAll("[data-slot='navigation-menu-trigger']")[1].Id, focus.Arguments[0]);
    }

    [Fact]
    public void NavigationMenuOpensContentFocusAndEscapeRestoresTrigger()
    {
        var cut = RenderNavigationMenu(null, _ => { });
        var trigger = cut.Find("[data-slot='navigation-menu-trigger']");
        trigger.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "focusFirstInId");

        cut.Render(p => p.Add(x => x.Value, "products").Add(x => x.ValueChanged, EventCallback.Factory.Create<string?>(this, _ => { })).Add(x => x.Label, "เมนูหลัก").AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnNavigationMenuList>(0);
            builder.AddAttribute(1, nameof(ShadcnNavigationMenuList.ChildContent), (RenderFragment)(list => AddNavigationItem(list, 0, "products", "ผลิตภัณฑ์")));
            builder.CloseComponent(); builder.OpenComponent<ShadcnNavigationMenuViewport>(2); builder.CloseComponent();
        }));
        cut.Find("[data-slot='navigation-menu-content']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });
        Assert.Equal(trigger.Id, JSInterop.Invocations.Last(invocation => invocation.Identifier == "focusById").Arguments[0]);
    }

    [Fact]
    public void NavigationMenuLinkCarriesCurrentSemanticsAndRequestsClose()
    {
        string? requested = "sentinel";
        var cut = RenderNavigationMenu("products", value => requested = value);
        var link = cut.WaitForElement("a[data-slot='navigation-menu-link']");
        Assert.Equal("page", link.GetAttribute("aria-current"));
        link.Click();
        Assert.Null(requested);
    }

    [Fact]
    public void NavigationMenuLetsNativeButtonActivationOwnEnterAndSpace()
    {
        string? requested = null;
        var cut = RenderNavigationMenu(null, value => requested = value);
        var trigger = cut.Find("[data-slot='navigation-menu-trigger']");

        trigger.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        Assert.Null(requested);

        trigger.Click();
        Assert.Equal("products", requested);
    }

    [Fact]
    public void NavigationMenuUncontrolledActivationRendersViewportContent()
    {
        var cut = Render<ShadcnNavigationMenu>(parameters => parameters
            .Add(component => component.Label, "Documentation")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnNavigationMenuList>(0);
                builder.AddAttribute(1, nameof(ShadcnNavigationMenuList.ChildContent), (RenderFragment)(list => AddNavigationItem(list, 0, "products", "Products")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnNavigationMenuViewport>(2);
                builder.CloseComponent();
            }));

        cut.Find("[data-slot='navigation-menu-trigger']").Click();

        Assert.Contains("ดูทั้งหมด", cut.Find("[data-slot='navigation-menu-content']").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void NavigationMenuDisabledStateDisablesTriggersAndLinks()
    {
        var cut = RenderNavigationMenu("products", _ => { }, rootDisabled: true);
        var trigger = cut.Find("[data-slot='navigation-menu-trigger']");
        var link = cut.Find("[data-slot='navigation-menu-link']");

        Assert.True(trigger.HasAttribute("disabled"));
        Assert.Equal("true", link.GetAttribute("aria-disabled"));
        Assert.Equal("-1", link.GetAttribute("tabindex"));
        Assert.Equal("true", link.GetAttribute("data-disabled"));
    }

    [Fact]
    public void NavigationMenuViewportOwnsOutsidePointerAndFocusDismissal()
    {
        var script = File.ReadAllText(Path.Combine(FindRoot(), "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-disclosure-navigation.js"));

        Assert.Contains("pointerdown", script, StringComparison.Ordinal);
        Assert.Contains("focusin", script, StringComparison.Ordinal);
        Assert.Contains("childList: true", script, StringComparison.Ordinal);
        Assert.Contains("requestAnimationFrame", script, StringComparison.Ordinal);
        Assert.Contains("CloseNavigationMenuFromOutsideAsync", script, StringComparison.Ordinal);
    }

    [Fact]
    public void NavigationMenuValidatesDurationsOrientationAndDuplicateValues()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnNavigationMenu>(p => p.Add(x => x.OpenDelay, -1)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnNavigationMenu>(p => p.Add(x => x.Orientation, (ShadcnNavigationMenuOrientation)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnNavigationMenu>(p => p.AddChildContent(builder =>
        {
            AddNavigationItem(builder, 0, "same", "One");
            AddNavigationItem(builder, 10, "same", "Two");
        })));
    }

    private IRenderedComponent<ShadcnTabs> RenderTabs(string value, Action<string>? changed = null, bool rootDisabled = false, ShadcnTabsActivationMode activation = ShadcnTabsActivationMode.Automatic, ShadcnTabsOrientation orientation = ShadcnTabsOrientation.Horizontal, bool firstDisabled = false, bool forceMount = false) => Render<ShadcnTabs>(p => p
        .Add(x => x.Value, value)
        .Add(x => x.ValueChanged, changed is null ? default : EventCallback.Factory.Create(this, changed))
        .Add(x => x.Disabled, rootDisabled)
        .Add(x => x.ActivationMode, activation)
        .Add(x => x.Orientation, orientation)
        .AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnTabsList>(0);
            builder.AddAttribute(1, nameof(ShadcnTabsList.Label), "ข้อมูลโครงการ");
            builder.AddAttribute(2, nameof(ShadcnTabsList.ChildContent), (RenderFragment)(list =>
            {
                AddTabTrigger(list, 0, "overview", "ภาพรวม", firstDisabled);
                AddTabTrigger(list, 10, "history", "ประวัติ", false);
            }));
            builder.CloseComponent();
            AddTabContent(builder, 20, "overview", "เนื้อหาภาพรวม", forceMount);
            AddTabContent(builder, 30, "history", "ประวัติที่ยังอยู่ใน DOM", forceMount);
        }));

    private IRenderedComponent<ShadcnNavigationMenu> RenderNavigationMenu(string? value, Action<string?> changed, int openDelay = 200, int closeDelay = 150, ShadcnNavigationMenuOrientation orientation = ShadcnNavigationMenuOrientation.Horizontal, bool includeSecond = false, bool rootDisabled = false) => Render<ShadcnNavigationMenu>(p => p
        .Add(x => x.Value, value)
        .Add(x => x.ValueChanged, EventCallback.Factory.Create(this, changed))
        .Add(x => x.Disabled, rootDisabled)
        .Add(x => x.OpenDelay, openDelay)
        .Add(x => x.CloseDelay, closeDelay)
        .Add(x => x.Orientation, orientation)
        .Add(x => x.Label, "เมนูหลัก")
        .AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnNavigationMenuList>(0);
            builder.AddAttribute(1, nameof(ShadcnNavigationMenuList.ChildContent), (RenderFragment)(list =>
            {
                AddNavigationItem(list, 0, "products", "ผลิตภัณฑ์");
                if (includeSecond) AddNavigationItem(list, 10, "services", "บริการ");
            }));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnNavigationMenuIndicator>(2); builder.CloseComponent();
            builder.OpenComponent<ShadcnNavigationMenuViewport>(3); builder.CloseComponent();
        }));

    private static void AddTabTrigger(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string value, string text, bool disabled)
    {
        builder.OpenComponent<ShadcnTabsTrigger>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnTabsTrigger.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnTabsTrigger.Disabled), disabled);
        builder.AddAttribute(sequence + 3, nameof(ShadcnTabsTrigger.ChildContent), (RenderFragment)(content => content.AddContent(0, text)));
        builder.CloseComponent();
    }

    private static void AddTabContent(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string value, string text, bool forceMount = false)
    {
        builder.OpenComponent<ShadcnTabsContent>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnTabsContent.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnTabsContent.ChildContent), (RenderFragment)(content => content.AddContent(0, text)));
        builder.AddAttribute(sequence + 3, nameof(ShadcnTabsContent.ForceMount), forceMount);
        builder.CloseComponent();
    }

    private static void AddNavigationItem(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder, int sequence, string value, string text)
    {
        builder.OpenComponent<ShadcnNavigationMenuItem>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnNavigationMenuItem.Value), value);
        builder.AddAttribute(sequence + 2, nameof(ShadcnNavigationMenuItem.ChildContent), (RenderFragment)(item =>
        {
            item.OpenComponent<ShadcnNavigationMenuTrigger>(0);
            item.AddAttribute(1, nameof(ShadcnNavigationMenuTrigger.ChildContent), (RenderFragment)(trigger => trigger.AddContent(0, text)));
            item.CloseComponent();
            item.OpenComponent<ShadcnNavigationMenuContent>(2);
            item.AddAttribute(3, nameof(ShadcnNavigationMenuContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnNavigationMenuLink>(0);
                content.AddAttribute(1, nameof(ShadcnNavigationMenuLink.Href), "/products");
                content.AddAttribute(2, nameof(ShadcnNavigationMenuLink.Current), true);
                content.AddAttribute(3, nameof(ShadcnNavigationMenuLink.ChildContent), (RenderFragment)(link => link.AddContent(0, "ดูทั้งหมด")));
                content.CloseComponent();
            }));
            item.CloseComponent();
        }));
        builder.CloseComponent();
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
