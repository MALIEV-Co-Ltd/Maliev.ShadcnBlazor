using Bunit;
using Maliev.ShadcnBlazor.Components.Overlays;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Maliev.ShadcnBlazor.Tests.Components.OverlaysMenus;

public sealed class MenuTests : BunitContext
{
    public MenuTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-overlays-menus.js");
        module.SetupVoid("attachPositioned", _ => true);
        module.SetupVoid("detachPositioned", _ => true);
        module.SetupVoid("attachMenu", _ => true);
        module.SetupVoid("detachMenu", _ => true);
        module.SetupVoid("attachContextMenuTrigger", _ => true);
        module.SetupVoid("detachContextMenuTrigger", _ => true);
        module.SetupVoid("attachContextMenuSubmenu", _ => true);
        module.SetupVoid("detachContextMenuSubmenu", _ => true);
    }

    [Fact]
    public void DropdownMenuRendersGroupsItemsSelectionAndSubmenuSemantics()
    {
        var checkedChanges = new List<bool>();
        var radioChanges = new List<string?>();
        var cut = Render<ShadcnDropdownMenu>(p => p.Add(x => x.Open, true).AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnDropdownMenuTrigger>(0);
            builder.AddAttribute(1, nameof(ShadcnDropdownMenuTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "เปิดเมนู")));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnDropdownMenuContent>(2);
            builder.AddAttribute(3, nameof(ShadcnDropdownMenuContent.ChildContent), (RenderFragment)(menu =>
            {
                menu.OpenComponent<ShadcnDropdownMenuGroup>(0);
                menu.AddAttribute(1, nameof(ShadcnDropdownMenuGroup.ChildContent), (RenderFragment)(group =>
                {
                    group.OpenComponent<ShadcnDropdownMenuLabel>(0); group.AddAttribute(1, nameof(ShadcnDropdownMenuLabel.ChildContent), (RenderFragment)(label => label.AddContent(0, "บัญชี"))); group.CloseComponent();
                    group.OpenComponent<ShadcnDropdownMenuItem>(2); group.AddAttribute(3, nameof(ShadcnDropdownMenuItem.TextValue), "โปรไฟล์"); group.AddAttribute(4, nameof(ShadcnDropdownMenuItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "โปรไฟล์"))); group.CloseComponent();
                    group.OpenComponent<ShadcnDropdownMenuItem>(5); group.AddAttribute(6, nameof(ShadcnDropdownMenuItem.Disabled), true); group.AddAttribute(7, nameof(ShadcnDropdownMenuItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "ปิดใช้งาน"))); group.CloseComponent();
                })); menu.CloseComponent();
                menu.OpenComponent<ShadcnDropdownMenuSeparator>(2); menu.CloseComponent();
                menu.OpenComponent<ShadcnDropdownMenuCheckboxItem>(3); menu.AddAttribute(4, nameof(ShadcnDropdownMenuCheckboxItem.Checked), true); menu.AddAttribute(5, nameof(ShadcnDropdownMenuCheckboxItem.CheckedChanged), EventCallback.Factory.Create<bool>(this, value => checkedChanges.Add(value))); menu.AddAttribute(6, nameof(ShadcnDropdownMenuCheckboxItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "แถบสถานะ"))); menu.CloseComponent();
                menu.OpenComponent<ShadcnDropdownMenuRadioGroup>(7); menu.AddAttribute(8, nameof(ShadcnDropdownMenuRadioGroup.Value), "compact"); menu.AddAttribute(9, nameof(ShadcnDropdownMenuRadioGroup.ValueChanged), EventCallback.Factory.Create<string?>(this, value => radioChanges.Add(value))); menu.AddAttribute(10, nameof(ShadcnDropdownMenuRadioGroup.ChildContent), (RenderFragment)(radio => { radio.OpenComponent<ShadcnDropdownMenuRadioItem>(0); radio.AddAttribute(1, nameof(ShadcnDropdownMenuRadioItem.Value), "compact"); radio.AddAttribute(2, nameof(ShadcnDropdownMenuRadioItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "กะทัดรัด"))); radio.CloseComponent(); radio.OpenComponent<ShadcnDropdownMenuRadioItem>(3); radio.AddAttribute(4, nameof(ShadcnDropdownMenuRadioItem.Value), "comfortable"); radio.AddAttribute(5, nameof(ShadcnDropdownMenuRadioItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "สบาย"))); radio.CloseComponent(); })); menu.CloseComponent();
                menu.OpenComponent<ShadcnDropdownMenuSub>(11); menu.AddAttribute(12, nameof(ShadcnDropdownMenuSub.ChildContent), (RenderFragment)(sub => { sub.OpenComponent<ShadcnDropdownMenuSubTrigger>(0); sub.AddAttribute(1, nameof(ShadcnDropdownMenuSubTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "เพิ่มเติม"))); sub.CloseComponent(); sub.OpenComponent<ShadcnDropdownMenuSubContent>(2); sub.AddAttribute(3, nameof(ShadcnDropdownMenuSubContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "ส่งออก"))); sub.CloseComponent(); })); menu.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        Assert.Equal("menu", cut.Find("[data-slot='dropdown-menu-content']").GetAttribute("role"));
        Assert.Equal("menuitem", cut.Find("[data-slot='dropdown-menu-item']").GetAttribute("role"));
        Assert.Equal("true", cut.Find("[data-slot='dropdown-menu-item'][data-disabled='true']").GetAttribute("aria-disabled"));
        Assert.Equal("true", cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("aria-checked"));
        Assert.Equal("true", cut.Find("[data-slot='dropdown-menu-radio-item']").GetAttribute("aria-checked"));
        Assert.Equal("menu", cut.Find("[data-slot='dropdown-menu-sub-trigger']").GetAttribute("aria-haspopup"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").Click();
        cut.FindAll("[data-slot='dropdown-menu-radio-item']")[1].Click();
        Assert.Equal([false], checkedChanges);
        Assert.Equal(["comfortable"], radioChanges);
    }

    [Fact]
    public void ContextMenuOpensAtPointerCoordinatesAndSupportsKeyboardTrigger()
    {
        var cut = Render<ShadcnContextMenu>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ShadcnContextMenuTrigger>(0); builder.AddAttribute(1, nameof(ShadcnContextMenuTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "คลิกขวาที่นี่"))); builder.CloseComponent();
            builder.OpenComponent<ShadcnContextMenuContent>(2); builder.AddAttribute(3, nameof(ShadcnContextMenuContent.ChildContent), (RenderFragment)(menu => { menu.OpenComponent<ShadcnContextMenuItem>(0); menu.AddAttribute(1, nameof(ShadcnContextMenuItem.Variant), ShadcnMenuItemVariant.Destructive); menu.AddAttribute(2, nameof(ShadcnContextMenuItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "ลบ"))); menu.CloseComponent(); })); builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-trigger']").ContextMenu(new MouseEventArgs { ClientX = 125, ClientY = 240 });
        var content = cut.Find("[data-slot='context-menu-content']");
        Assert.Equal("125", content.GetAttribute("data-anchor-x"));
        Assert.Equal("240", content.GetAttribute("data-anchor-y"));
        Assert.Equal("destructive", cut.Find("[data-slot='context-menu-item']").GetAttribute("data-variant"));

        cut.Find("[data-slot='context-menu-trigger']").ContextMenu(new MouseEventArgs { ClientX = 520, ClientY = 360 });
        content = cut.Find("[data-slot='context-menu-content']");
        Assert.Equal("520", content.GetAttribute("data-anchor-x"));
        Assert.Equal("360", content.GetAttribute("data-anchor-y"));
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier.EndsWith("attachContextMenuTrigger", StringComparison.Ordinal));
    }

    [Fact]
    public void ContextMenuComposesCheckedRadioDisabledShortcutAndPositionedSubmenuSemantics()
    {
        var checkedChanges = new List<bool>();
        var radioChanges = new List<string?>();
        var cut = Render<ShadcnContextMenu>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<ShadcnContextMenuTrigger>(0);
                builder.AddAttribute(1, nameof(ShadcnContextMenuTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "Workspace")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnContextMenuContent>(2);
                builder.AddAttribute(3, nameof(ShadcnContextMenuContent.ChildContent), (RenderFragment)(menu =>
                {
                    menu.OpenComponent<ShadcnContextMenuCheckboxItem>(0);
                    menu.AddAttribute(1, nameof(ShadcnContextMenuCheckboxItem.Checked), true);
                    menu.AddAttribute(2, nameof(ShadcnContextMenuCheckboxItem.CheckedChanged), EventCallback.Factory.Create<bool>(this, value => checkedChanges.Add(value)));
                    menu.AddAttribute(3, nameof(ShadcnContextMenuCheckboxItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "Show archived")));
                    menu.CloseComponent();
                    menu.OpenComponent<ShadcnContextMenuRadioGroup>(4);
                    menu.AddAttribute(5, nameof(ShadcnContextMenuRadioGroup.Value), "comfortable");
                    menu.AddAttribute(6, nameof(ShadcnContextMenuRadioGroup.ValueChanged), EventCallback.Factory.Create<string?>(this, value => radioChanges.Add(value)));
                    menu.AddAttribute(7, nameof(ShadcnContextMenuRadioGroup.ChildContent), (RenderFragment)(radio =>
                    {
                        radio.OpenComponent<ShadcnContextMenuRadioItem>(0);
                        radio.AddAttribute(1, nameof(ShadcnContextMenuRadioItem.Value), "compact");
                        radio.AddAttribute(2, nameof(ShadcnContextMenuRadioItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "Compact")));
                        radio.CloseComponent();
                        radio.OpenComponent<ShadcnContextMenuRadioItem>(3);
                        radio.AddAttribute(4, nameof(ShadcnContextMenuRadioItem.Value), "comfortable");
                        radio.AddAttribute(5, nameof(ShadcnContextMenuRadioItem.ChildContent), (RenderFragment)(text => text.AddContent(0, "Comfortable")));
                        radio.CloseComponent();
                    }));
                    menu.CloseComponent();
                    menu.OpenComponent<ShadcnContextMenuItem>(8);
                    menu.AddAttribute(9, nameof(ShadcnContextMenuItem.Disabled), true);
                    menu.AddAttribute(10, nameof(ShadcnContextMenuItem.ChildContent), (RenderFragment)(item =>
                    {
                        item.AddContent(0, "Publish");
                        item.OpenComponent<ShadcnContextMenuShortcut>(1);
                        item.AddAttribute(2, nameof(ShadcnContextMenuShortcut.ChildContent), (RenderFragment)(text => text.AddContent(0, "Ctrl+P")));
                        item.CloseComponent();
                    }));
                    menu.CloseComponent();
                    menu.OpenComponent<ShadcnContextMenuSub>(11);
                    menu.AddAttribute(12, nameof(ShadcnContextMenuSub.Open), true);
                    menu.AddAttribute(13, nameof(ShadcnContextMenuSub.ChildContent), (RenderFragment)(sub =>
                    {
                        sub.OpenComponent<ShadcnContextMenuSubTrigger>(0);
                        sub.AddAttribute(1, nameof(ShadcnContextMenuSubTrigger.Disabled), false);
                        sub.AddAttribute(2, nameof(ShadcnContextMenuSubTrigger.ChildContent), (RenderFragment)(text => text.AddContent(0, "Export")));
                        sub.CloseComponent();
                        sub.OpenComponent<ShadcnContextMenuSubContent>(3);
                        sub.AddAttribute(4, nameof(ShadcnContextMenuSubContent.ChildContent), (RenderFragment)(text => text.AddContent(0, "PDF")));
                        sub.CloseComponent();
                    }));
                    menu.CloseComponent();
                }));
                builder.CloseComponent();
            })));

        Assert.Equal("true", cut.Find("[data-slot='context-menu-checkbox-item']").GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-slot='context-menu-checkbox-item-indicator'] svg"));
        Assert.Equal("true", cut.FindAll("[data-slot='context-menu-radio-item']")[1].GetAttribute("aria-checked"));
        Assert.NotNull(cut.Find("[data-slot='context-menu-radio-item-indicator'] svg"));
        Assert.Equal("true", cut.Find("[data-slot='context-menu-item'][data-disabled='true']").GetAttribute("aria-disabled"));
        Assert.Equal("true", cut.Find("[data-slot='context-menu-shortcut']").GetAttribute("aria-hidden"));
        Assert.Equal("menu", cut.Find("[data-slot='context-menu-sub-trigger']").GetAttribute("aria-haspopup"));
        Assert.NotNull(cut.Find("[data-slot='context-menu-sub-trigger-icon']"));
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier.EndsWith("attachContextMenuSubmenu", StringComparison.Ordinal));

        cut.Find("[data-slot='context-menu-checkbox-item']").Click();
        cut.FindAll("[data-slot='context-menu-radio-item']")[0].Click();
        Assert.Equal([false], checkedChanges);
        Assert.Equal(["compact"], radioChanges);
    }

    [Fact]
    public void MenusRejectInvalidVariantsAndEmptyRadioValues()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDropdownMenuItem>(p => p.Add(x => x.Variant, (ShadcnMenuItemVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnContextMenuRadioItem>(p => p.Add(x => x.Value, " ")));
    }

    [Fact]
    public void ContextMenuAssetsCoverKeyboardCollisionRtlMotionAndForcedColors()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-overlays-menus.js"));
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-overlays-menus.css"));

        Assert.Contains("attachContextMenuTrigger", script, StringComparison.Ordinal);
        Assert.Contains("event.key !== 'ContextMenu'", script, StringComparison.Ordinal);
        Assert.Contains("event.shiftKey && event.key === 'F10'", script, StringComparison.Ordinal);
        Assert.Contains("attachContextMenuSubmenu", script, StringComparison.Ordinal);
        Assert.Contains(":dir(rtl) [data-slot=\"context-menu-sub-trigger-icon\"]", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-context-menu-content", css, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
