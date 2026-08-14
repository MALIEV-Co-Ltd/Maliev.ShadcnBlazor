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
    }

    [Fact]
    public void MenusRejectInvalidVariantsAndEmptyRadioValues()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDropdownMenuItem>(p => p.Add(x => x.Variant, (ShadcnMenuItemVariant)999)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnContextMenuRadioItem>(p => p.Add(x => x.Value, " ")));
    }
}
